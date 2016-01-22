// --------------------------------------------------
// CM3D2.Toolkit - ArcFileSystem.FileOps.cs
// --------------------------------------------------

using System.IO;

using CM3D2.Toolkit.Arc.Entry;

namespace CM3D2.Toolkit.Arc
{
    public partial class ArcFileSystem
    {
        /// <summary>
        ///     Creates a copy of <paramref name="source" /> inside <paramref name="target " />
        /// </summary>
        /// <param name="source">Source File</param>
        /// <param name="target">Target Directory</param>
        /// <returns>True if Successful</returns>
        public bool Copy(ArcFileEntry source, ArcDirectoryEntry target)
        {
            Logger.Info("Copying File '{0}' to '{1}'", source, target);
            
            return CopyFile_Internal(source, target);
        }

        /// <summary>
        ///     Creates a copy of <paramref name="source" /> inside <paramref name="target " />
        /// </summary>
        /// <param name="source">Source Directory</param>
        /// <param name="target">Target Directory</param>
        /// <returns>True if Successful</returns>
        public bool Copy(ArcDirectoryEntry source, ArcDirectoryEntry target)
        {
            Logger.Info("Copying Directory '{0}' to '{1}'", source, target);

            return CopyDir_Internal(source, target);
        }

        /// <summary>
        ///     Creates a Directory with the specified name inside <see cref="Root" />
        /// </summary>
        /// <param name="name">Directory Name</param>
        /// <returns>The created Directory</returns>
        public ArcDirectoryEntry CreateDirectory(string name)
        {
            return CreateDirectory(name, Root);
        }

        /// <summary>
        ///     Creates a Directory with the specified name inside <paramref name="parent" />
        /// </summary>
        /// <param name="name">Directory Name</param>
        /// <param name="parent">Parent Directory</param>
        /// <returns>The created Directory</returns>
        public ArcDirectoryEntry CreateDirectory(string name, ArcDirectoryEntry parent)
        {
            Logger.Info("Creating Directory '{0}' into '{1}'", name, parent);

            return GetOrCreateDirectory_Internal(name, parent, true);
        }

        /// <summary>
        ///     Creates a File with the specified name inside <paramref name="parent" />
        /// </summary>
        /// <param name="name">File Name</param>
        /// <param name="parent">Parent Directory</param>
        /// <returns>The created File</returns>
        public ArcFileEntry CreateFile(string name, ArcDirectoryEntry parent)
        {
            Logger.Info("Creating File '{0}' into '{1}'", name, parent);

            return GetOrCreateFile_Internal(name, parent, true);
        }

        /// <summary>
        ///     Creates a File with the specified name inside <see cref="Root" />
        /// </summary>
        /// <param name="name">File Name</param>
        /// <returns>The created File</returns>
        public ArcFileEntry CreateFile(string name)
        {
            return CreateFile(name, Root);
        }

        /// <summary>
        ///     Deletes a File
        /// </summary>
        /// <param name="entry">File to Delete</param>
        /// <returns>True if Successful</returns>
        public bool Delete(ArcFileEntry entry)
        {
            Logger.Info("Deleting File '{0}'",entry);

            return Delete_Internal(entry);
        }

        /// <summary>
        ///     Deletes a Directory
        /// </summary>
        /// <param name="entry">Directory to Delete</param>
        /// <param name="recursive">True to Delete Files and Subdirectories</param>
        /// <returns>True if Successful</returns>
        public bool Delete(ArcDirectoryEntry entry, bool recursive)
        {
            if (recursive)
                Logger.Info("Deleting Directory '{0}'", entry);
            else
                Logger.Info("Deleting Directory '{0}' Recursively", entry);

            return Delete_Internal(entry, recursive);
        }

        /// <summary>
        ///     Clears the <see cref="Root"/> Directory
        /// </summary>
        /// <returns>True if Sucessful</returns>
        public bool Clear()
        {
            return Clear(Root);
        }

        /// <summary>
        ///     Clears a Directory
        /// </summary>
        /// <param name="entry">Directory to Clear</param>
        /// <returns>True if Successful</returns>
        public bool Clear(ArcDirectoryEntry entry)
        {
            Logger.Info("Clearing Directory '{0}'", entry);

            return Clear_Internal(entry);
        }

        /// <summary>
        ///     Checks if a Directory Exists inside of <paramref name="parent" />
        /// </summary>
        /// <param name="name">Directory Name</param>
        /// <param name="parent">Parent Directory</param>
        /// <returns>True if Exists</returns>
        public bool DirectoryExists(string name, ArcDirectoryEntry parent)
        {
            Logger.Info("Checking Directory '{0}' into '{1}'", name, parent);

            return GetOrCreateDirectory_Internal(name, parent, false) != null;
        }

        /// <summary>
        ///     Checks if a Directory Exists inside of <see cref="Root" />
        /// </summary>
        /// <param name="name">Directory Name</param>
        /// <returns>True if Exists</returns>
        public bool DirectoryExists(string name)
        {
            return DirectoryExists(name, Root);
        }

        /// <summary>
        ///     Checks if a File Exists inside of <paramref name="parent" />
        /// </summary>
        /// <param name="name">File Name</param>
        /// <param name="parent">Parent Directory</param>
        /// <returns>True if Exists</returns>
        public bool FileExists(string name, ArcDirectoryEntry parent)
        {
            Logger.Info("Checking File '{0}' into '{1}'", name, parent);

            return GetOrCreateFile_Internal(name, parent, false) != null;
        }

        /// <summary>
        ///     Checks if a File Exists inside of <see cref="Root" />
        /// </summary>
        /// <param name="name">File Name</param>
        /// <returns>True if Exists</returns>
        public bool FileExists(string name)
        {
            return FileExists(name, Root);
        }

        /// <summary>
        ///     Finds a Existing Directory inside <paramref name="parent" />
        /// </summary>
        /// <param name="name">Directory Name</param>
        /// <param name="parent">Parent Directory</param>
        /// <returns>The Directory or null if none found</returns>
        public ArcDirectoryEntry GetDirectory(string name, ArcDirectoryEntry parent)
        {
            Logger.Info("Fetching Directory '{0}' into '{1}'", name, parent);

            return GetOrCreateDirectory_Internal(name, parent, false);
        }

        /// <summary>
        ///     Finds a Existing Directory inside <see cref="Root" />
        /// </summary>
        /// <param name="name">Directory Name</param>
        /// <returns>The Directory or null if none found</returns>
        public ArcDirectoryEntry GetDirectory(string name)
        {
            return GetDirectory(name, Root);
        }

        /// <summary>
        ///     Finds a Existing File inside <paramref name="parent" />
        /// </summary>
        /// <param name="name">File Name</param>
        /// <param name="parent">Parent Directory</param>
        /// <returns>The Directory or null if none found</returns>
        public ArcFileEntry GetFile(string name, ArcDirectoryEntry parent)
        {
            Logger.Info("Fetching File '{0}' into '{1}'", name, parent);

            return GetOrCreateFile_Internal(name, parent, false);
        }

        /// <summary>
        ///     Finds a Existing File inside <see cref="Root" />
        /// </summary>
        /// <param name="name">File Name</param>
        /// <returns>The Directory or null if none found</returns>
        public ArcFileEntry GetFile(string name)
        {
            return GetFile(name, Root);
        }

        /// <summary>
        ///     Checks if an Arc Entry is part of this File System
        /// </summary>
        /// <param name="a">Entry A</param>
        /// <returns>True if Same FileSystem</returns>
        public bool HasEntry(ArcEntryBase a)
        {
            return a.FileSystem == Root.FileSystem;
        }

        /// <summary>
        ///     Merges a Copy of <paramref name="sourceDir" /> with <paramref name="targetDir" />
        /// </summary>
        /// <param name="sourceDir">Source Directory</param>
        /// <param name="targetDir">Target Directory</param>
        /// <returns>True if Successful</returns>
        public bool MergeCopy(ArcDirectoryEntry sourceDir, ArcDirectoryEntry targetDir)
        {
            Logger.Info("Merging a Copy of '{0}' into '{1}'", sourceDir, targetDir);

            return Merge_Internal(sourceDir, targetDir, true);
        }

        /// <summary>
        ///     Merges <paramref name="sourceDir" /> with <paramref name="targetDir" />
        /// </summary>
        /// <param name="sourceDir">Source Directory</param>
        /// <param name="targetDir">Target Directory</param>
        /// <returns>True if Successful</returns>
        public bool MergeMove(ArcDirectoryEntry sourceDir, ArcDirectoryEntry targetDir)
        {
            Logger.Info("Merging '{0}' into '{1}'", sourceDir, targetDir);

            return Merge_Internal(sourceDir, targetDir, false);
        }

        /// <summary>
        ///     Moves <parmref name="sourceFile" /> into <paramref name="targetDir" />
        /// </summary>
        /// <param name="sourceFile">Source File</param>
        /// <param name="targetDir">Target Directory</param>
        /// <returns>True if Successful</returns>
        public bool Move(ArcFileEntry sourceFile, ArcDirectoryEntry targetDir)
        {
            Logger.Info("Moving File '{0}' into '{1}'", sourceFile, targetDir);

            return MoveFile_Internal(sourceFile, targetDir);
        }

        /// <summary>
        ///     Moves <parmref name="sourceDir" /> into <paramref name="targetDir" />
        /// </summary>
        /// <param name="sourceDir">Source Directory</param>
        /// <param name="targetDir">Target Directory</param>
        /// <returns>True if Successful</returns>
        public bool Move(ArcDirectoryEntry sourceDir, ArcDirectoryEntry targetDir)
        {
            Logger.Info("Moving Directory '{0}' into '{1}'", sourceDir, targetDir);

            return MoveDir_Internal(sourceDir, targetDir);
        }

        /// <summary>
        ///     Renames a <see cref="ArcEntryBase" />
        /// </summary>
        /// <param name="entry">Entry to Rename</param>
        /// <param name="newName">New Name</param>
        /// <returns>True</returns>
        public bool Rename(ArcEntryBase entry, string newName)
        {
            Logger.Info("Renaming '{0}' to '{1}'", entry, newName);

            return Rename_Internal(entry, newName);
        }
    }
}
