// --------------------------------------------------
// CM3D2.Toolkit - ArcEntryBase.cs
// --------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace CM3D2.Toolkit.Arc.Entry
{
    /// <summary>
    ///     Base class for Arc File System Entries
    /// </summary>
    public abstract class ArcEntryBase 
    {
        private ArcFileSystem _fileSystem;
        private string _name;
        private ArcEntryBase _parent;

        /// <summary>
        ///     Entry Depth in the <see cref="ArcFileSystem"/> Tree
        /// </summary>
        public int Depth => (Parent?.Depth + 1) ?? 0;

        /// <summary>
        ///     <see cref="ArcFileSystem"/> containing this Entry
        /// </summary>
        public ArcFileSystem FileSystem => _fileSystem;

        internal virtual void Invalidate()
        {
            _fileSystem = null;
            _parent = null;
            _name = String.Empty;
            UTF16Hash = 0;
            UTF8Hash = 0;
        }

        /// <summary>
        ///     Full Name of this Entry (Parents and Self)
        /// </summary>
        public string FullName => Parent != null
                                      ? Parent.FullName + Path.DirectorySeparatorChar + Name
                                      : Name;

        /// <summary>
        ///     Name of this Entry
        /// </summary>
        public string Name

        {
            get { return _name; }
            set
            {
                _name = value;
                var preName = PreHash(_name);
                UTF8Hash = ArcFileSystem.Hasher.GetHash(preName, false);
                UTF16Hash = ArcFileSystem.Hasher.GetHash(preName, true);
            }
        }

        internal abstract string PreHash(string nameIn);

        /// <summary>
        ///     Parent <see cref="ArcEntryBase"/> on the File System Tree
        /// </summary>
        public ArcEntryBase Parent => _parent;

        internal ulong UniqueID => ArcFileSystem.Hasher.GetHash(FullName, true);

        internal ulong UTF16Hash { get; private set; }

        internal ulong UTF8Hash { get; private set; }

        /// <summary>
        ///     Creates a new Entry, pertaining to <paramref name="fileSystem" />
        /// </summary>
        /// <param name="fileSystem">File System</param>
        protected internal ArcEntryBase(ArcFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"\"{FullName}\"";
        }

        internal void SetParent(ArcEntryBase newParent)
        {
            var oldParent = (ArcDirectoryEntry) Parent;
            oldParent?.RemoveEntry(this);
            _parent = newParent;
        }
    }
}
