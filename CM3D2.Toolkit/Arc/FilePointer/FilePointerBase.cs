// --------------------------------------------------
// CM3D2.Toolkit - FilePointerBase.cs
// --------------------------------------------------

using System;

namespace CM3D2.Toolkit.Arc.FilePointer
{
    /// <summary>
    ///     Base Class for all File Pointers
    /// </summary>
    public abstract class FilePointerBase : IDisposable
    {
        /// <summary>
        ///     File Data is Compressed
        ///     <para />
        ///     See <see cref="Deflate" />.
        /// </summary>
        public abstract bool Compressed { get; }

        /// <summary>
        ///     File Data
        /// </summary>
        public abstract byte[] Data { get; }

        /// <summary>
        ///     Decompressed Size
        /// </summary>
        public abstract uint RawSize { get; }

        /// <summary>
        ///     Compressed Size
        /// </summary>
        public abstract uint Size { get; }

        /// <summary>
        ///     Compresses this File Pointer
        /// </summary>
        /// <remarks>Default implementation returns a <see cref="MemoryFilePointer"/></remarks>
        /// <returns>A Compressed copy of this <see cref="FilePointerBase" /></returns>
        public virtual FilePointerBase Compress()
        {
            if (Compressed)
                return this;
            var raw = Data;
            var com = Deflate.Compress(raw);
            return new MemoryFilePointer(com, raw.Length);
        }

        /// <summary>
        ///     Decompresses this File Pointer
        /// </summary>
        /// <remarks>Default implementation returns a <see cref="MemoryFilePointer"/></remarks>
        /// <returns>A Decompressed copy of this <see cref="FilePointerBase" /></returns>
        public virtual FilePointerBase Decompress()
        {
            if (!Compressed)
                return this;
            var com = Data;
            var raw = Deflate.Decompress(com);
            return new MemoryFilePointer(raw);
        }

        /// <summary>
        ///     Disposes of Used Resources
        /// </summary>
        public virtual void Dispose() {}

        #if POINTER_CACHE
        /// <summary>
        ///     Frees the Data from RAM without Disposing of the Pointer.
        /// </summary>
        /// <returns>True if Succeeded</returns>
        public virtual bool FreeData()
        {
            return false;
        }
        #endif
    }
}
