// --------------------------------------------------
// CM3D2.Toolkit - ArcDirectoryEntry.cs
// --------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace CM3D2.Toolkit.Arc.Entry
{
    /// <summary>
    ///     Arc Directory Entry
    /// </summary>
    public class ArcDirectoryEntry : ArcEntryBase
    {
        private List<ArcDirectoryEntry> _directories;
        private List<ArcFileEntry> _files;

        internal override void Invalidate()
        {
            base.Invalidate();
            _directories.Clear();
            _files.Clear();
        }

        /// <summary>
        ///     List of Sub Directories
        /// </summary>
        public IEnumerable<ArcDirectoryEntry> Directories => _directories;

        /// <summary>
        ///     Directory Count
        /// </summary>
        public int DirectoryCount => Directories.Count();

        /// <summary>
        ///     File Count
        /// </summary>
        public int FileCount => Files.Count();

        /// <summary>
        ///     List of Files
        /// </summary>
        public IEnumerable<ArcFileEntry> Files => _files;

        /// <summary>
        ///     Is the <see cref="ArcFileSystem.Root" /> of a <see cref="ArcFileSystem" />
        /// </summary>
        public bool IsRoot => Depth == 0 && FileSystem.Root == this;

        /// <summary>
        ///     Creates a new Directory Entry, pertaining to <paramref name="fileSystem" />
        /// </summary>
        /// <param name="fileSystem">File System</param>
        internal ArcDirectoryEntry(ArcFileSystem fileSystem) : base(fileSystem)
        {
            _directories = new List<ArcDirectoryEntry>();
            _files = new List<ArcFileEntry>();
        }

        internal void AddEntry(ArcEntryBase entry)
        {
            var fileEntry = entry as ArcFileEntry;
            if (fileEntry != null)
            {
                AddEntry(fileEntry);
                return;
            }

            var dirEntry = entry as ArcDirectoryEntry;
            if (dirEntry != null)
            {
                AddEntry(dirEntry);
                return;
            }
        }

        internal void AddEntry(ArcFileEntry entry)
        {
            _files.Add(entry);
        }

        internal void AddEntry(ArcDirectoryEntry entry)
        {
            _directories.Add(entry);
        }

        internal override string PreHash(string nameIn)
        {
            //return nameIn;
            return nameIn.ToLower();
        }

        internal void RemoveEntry(ArcEntryBase entry)
        {
            var fileEntry = entry as ArcFileEntry;
            if (fileEntry != null)
            {
                RemoveEntry(fileEntry);
                return;
            }

            var dirEntry = entry as ArcDirectoryEntry;
            if (dirEntry != null)
            {
                RemoveEntry(dirEntry);
                return;
            }
        }

        internal void RemoveEntry(ArcFileEntry entry)
        {
            _files.Remove(entry);
        }

        internal void RemoveEntry(ArcDirectoryEntry entry)
        {
            _directories.Remove(entry);
        }
    }
}
