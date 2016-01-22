// --------------------------------------------------
// CM3D2.Toolkit - ArcFileSystem.Internals.cs
// --------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using CM3D2.Toolkit.Arc.Entry;
using CM3D2.Toolkit.Arc.FilePointer;
using CM3D2.Toolkit.Arc.LambdaHolders;
using CM3D2.Toolkit.Arc.Native;

namespace CM3D2.Toolkit.Arc
{


    public partial class ArcFileSystem
    {
        private static readonly byte[] ArcHeader =
        {
            0x77, 0x61, 0x72, 0x63, // warc
            0xFF, 0xAA, 0x45, 0xF1, // ?
            0xE8, 0x03, 0x00, 0x00, // 1000
            0x04, 0x00, 0x00, 0x00, // 4
            0x02, 0x00, 0x00, 0x00, // 2
        };

        private static readonly byte[] DirHeader =
        {
            0x20, 0x00, 0x00, 0x00, // 32
            0x10, 0x00, 0x00, 0x00, // 16
        };

        internal static DataHasher Hasher = DataHasher.GetBaseHasher();

        private Dictionary<ulong, long> CalculateHashTableOffsets(Dictionary<ulong, ulong> uuidToHash)
        {
            var dict = new Dictionary<ulong, long>();
            long offset = 0;
            CalculateHashTableOffsets_Internal(Root, dict, uuidToHash, ref offset);
            return dict;
        }

        private void CalculateHashTableOffsets_Internal(
            ArcDirectoryEntry dir,
            Dictionary<ulong, long> dict,
            Dictionary<ulong, ulong> uuidToHash,
            ref long offset)
        {
            Logger.Debug("Calculating Offsets for '{0}'", dir);

            long delta = 0;
            ArcEntryBase prev = dir.Parent;
            while (prev != null)
            {
                // Accumulate parent directory offsets
                // Offset is the number of bytes to seek in order to get to the directory header
                delta += dict[prev.UniqueID];
                prev = prev.Parent;
            }

            dict.Add(dir.UniqueID, offset - delta); // Subtract global count by accumulated offsets
            offset += 32; // Present on the Header 20 00 00 00, size of header
            offset += 16 * (dir.FileCount + dir.DirectoryCount); // Present on the Header 10 00 00 00, size of Offset + Hash
            offset += 8 * dir.Depth; // sizeof(long)

            // Directories have to be Ordered by hash, then by offset when writing
            var ordered = dir.Directories.OrderBy(entry => uuidToHash[entry.UniqueID]);
            foreach (var subDir in ordered)
                CalculateHashTableOffsets_Internal(subDir, dict, uuidToHash, ref offset);
        }

        private static bool DetectMagic_Internal(string file)
        {
            using (var fs = File.OpenRead(file))
            using (var br = new BinaryReader(fs))
            {
                var head = br.ReadBytes(20); // Arc Header
                return head.SequenceEqual(ArcHeader);
            }
        }

        private bool LoadArcFile_Internal(string arcFile, ArcDirectoryEntry target)
        {
            Logger.Debug("Loading Arc File: '{0}' into '{1}'", arcFile, target);
            Logger.Trace("Loading ARC");
            if (!HasEntry(target))
            {
                Logger.Error("Attemped to Load into another FileSytem");
                return false;
            }
            if (!File.Exists(arcFile))
            {
                Logger.Error("File '{0}' not Found", arcFile);
                return false;
            }

            using (var fs = File.OpenRead(arcFile))
            using (var br = new BinaryReader(fs))
            {
                var head = br.ReadBytes(20); // Arc Header
                if (!head.SequenceEqual(ArcHeader))
                {
                    Logger.Error("Invalid File Format");
                    return false;
                }

                var footer = br.ReadInt64(); // Footer Position
                var baseOffset = fs.Position; // Store Offset for Calculations
                fs.Position += footer; // Advance to Footer

                byte[] utf8HashData = null;
                byte[] utf16HashData = null;
                byte[] utf16NameData = null;

                while (utf8HashData == null || utf16HashData == null || utf16NameData == null)
                {
                    var blockType = br.ReadInt32();
                    var blockSize = br.ReadInt64();
                    switch (blockType)
                    {
                        case 0:
                            if (utf16HashData != null)
                            {
                                Logger.Error("Duplicate UTF16 Data Block");
                                return false;
                            }
                            utf16HashData = br.ReadBytes((int) blockSize);
                            break;
                        case 1:
                            if (utf8HashData != null)
                            {
                                Logger.Error("Duplicate UTF8 Data Block");
                                return false;
                            }
                            utf8HashData = br.ReadBytes((int) blockSize);
                            break;
                        case 3:
                            if (utf16NameData != null)
                            {
                                Logger.Error("Duplicate Name Data Block");
                                return false;
                            }
                            FilePointerBase afp = new ArcFilePointer(arcFile, fs.Position);
                            if (afp.Compressed)
                                afp = afp.Decompress();
                            utf16NameData = afp.Data;
                            break;
                        default:
                        {
                            Logger.Error("Unknown Footer Data Block");
                            return false;
                        }
                    }
                }

                var utf8Footer = NativeUtil.ReadHashTable(new MemoryStream(utf8HashData));
                var utf16Footer = NativeUtil.ReadHashTable(new MemoryStream(utf16HashData));
                var nameLookup = NativeUtil.ReadNameTable(new MemoryStream(utf16NameData));

                {
                    var utf8Flat = Extensions.Flatten(utf8Footer, table => table.SubdirEntries, table => table.FileEntries)
                                             .ToArray();
                    var utf16Flat = Extensions.Flatten(utf16Footer, table => table.SubdirEntries, table => table.FileEntries)
                                              .ToArray();

                    var dummyFile = new ArcFileEntry(null);

                    var check = utf16Flat.All(e16 =>
                    {
                        var e8 = utf8Flat.First(e => e.Offset == e16.Offset);
                        var name = nameLookup[e16.Hash];
                        dummyFile.Name = name;
                        return e8.Hash == dummyFile.UTF8Hash;
                    });

                    if (!check)
                    {
                        Logger.Error("File Checksum Mismatch");
                        return false;
                    }
                }
                if (_nameNotSet)
                {
                    var rootName = nameLookup[utf16Footer.ID];
                    rootName = rootName.Substring(rootName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                    Name = rootName;
                    _nameNotSet = false;
                }
                Populate_HashTable(utf16Footer, nameLookup, arcFile, baseOffset, target);
            }
            return true;
        }

        
        private bool LoadDirectory_Internal(string path, ArcDirectoryEntry target)
        {
            Logger.Debug("Loading Directory '{0}' into '{1}'", path, target);
            if (!HasEntry(target))
            {
                Logger.Error("Attemped to Load into another FileSytem");
                return false;
            }
            if (!Directory.Exists(path))
            {
                Logger.Error("Directory not Found: '{0}'", path);
                return false;
            }

            Func<string, DirScanResult> scanFunc = null;
            scanFunc = s => new DirScanResult
            {
                Name = Path.GetFileName(s),
                Dirs = Directory.GetDirectories(s, "*", SearchOption.TopDirectoryOnly).Select(scanFunc).ToArray(),
                Files = Directory.GetFiles(s, "*", SearchOption.TopDirectoryOnly)
            };
            Func<DirScanResult, int> sumFunc = null;
            sumFunc = d => d.Files.Length + d.Dirs.Sum(sumFunc);

            var tree = scanFunc(path);
            var fileSum = sumFunc(tree);
            var addCount = 0;

            Action<DirScanResult,ArcDirectoryEntry> populateFunc = null;
            populateFunc = (d, e) =>
            {
                foreach (var file in d.Files)
                {
                    addCount++;
                    var name = Path.GetFileName(file);
                    Logger.Trace("Adding File '{0}/{1}' - '{2}'", addCount,fileSum,name);
                    LoadFile_Internal(file, e);
                }
                foreach (var dir in d.Dirs)
                {
                    var name = Path.GetFileName(dir.Name);
                    var sub = GetOrCreateDirectory_Internal(name, target, true);
                    populateFunc(dir, sub);
                }
            };

            populateFunc(tree, target);

            return true;
        }
        
        /*
        private bool LoadDirectory_Internal(string path, ArcDirectoryEntry target)
        {
            Logger.Debug("Loading Directory '{0}' into '{1}'", path, target);
            if (!HasEntry(target))
            {
                Logger.Error("Attemped to Load into another FileSytem");
                return false;
            }
            if (!Directory.Exists(path))
            {
                Logger.Error("Directory not Found: '{0}'", path);
                return false;
            }

            var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
            var dirs = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                LoadFile_Internal(file, target);
            }

            foreach (var dir in dirs)
            {
                var name = Path.GetFileName(dir);
                var sub = GetOrCreateDirectory_Internal(name, target, true);
                LoadDirectory_Internal(dir, sub);
            }

            return true;
        }
        */

        private bool LoadFile_Internal(string path, ArcDirectoryEntry target)
        {
            Logger.Debug("Loading File: '{0}' into '{1}'", path, target);
            //Logger.Info("Adding File '{0}'", Path.GetFileName(path));
            if (!HasEntry(target))
            {
                Logger.Error("Attemped to Load into another FileSytem");
                return false;
            }
            if (!File.Exists(path))
            {
                Logger.Error("File '{0}' not Found", path);
                return false;
            }

            var name = Path.GetFileName(path);
            var entry = GetOrCreateFile_Internal(name, target, true);

            entry.Pointer = new WindowsFilePointer(path);

            return true;
        }

        private void Populate_HashTable(
            FileHashTable utf16Table,
            Dictionary<ulong, string> nameLut,
            string arcFile,
            long baseOffset,
            ArcDirectoryEntry root)
        {
            Func<FileHashTable, int> sumFunc = null;
            sumFunc = table => table.Files + table.SubdirEntries.Sum(sumFunc);

            var fileSum = sumFunc(utf16Table);
            var addCount = 0;

            Action<FileHashTable, ArcDirectoryEntry> populateFunc = null;
            populateFunc = (current, parent) =>
            {
                foreach (var fileEntry in current.FileEntries)
                {
                    addCount++;
                    var name = nameLut[fileEntry.Hash];
                    var file = GetOrCreateFile_Internal(name, parent, true);
                    file.Pointer = new ArcFilePointer(arcFile, fileEntry.Offset + baseOffset);
                    Logger.Trace("Adding File '{0}/{1}' - '{2}'", addCount, fileSum, name);
                }

                foreach (var dirEntry in current.DirEntries)
                {
                    string name = nameLut[dirEntry.Hash];
                    var nextDir = GetOrCreateDirectory_Internal(name, parent, true);
                    var nextTable = current.SubdirEntries.First(table => table.ID == dirEntry.Hash);
                    populateFunc(nextTable, nextDir);
                }
            };

            populateFunc(utf16Table, root);
        }

        /*
        private void Populate_HashTable(
            FileHashTable utf16Table,
            Dictionary<ulong, string> nameLut,
            string arcFile,
            long baseOffset,
            ArcDirectoryEntry root)
        {
            Logger.Debug("Populating Arc File '{0}' from HashTable. Offset: '{1}'", arcFile, baseOffset);

            ArcDirectoryEntry parent;

            // Find Parent
            if (utf16Table.Depth == 0)
            {
                parent = root;
            }
            else
            {
                var fullTree = utf16Table.Parents.Concat(new[]
                {
                    utf16Table.ID
                }).ToArray();
                var fullTreeFallback = fullTree.Select(u => nameLut[u]).ToArray();
               
                // Find Self in Tree
                parent = root;
                for (int i = 1; i < fullTree.Length; i++)
                {
                    try
                    {
                        parent = parent.Directories.First(entry => entry.UTF16Hash == fullTree[i]);
                    }
                    catch (InvalidOperationException)
                    {
                        Logger.Warn("Checksum Mismatch. Attemping Recovery");
                        try
                        {
                            parent = parent.Directories.First(entry => entry.Name == fullTreeFallback[i]);
                        }
                        catch (InvalidOperationException)
                        {
                            Logger.Error("Recovery Failed. Check LOST.DIR");
                            parent = GetOrCreateDirectory_Internal("LOST.DIR", Root, true);
                        }
                    }
                }
            }

            // Create or Replace Files
            foreach (var fileEntry in utf16Table.FileEntries)
            {
                var name = nameLut[fileEntry.Hash];
                var existing = parent.Files.FirstOrDefault(entry => entry.UTF16Hash == fileEntry.Hash);
                if (existing == null)
                {
                    var file = GetOrCreateFile_Internal(name, parent, true);
                    file.Pointer = new ArcFilePointer(arcFile, fileEntry.Offset + baseOffset);
                }
                else
                {
                    existing.Pointer = new ArcFilePointer(arcFile, fileEntry.Offset + baseOffset);
                }
                Logger.Trace("Adding Arc-File '{0}'", name);
            }

            // Create or Skip Directories
            foreach (var dirEntry in utf16Table.DirEntries)
            {
                var existing = parent.Directories.FirstOrDefault(entry => entry.UTF16Hash == dirEntry.Hash);
                if (existing == null)
                {
                    string name = nameLut[dirEntry.Hash];
                    GetOrCreateDirectory_Internal(name, parent, true);
                }
                var nat = utf16Table.SubdirEntries.First(table => table.ID == dirEntry.Hash);
                Populate_HashTable(nat, nameLut, arcFile, baseOffset, root);
            }
        }
        */

        private bool Save_Internal(Stream stream)
        {
            Logger.Debug("Saving to Stream '{0}'", stream);
            Logger.Info("Saving ARC");
            if (!stream.CanSeek || !stream.CanWrite)
            {
                Logger.Error("Unsupported Stream: Seeking or Writing not Available");
                return false;
            }

            using (var arcWriter = new BinaryWriter(stream))
            {
                arcWriter.Write(ArcHeader);
                var footerHold = stream.Position;
                arcWriter.Write((long) 0);
                var baseOffset = stream.Position;

                // Create LUTs
                Dictionary<ulong, ulong> uuidToHash8 = new Dictionary<ulong, ulong>();
                Dictionary<ulong, ulong> uuidToHash16 = new Dictionary<ulong, ulong>();
                Logger.Debug("Generating Lookup Tables");
                uuidToHash8.Add(Root.UniqueID, Root.UTF8Hash);
                uuidToHash16.Add(Root.UniqueID, Root.UTF16Hash);
                foreach (var entry in Directories)
                {
                    uuidToHash8.Add(entry.UniqueID, entry.UTF8Hash);
                    uuidToHash16.Add(entry.UniqueID, entry.UTF16Hash);
                }
                foreach (var entry in Files)
                {
                    uuidToHash8.Add(entry.UniqueID, entry.UTF8Hash);
                    uuidToHash16.Add(entry.UniqueID, entry.UTF16Hash);
                }

                Logger.Debug("Calculating HashTable Offsets");
                Dictionary<ulong, long> dirOffsetTable8 = CalculateHashTableOffsets(uuidToHash8);
                Dictionary<ulong, long> dirOffsetTable16 = CalculateHashTableOffsets(uuidToHash16);
                Dictionary<ulong, long> fileOffsetTable = new Dictionary<ulong, long>();

                var compressGlob = CompressList.Select(Extensions.WildcardToRegex).ToList();
                var files = Files.Select(entry =>
                {
                    var compress = compressGlob.Any(pat => Regex.IsMatch(entry.Name, pat));
                    return new FileTableEntryHolder
                    {
                        Name = entry.Name,
                        Compress = compress,
                        UUID = entry.UniqueID,
                        Pointer = entry.Pointer
                    };
                }).ToList();

                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];

                    Logger.Trace("Packing File '{0}/{1}' - '{2}'", i, files.Count, file.Name);

                    FilePointerBase pointer = file.Pointer;

                    if (file.Compress && !pointer.Compressed)
                        pointer = file.Pointer.Compress();

                    fileOffsetTable.Add(file.UUID, stream.Position - baseOffset);

                    arcWriter.Write((uint) (pointer.Compressed ? 1u : 0u));
                    arcWriter.Write((uint) 0u);
                    arcWriter.Write((uint) pointer.RawSize);
                    arcWriter.Write((uint) pointer.Size);

                    arcWriter.Write(pointer.Data);

#if POINTER_CACHE
                    pointer.FreeData();
#endif
                }

                var footerPos = stream.Position;
                stream.Position = footerHold;
                arcWriter.Write((long) footerPos - baseOffset);
                stream.Position = footerPos;

                var footerStream = new MemoryStream();
                using (var footerWriter = new BinaryWriter(footerStream))
                {
                    Logger.Debug("Writing Footer 0");
                    // Footer 0 - UTF16
                    WriteHashTable(footerWriter, uuidToHash16, dirOffsetTable16, fileOffsetTable);

                    arcWriter.Write((int) 0);
                    arcWriter.Write((long) footerStream.Length);

                    footerStream.Position = 0;
                    footerStream.CopyTo(stream);
                    footerStream.SetLength(0);
                    // --------------------------

                    Logger.Debug("Writing Footer 0");
                    // Footer 1 - UTF8
                    WriteHashTable(footerWriter, uuidToHash8, dirOffsetTable8, fileOffsetTable);

                    arcWriter.Write((int) 1);
                    arcWriter.Write((long) footerStream.Length);

                    footerStream.Position = 0;
                    footerStream.CopyTo(stream);
                    footerStream.SetLength(0);
                    // --------------------------

                    Logger.Debug("Writing Footer 2");
                    // Footer 3 - Names
                    WriteNameTable(footerWriter, true);

#if RAW_FOOTER
                    var nameData = footerStream.ToArray();

                    arcWriter.Write((int)3);
                    arcWriter.Write((ulong)nameData.Length + 16); // +[Compress Junk Raw Enc] Sizes

                    arcWriter.Write((uint)0u);
                    arcWriter.Write((uint)0u);
                    arcWriter.Write((uint)nameData.Length);
                    arcWriter.Write((uint)nameData.Length);

                    arcWriter.Write(nameData);
#else
                    var nameData = footerStream.ToArray();
                    var nameDataEnc = Deflate.Compress(nameData);

                    arcWriter.Write((int) 3);
                    arcWriter.Write((ulong) nameDataEnc.Length + 16); // +[Compress Junk Raw Enc] Sizes

                    arcWriter.Write((uint) 1u);
                    arcWriter.Write((uint) 0u);
                    arcWriter.Write((uint) nameData.Length);
                    arcWriter.Write((uint) nameDataEnc.Length);

                    arcWriter.Write(nameDataEnc);
#endif
                }
            }
            Logger.Trace("Saving Complete");
            return true;
        }

        private void WriteHashTable(
            BinaryWriter writer,
            Dictionary<ulong, ulong> uuidToHash,
            Dictionary<ulong, long> dirOffsets,
            Dictionary<ulong, long> fileOffsets)
        {
            WriteHashTable_Internal(writer, dirOffsets, uuidToHash, fileOffsets, Root);
        }

        private void WriteHashTable_Internal(
            BinaryWriter writer,
            Dictionary<ulong, long> dirOffsets,
            Dictionary<ulong, ulong> uuidToHash,
            Dictionary<ulong, long> fileOffsets,
            ArcDirectoryEntry current)
        {
            Logger.Debug("Writing HashTable");

            writer.Write(DirHeader);
            writer.Write((ulong) uuidToHash[current.UniqueID]);
            writer.Write((uint) current.DirectoryCount);
            writer.Write((uint) current.FileCount);
            writer.Write((uint) current.Depth);
            writer.Write((uint) 0);

            // Write Directory Entries
            var dirs = current.Directories.OrderBy(entry => dirOffsets[entry.UniqueID]).ToList();
            foreach (var dir in dirs)
            {
                writer.Write((ulong) uuidToHash[dir.UniqueID]);
                writer.Write((ulong) dirOffsets[dir.UniqueID]);
            }

            // Write File Entries
            var files = current.Files.OrderBy(entry => uuidToHash[entry.UniqueID]).ToList();
            foreach (var file in files)
            {
                writer.Write((ulong) uuidToHash[file.UniqueID]);
                writer.Write((ulong) fileOffsets[file.UniqueID]);
            }

            // Write Parent Hashes
            ulong[] parentsHashes = new ulong[current.Depth];
            ArcEntryBase next = current.Parent;
            for (int i = 0; i < parentsHashes.Length; i++)
            {
                parentsHashes[i] = uuidToHash[next.UniqueID];
                next = next.Parent;
            }
            foreach (var parentHash in parentsHashes.Reverse())
            {
                writer.Write((ulong) parentHash);
            }

            // Write SubTables
            foreach (var dir in dirs)
            {
                WriteHashTable_Internal(writer, dirOffsets, uuidToHash, fileOffsets, dir);
            }
        }

        private void WriteNameTable(BinaryWriter bw, bool utf16)
        {
            WriteNameTable_Internal(bw, utf16);
        }

        private void WriteNameTable_Internal(BinaryWriter bw, bool utf16)
        {
            Logger.Debug("Writing Name Table");

            var fileNames = Files.Cast<ArcEntryBase>();
            var dirNames = Directories.Cast<ArcEntryBase>();

           
            var allNames = fileNames.Concat(dirNames)
                                    .Concat(new[]
                                    {
                                        Root
                                    })
                                    .DistinctBy(e => e.Name)
                                    .Select(s => new NameTableEntryHolder
                                    {
                                        Size = s.Name.Length,
                                        Bytes = Encoding.Unicode.GetBytes(s.Name),
                                        Hash = utf16 ? s.UTF16Hash : s.UTF8Hash
                                    });

            foreach (var name in allNames)
            {
                bw.Write((ulong) name.Hash);
                bw.Write((int) name.Size);
                bw.Write((byte[]) name.Bytes);
            }
        }
    }

    public partial class ArcFileSystem
    {
        private bool CopyDir_Internal(ArcDirectoryEntry sourceDir, ArcDirectoryEntry targetDir)
        {
            Logger.Debug("Copying '{0}' into '{1}'", sourceDir, targetDir);

            if (sourceDir.IsRoot && sourceDir.FileSystem.HasEntry(targetDir))
            {
                Logger.Error("Cannot Copy Root into its own File System");
                return false;
            }

            var existing = targetDir.Directories.FirstOrDefault(entry => entry.UTF16Hash == sourceDir.UTF16Hash);
            // Create Folder if not Exists
            if (existing == null)
            {
                var newDir = new ArcDirectoryEntry(this)
                {
                    Name = sourceDir.Name,
                };
                _directories.Add(newDir);
                newDir.SetParent(targetDir);
                targetDir.AddEntry(newDir);
                existing = newDir;
            }

            // Copy all Files into the existing (or new) folder
            foreach (var file in sourceDir.Files.ToList())
                if (!CopyFile_Internal(file, existing))
                    return false;

            // Copy All Directories existing (or new) folder
            foreach (var dir in sourceDir.Directories.ToList())
                if (!CopyDir_Internal(dir, existing))
                    return false;

            return true;
        }

        private bool CopyFile_Internal(ArcFileEntry sourceFile, ArcDirectoryEntry targetDir)
        {
            Logger.Debug("Copying '{0}' into '{1}'", sourceFile, targetDir);

            var existing = targetDir.Files.FirstOrDefault(entry => entry.UTF16Hash == sourceFile.UTF16Hash);
            // Delete file if Exists
            if (existing != null)
                if (!Delete_Internal(existing))
                    return false;

            // Create new File
            var newFile = new ArcFileEntry(this)
            {
                Name = sourceFile.Name,
                Pointer = sourceFile.Pointer,
            };
            _files.Add(newFile);

            // Set Parent 
            newFile.SetParent(targetDir);
            // Add to Children
            targetDir.AddEntry(newFile);

            return true;
        }

        private bool Delete_Internal(ArcFileEntry entry)
        {
            Logger.Debug("Deleting File '{0}'", entry);

            if (!HasEntry(entry))
            {
                Logger.Error("Cross FileSystem Operation not Supported");
                return false;
            }

            // Delete File
            var parent = (ArcDirectoryEntry) entry.Parent;
            parent.RemoveEntry(entry);
            _files.Remove(entry);
            entry.Invalidate();
            return true;
        }

        private bool Clear_Internal(ArcDirectoryEntry entry)
        {
            Logger.Debug("Clearing Directory '{0}'", entry);


            if (!HasEntry(entry))
            {
                Logger.Error("Cross FileSystem Operation not Supported");
                return false;
            }

            // Delete each directory
            foreach (var dir in entry.Directories.ToList())
                Delete_Internal(dir, true);
            // Delete each file
            foreach (var file in entry.Files.ToList())
                Delete_Internal(file);

            return true;
        }

        private bool Delete_Internal(ArcDirectoryEntry entry, bool recursive)
        {
            Logger.Debug("Deleting Directory '{0}' Recursively: '{1}'", entry, recursive);

            if (!HasEntry(entry))
            {
                Logger.Error("Cross FileSystem Operation not Supported");
                return false;
            }

            if (entry == Root)
            {
                Logger.Error("Cannot delete Root Directory");
                return false;
            }

            // Delete Folder if Empty or Recursive
            if (recursive)
            {

                // Delete each directory
                foreach (var dir in entry.Directories.ToList())
                    Delete_Internal(dir, true);
                // Delete each file
                foreach (var file in entry.Files.ToList())
                    Delete_Internal(file);
            }
            else
            {
                // If not empty stop
                if (entry.DirectoryCount + entry.FileCount > 0)
                {
                    Logger.Error("Directory not Empty");
                    return false;
                }
            }
            // If Empty or Recursive deleted, remove self
            var parent = (ArcDirectoryEntry) entry.Parent;
            parent.RemoveEntry(entry);
            _directories.Remove(entry);
            entry.Invalidate();
            return true;
        }

        private ArcDirectoryEntry GetOrCreateDirectory_Internal(string name, ArcDirectoryEntry parent, bool create)
        {
            Logger.Debug(create
                             ? "Creating Directory '{0}' into '{1}'"
                             : "Fetching Directory '{0}' into '{1}'",
                name,
                parent);

            var segments =
                name.Split(Path.DirectorySeparatorChar)
                    .Where(s => !String.IsNullOrEmpty(s))
                    .ToList();

            name = segments.Last();

            var dummyDir = new ArcDirectoryEntry(null)
            {
                Name = name
            };

            // Create or Navigate All Roots
            for (var i = 0; i < segments.Count - 1; i++)
            {
                var segName = segments[i];
                switch (segName)
                {
                    case "..":
                        parent = (ArcDirectoryEntry) parent.Parent;
                        break;
                    case ".":
                        continue;
                    default:
                        var subDir = parent.Directories.FirstOrDefault(e => e.UTF16Hash == dummyDir.UTF16Hash);
                        if (create)
                            parent = subDir ?? GetOrCreateDirectory_Internal(segName, parent, true);
                        else if (subDir == null)
                        {
                            Logger.Error("Directory Not Found");
                            return null;
                        }
                        break;
                }
            }

            // Return existing 
            var existing = parent.Directories.FirstOrDefault(e => e.UTF16Hash == dummyDir.UTF16Hash);
            if (existing != null)
                return existing;

            if (!create)
            {
                Logger.Error("Directory not Found");
                return null;
            }

            // Return new Directory
            var entry = new ArcDirectoryEntry(this)
            {
                Name = name
            };
            _directories.Add(entry);
            MoveDir_Internal(entry, parent);
            return entry;
        }

        private ArcFileEntry GetOrCreateFile_Internal(string path, ArcDirectoryEntry parent, bool create)
        {
            Logger.Debug(create
                             ? "Creating File '{0}' into '{1}'"
                             : "Fetching File '{0}' into '{1}'",
                path,
                parent);

            var slash = path.LastIndexOf(Path.DirectorySeparatorChar);
            var name = path.Substring(slash + 1);
            var dir = path.Remove(path.Length - name.Length);

            var dummyFile = new ArcFileEntry(null)
            {
                Name = name
            };

            // Create All Roots
            if (!string.IsNullOrEmpty(dir))
            {
                parent = GetOrCreateDirectory_Internal(dir, parent, create);
                if (parent == null)
                {
                    Logger.Error("Directory not Found");
                    return null;
                }
            }

            // Return existing 
            var existing = parent.Files.FirstOrDefault(e => e.UTF16Hash == dummyFile.UTF16Hash);
            if (existing != null)
                return existing;

            if (!create)
            {
                Logger.Error("File not Found");
                return null;
            }

            // Return new File
            var entry = new ArcFileEntry(this)
            {
                Name = name,
                Pointer = NullFilePointer.UncompressedPointer
            };
            _files.Add(entry);
            MoveFile_Internal(entry, parent);
            return entry;
        }

        private bool Merge_Internal(ArcDirectoryEntry sourceDir, ArcDirectoryEntry targetDir, bool copy)
        {
            Logger.Debug(copy
                             ? "Merging a Copy of '{0}' into '{1}'"
                             : "Merging '{0}' into '{1}'",
                sourceDir,
                targetDir);

            if (sourceDir.IsRoot && sourceDir.FileSystem.HasEntry(targetDir))
            {
                Logger.Error("Cannot Merge Root into its own File System");
                return false;
            }

            Func<ArcFileEntry, ArcDirectoryEntry, bool> fileOp;
            Func<ArcDirectoryEntry, ArcDirectoryEntry, bool> dirOp;

            // Select Operation
            if (copy)
            {
                fileOp = CopyFile_Internal;
                dirOp = CopyDir_Internal;
            }
            else
            {
                if (!sourceDir.FileSystem.HasEntry(targetDir))
                {
                    Logger.Error("Cross File System Operation not Supported");
                    return false;
                }
                fileOp = MoveFile_Internal;
                dirOp = MoveDir_Internal;
            }

            // Apply Operation on all Files
            foreach (var file in sourceDir.Files.ToList())
                if (!fileOp(file, targetDir))
                    return false;

            // Apply Operation on all Directories
            foreach (var dir in sourceDir.Directories.ToList())
                if (!dirOp(dir, targetDir))
                    return false;

            // Delete self if Move
            return copy || Delete_Internal(sourceDir, false);
        }

        private bool MoveDir_Internal(ArcDirectoryEntry sourceDir, ArcDirectoryEntry targetDir)
        {
            Logger.Debug("Moving '{0}' into '{1}'", sourceDir, targetDir);

            if (sourceDir.IsRoot)
            {
                Logger.Error("Cannot Move Root Directory");
                return false;
            }

            if (!sourceDir.FileSystem.HasEntry(targetDir))
            {
                Logger.Error("Cross FileSystem Operation not Supported");
                return false;
            }

            var existing = targetDir.Directories.FirstOrDefault(entry => entry.UTF16Hash == sourceDir.UTF16Hash);
            // If Existing Directory
            if (existing != null)
            {
                // Move all Files
                foreach (var file in sourceDir.Files.ToList())
                    if (!MoveFile_Internal(file, existing))
                        return false;

                // Move all Subdirs
                foreach (var dir in sourceDir.Directories.ToList())
                    if (!MoveDir_Internal(dir, existing))
                        return false;

                Delete_Internal(sourceDir, false);
                return true;
            }

            // Set Parent 
            sourceDir.SetParent(targetDir);
            // Add to Children
            targetDir.AddEntry(sourceDir);
            return true;
        }

        private bool MoveFile_Internal(ArcFileEntry sourceFile, ArcDirectoryEntry targetDir)
        {
            Logger.Debug("Moving '{0}' into '{1}'", sourceFile, targetDir);
            if (!sourceFile.FileSystem.HasEntry(targetDir))
            {
                Logger.Error("Cross FileSystem Operation not Supported");
                return false;
            }

            var existing = targetDir.Files.FirstOrDefault(entry => entry.UTF16Hash == sourceFile.UTF16Hash);
            // Delete Existing
            if (existing != null)
                if (!Delete_Internal(existing))
                    return false;

            // Set Parent 
            sourceFile.SetParent(targetDir);
            // Add to Children
            targetDir.AddEntry(sourceFile);
            return true;
        }

        private bool Rename_Internal(ArcEntryBase entry, string newName)
        {
            Logger.Debug("Renaming '{0}' to '{1}'", entry, newName);

            entry.Name = newName;
            return true;
        }


    }
}
