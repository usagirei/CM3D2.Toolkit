// --------------------------------------------------
// CM3D2.Toolkit - NullFilePointer.cs
// --------------------------------------------------

namespace CM3D2.Toolkit.Arc.FilePointer
{
    /// <summary>
    ///     A Null, Zero-Byte File Pointer
    /// </summary>
    /// <remarks>
    ///     Has no Constructors.
    ///     <para />
    ///     See <see cref="CompressedPointer" /> and <see cref="UncompressedPointer" />
    /// </remarks>
    public sealed class NullFilePointer : FilePointerBase
    {
        /// <summary>
        ///     Static, Compressed-Tagged Null File Pointer
        /// </summary>
        public static NullFilePointer CompressedPointer;

        /// <summary>
        ///     Static, Uncompressed-Tagged Null File Pointer
        /// </summary>
        public static NullFilePointer UncompressedPointer;

        private static readonly byte[] ZeroBytes;

        /// <inheritdoc />
        public override bool Compressed { get; }

        /// <inheritdoc />
        public override byte[] Data { get; }

        /// <inheritdoc />
        public override uint RawSize { get; }

        /// <inheritdoc />
        public override uint Size { get; }

        static NullFilePointer()
        {
            ZeroBytes = new byte[0];
            UncompressedPointer = new NullFilePointer(false);
            CompressedPointer = new NullFilePointer(true);
        }

        private NullFilePointer(bool tagAsCompressed)
        {
            Compressed = tagAsCompressed;
            Data = ZeroBytes;
            RawSize = 0;
            Size = 0;
        }

        /// <summary>
        ///     Returns the corresponding <see cref="NullFilePointer" />, depending on <see cref="Compressed"/>
        /// </summary>
        /// <returns><see cref="CompressedPointer" /> or <see cref="UncompressedPointer" /> </returns>
        public override FilePointerBase Compress()
        {
            return Compressed
                       ? CompressedPointer
                       : UncompressedPointer;
        }

        /// <summary>
        ///     Returns the corresponding <see cref="NullFilePointer" />, depending on <see cref="Compressed"/>
        /// </summary>
        /// <returns><see cref="CompressedPointer" /> or <see cref="UncompressedPointer" /> </returns>
        public override FilePointerBase Decompress()
        {
            return Compressed
                       ? UncompressedPointer
                       : CompressedPointer;
        }
    }
}
