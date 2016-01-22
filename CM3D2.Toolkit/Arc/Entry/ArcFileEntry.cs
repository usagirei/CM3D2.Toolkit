// --------------------------------------------------
// CM3D2.Toolkit - ArcFileEntry.cs
// --------------------------------------------------

using CM3D2.Toolkit.Arc.FilePointer;

namespace CM3D2.Toolkit.Arc.Entry
{
    /// <summary>
    ///     Arc File Entry
    /// </summary>
    public class ArcFileEntry : ArcEntryBase
    {
        /// <summary>
        ///     File Pointer, Contains File Size and Data
        /// </summary>
        public FilePointerBase Pointer { get; set; }

        internal override void Invalidate()
        {
            base.Invalidate();
            Pointer = NullFilePointer.UncompressedPointer;
        }

        internal override string PreHash(string nameIn)
        {
            //return nameIn;
            return nameIn.ToLower();
        }

        /// <summary>
        ///     Creates a new File Entry, pertaining to <paramref name="fileSystem" />
        /// </summary>
        /// <param name="fileSystem">File System</param>
        internal ArcFileEntry(ArcFileSystem fileSystem) : base(fileSystem) {}
    }
}
