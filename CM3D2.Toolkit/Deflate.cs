// --------------------------------------------------
// CM3D2.Toolkit - Deflate.cs
// --------------------------------------------------

using System.IO;
using System.IO.Compression;

namespace CM3D2.Toolkit
{
    /// <summary>
    ///     Contains Methods to Compress and Decompress data in the DEFLATE Specification
    ///     <para />
    ///     See <a href="https://tools.ietf.org/html/rfc1951" target="_blank">RFC 1951</a>
    /// </summary>
    public static class Deflate
    {
        /// <summary>
        ///     Compresses Data
        /// </summary>
        /// <param name="input">Data to be Compressed</param>
        /// <returns>Compressed Data</returns>
        public static byte[] Compress(byte[] input)
        {
            using (var msIn = new MemoryStream(input))
            using (var msOut = new MemoryStream())
            using (var comp = new DeflateStream(msOut, CompressionMode.Compress, true))
            {
                msOut.WriteByte(0x78);
                msOut.WriteByte(0x5E);
                msIn.CopyTo(comp);
                comp.Close();
                return msOut.ToArray();
            }
        }

        /// <summary>
        ///     Decompresses Data
        /// </summary>
        /// <param name="input">Data to be Decompressed</param>
        /// <returns>Decompressed Data</returns>
        public static byte[] Decompress(byte[] input)
        {
            using (var msIn = new MemoryStream(input, 2, input.Length - 2))
            using (var msOut = new MemoryStream())
            using (var comp = new DeflateStream(msIn, CompressionMode.Decompress, true))
            {
                comp.CopyTo(msOut);
                comp.Close();
                return msOut.ToArray();
            }
        }
    }
}
