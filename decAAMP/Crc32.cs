using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Security.Cryptography
{
    /// <summary>
    /// Computes a CRC32 Checksum.
    /// </summary>
    /// <remarks> Based on <see cref="https://sanity-free.org/12/crc32_implementation_in_csharp.html"/> </remarks>
    public static class Crc32
    {
        readonly static uint[] table = CreateTable();

        static Crc32() {}

        /// <summary>
        /// Compute the Checksum of a UTF-8 Text.
        /// </summary>
        /// <param name="text">Text to calculate.</param>
        /// <returns>Checksum</returns>
        public static uint Compute(string text)
        {
            return Compute(text, Encoding.UTF8);
        }

        /// <summary>
        /// Compute the Checksum of a Text using a specified Encoding.
        /// </summary>
        /// <param name="text">Text to calculate.</param>
        /// <param name="encoding">Text encoding.</param>
        
        public static uint Compute(string text, Encoding encoding)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            byte[] bytes = encoding.GetBytes(text);
            return Compute(bytes);
        }

        /// <summary>
        /// Compute the Checksum of a binary Buffer.
        /// </summary>
        /// <param name="bytes">Buffer to calculate.</param>
        /// <returns></returns>
        public static uint  Compute(byte[] bytes)
        {
            uint crc = 0xffffffff;
            for(int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                crc = (crc >> 8) ^ table[index];
            }
            return unchecked((~crc));
        }


        static uint[] CreateTable()
        {
            const uint POLY = 0xedb88320;
            var table = new uint[256];
            uint temp = 0;
            for(uint i = 0; i < table.Length; ++i)
            {
                temp = i;
                for(int j = 8; j > 0; --j)
                {
                    if((temp & 1) == 1)
                    {
                        temp = (uint)((temp >> 1) ^ POLY);
                    }else
                    {
                        temp >>= 1;
                    }
                }
                table[i] = temp;
            }
            return table;
        }
    }
}
