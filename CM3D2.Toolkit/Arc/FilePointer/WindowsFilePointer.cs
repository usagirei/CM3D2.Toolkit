// --------------------------------------------------
// CM3D2.Toolkit - WindowsFilePointer.cs
// --------------------------------------------------

using System;
using System.IO;

namespace CM3D2.Toolkit.Arc.FilePointer
{
    /// <summary>
    ///     A Local Disk File Pointer
    ///     <para />
    ///     Holds a reference to a Local File on the Underlying Operating System
    /// </summary>
    public sealed class WindowsFilePointer : FilePointerBase
    {
        private FileInfo _fileInfo;
        private bool _disposed;
        private readonly bool _compressed = false;
#if POINTER_CACHE
        private byte[] _data;
#endif

        /// <inheritdoc />
        public override bool Compressed
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException("File Pointer Disposed"); // TODO: Custom Exceptions

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

                return Size;
            }
        }

        /// <inheritdoc />
        public override uint Size
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException("File Pointer Disposed"); // TODO: Custom Exceptions

                return (uint) _fileInfo.Length;
            }
        }

        /// <summary>
        ///     Creates a new Physical Disk File Pointer for the file given by <paramref name="filePath" />
        /// </summary>
        /// <param name="filePath">File Path</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when the File given by <paramref name="filePath" /> exceeds 4,294,967,295 bytes (4GB)
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     Thrown when the File given by <paramref name="filePath" /> does not exist
        /// </exception>
        public WindowsFilePointer(string filePath)
        {
            _fileInfo = new FileInfo(filePath);
            if (!_fileInfo.Exists)
                throw new FileNotFoundException("File not Found");
            if (_fileInfo.Length > uint.MaxValue)
                throw new ArgumentException("File is Too big", nameof(filePath));
        }

        private byte[] LoadFileData()
        {
            return File.ReadAllBytes(_fileInfo.FullName);
        }

    /// <inheritdoc />
        public override void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _fileInfo = null;
#if POINTER_CACHE
            _data = null;
#endif
        }

#if POINTER_CACHE
        
    /// <inheritdoc />
        public override bool FreeData()
        {
            _data = null;
            return true;
        }
#endif
    }
}
