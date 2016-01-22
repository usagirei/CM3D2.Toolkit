// --------------------------------------------------
// CM3D2.Toolkit - MemoryFilePointer.cs
// --------------------------------------------------

namespace CM3D2.Toolkit.Arc.FilePointer
{
    /// <summary>
    ///     A In-Memory File Pointer
    ///     <para />
    ///     Holds the Data for a File in Memory
    /// </summary>
    public sealed class MemoryFilePointer : FilePointerBase
    {
        private bool _compressed;
        private byte[] _data;
        private bool _disposed;
        private uint _rawSize;
        private uint _size;

        /// <inheritdoc />
        public override bool Compressed => _compressed;

        /// <inheritdoc />
        public override byte[] Data => _data;

        /// <inheritdoc />
        public override uint RawSize => _rawSize;

        /// <inheritdoc />
        public override uint Size => _size;

        /// <summary>
        ///     Creates a new Memory File Pointer from Uncompressed Data
        /// </summary>
        /// <param name="data">Uncompressed Data</param>
        public MemoryFilePointer(byte[] data)
        {
            _data = data;
            _rawSize = _size = (uint) data.Length;
            _compressed = false;
        }

        /// <summary>
        ///     Creates a new Memory File Pointer from Compressed Data
        ///     <para />
        ///     See <see cref="Deflate"/>
        /// </summary>
        /// <param name="data">Compressed Data</param>
        /// <param name="rawSize">Decompressed Size</param>
        public MemoryFilePointer(byte[] data, int rawSize)
        {
            _data = data;
            _size = (uint) data.Length;
            _rawSize = (uint) rawSize;
            _compressed = true;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _data = null;
            _compressed = false;
            _rawSize = 0;
            _size = 0;
        }
    }
}
