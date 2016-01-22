// --------------------------------------------------
// CM3D2.Toolkit - DataHasher.cs
// --------------------------------------------------

using System.Text;

namespace CM3D2.Toolkit
{
    /// <summary>
    ///     Data Hasher
    /// </summary>
    internal class DataHasher
    {
        /// <summary>
        ///     XOR Key
        /// </summary>
        public UInt64Ex Key { get; }

        /// <summary>
        ///     Base Seed
        /// </summary>
        public UInt64Ex Seed { get; }

        /// <summary>
        ///     Initializes an <see cref="DataHasher" /> instance with the specified Seeds and Keys
        /// </summary>
        /// <param name="seedA">Seed A</param>
        /// <param name="seedB">Seed B</param>
        /// <param name="keyA">Key A</param>
        /// <param name="keyB">Key B</param>
        public DataHasher(uint seedA, uint seedB, uint keyA, uint keyB)
        {
            Seed = new UInt64Ex
            {
                DWORD_0 = seedA,
                DWORD_1 = seedB
            };
            Key = new UInt64Ex
            {
                DWORD_0 = keyA,
                DWORD_1 = keyB
            };
        }

        /// <summary>
        ///     Creates a <see cref="DataHasher" /> for Base CM3D2 Files
        /// </summary>
        /// <returns></returns>
        public static DataHasher GetBaseHasher()
        {
            return new DataHasher(0x84222325u, 0xCBF29CE4u, 0x100u, 0x1B3u);
        }

        /// <summary>
        ///     Generates a Hash for the specified data
        /// </summary>
        /// <param name="data">Binary Data</param>
        /// <returns>Hash</returns>
        public ulong GetHash(byte[] data)
        {
            /* 
             * Optimized Hashing Method
             * Utilizes a Custom DataType to Avoid Bit-Shifting Madness
             */

            // Copies Seed and Key
            UInt64Ex seed = Seed;
            UInt64Ex key = Key;

            // Creates Temp Block
            var temp = new UInt64Ex();

            for (int i = 0; i < data.Length; i++)
            {
                //XOR Seed A with Next Byte
                seed.DWORD_0 ^= data[i];

                //Loads Current Seed
                temp.QWORD = seed.QWORD;
                //Multiplies Seed A * Key A
                temp.DWORD_0 *= key.DWORD_0;
                //Multiplies Seed B * Key B
                temp.DWORD_1 *= key.DWORD_1;

                //Resets Seed B
                seed.DWORD_1 = 0;
                //Multiplies Seed A * Key B with Carry over to Seed B
                seed.QWORD *= key.DWORD_1;
                //Add Multiplied Seeds and Keys to Seed B (Add to Carried MSB)
                seed.DWORD_1 += temp.DWORD_0;
                seed.DWORD_1 += temp.DWORD_1;
            }
            //XOR Seeed A with Seed B
            seed.DWORD_0 ^= seed.DWORD_1;

            //Return Checksum
            return seed.QWORD;
        }

        /// <summary>
        ///     Generates a Hash for the specified string
        /// </summary>
        /// <param name="data">String</param>
        /// <param name="utf16">Yse Unicode bytes</param>
        /// <returns>Hash</returns>
        public ulong GetHash(string data, bool utf16)
        {
            return utf16
                       ? GetHash(Encoding.Unicode.GetBytes(data))
                       : GetHash(Encoding.UTF8.GetBytes(data));
        }
    }
}
