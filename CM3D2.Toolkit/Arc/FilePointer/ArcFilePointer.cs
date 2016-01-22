// --------------------------------------------------
// CM3D2.Toolkit - ArcFilePointer.cs
// --------------------------------------------------

using System;
using System.IO;

namespace CM3D2.Toolkit.Arc.FilePointer
{
    /// <summary>
    ///     A Arc-File File Pointer
    ///     <para />
    ///     Holds a reference to a File inside an Arc File
    /// </summary>
    public sealed class ArcFilePointer : FilePointerBase
    {
#if POINTER_CACHE
        private byte[] _data;
#endif
        private string _arcFile;
        private bool _compressed;
        private bool _disposed;
        private bool _initialized;
        private long _offset;
        private uint _rawSize;
        private uint _size;

        /// <inheritdoc />
        public override bool Compressed
        {
            get
            {
                Initialize();
                return _compressed;
            }
        }

        /// <inheritdoc />
        public override byte[] Data
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException("File Pointer Disposed"); // TODO: Custom Exceptions

                Initialize();
#if POINTER_CACHE
                return _data ?? (_data = LoadFileData());
 #else
                return LoadFileData();
#endif
            }
        }

        /// <inheritdoc />
        public override uint RawSize
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException("File Pointer Disposed"); // TODO: Custom Exceptions

                Initialize();
                return _rawSize;
            }
        }

        /// <inheritdoc />
        public override uint Size
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException("File Pointer Disposed"); // TODO: Custom Exceptions

                Initialize();
                return _size;
            }
        }

        /// <summary>
        ///     Creates a new Arc File Pointer at the Specified Offset
        /// </summary>
        /// <param name="arcFile">Arc File</param>
        /// <param name="offset">Offset</param>
        internal ArcFilePointer(string arcFile, long offset)
        {
            _initialized = false;
            _offset = offset;
            _arcFile = arcFile;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _initialized = false;
            _arcFile = String.Empty;
            _rawSize = 0;
            _size = 0;
            _compressed = false;
            _offset = -1;
#if POINTER_CACHE
            _data = null;
#endif
        }

        private void Initialize()
        {
            if (_initialized)
                return;
            using (var fs = File.OpenRead(_arcFile))
            using (var br = new BinaryReader(fs))
            {
                fs.Position = _offset;
                _compressed = br.ReadUInt32() == 1;
                br.ReadUInt32(); // Junk Data?
                _rawSize = br.ReadUInt32();
                _size = br.ReadUInt32();
                _offset = fs.Position;
            }
            _initialized = true;
        }

        private byte[] LoadFileData()
        {
            using (var fs = File.OpenRead(_arcFile))
            {
                fs.Position = _offset;
                var data = new byte[_size];
                fs.Read(data, 0, (int) _size);
                return data;
            }
        }

#if POINTER_CACHE
    /// <inheritdoc />
        public override bool FreeData()
        {
            if (_disposed)
                return false;
            _data = null;
            return true;
        }
#endif
    }
}
