using System;
using System.IO;
using System.Text;
using GameFormatUtil.Endian;

namespace GameFormatUtil.Compression
{
    /// <summary>
    /// Utility-Functions for Yaz0-Compressed Data.
    /// </summary>
    public static class Yaz0
    {
        #region Constants

        private const int DATA_START = 0x11;

        #endregion

        #region Public Methods

        /// <summary>
        /// Decodes a given Yaz0-compressed file.
        /// </summary>
        /// <param name="f">Path to Yaz0-compressed file.</param>
        /// <returns>Decoded data.</returns>
        public static byte[] Decode(string f)
        {
            if (f == null)
                throw new ArgumentNullException(nameof(f));
            if (!File.Exists(f))
                throw new FileNotFoundException($"File {f} does not exist!", f);

            return Decode(File.ReadAllBytes(f));
        }

        /// <summary>
        /// Decodes the given Yaz0-data.
        /// </summary>
        /// <remarks>Decoding-Algorythm borrowed from YAGCD.</remarks>.
        /// <param name="input">The Data to decode.</param>
        /// <returns>Decoded data.</returns>
        public static byte[] Decode(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            using (var reader = new EndianBinaryReader(new MemoryStream(input), Endian.Endian.Big))
            {
                if (IsYaz0(reader.ReadBytes(4)))
                    throw new ArgumentException($"{input} is not Yaz0-compressed data!", nameof(input));

                byte[] output = new byte[reader.ReadUInt32()];

                long srcPos = DATA_START;
                long dstPos = 0;
                uint validBitCount = 0;
                byte currentCodeByte = 0;

                while(dstPos < output.Length)
                {
                    if(validBitCount == 0)
                    {
                        currentCodeByte = input[srcPos++];
                        validBitCount = 8;
                    }

                    if((currentCodeByte & 0x80) != 0)   
                        output[dstPos++] = input[srcPos++];
                    else
                    {
                        byte byte1 = input[srcPos++];
                        byte byte2 = input[srcPos++];

                        uint dist = (uint)(((byte1 & 0x0F) << 8) | byte2);
                        uint copySrc = (uint)(dstPos - (dist + 1));
                        uint numBytes = (uint)(byte1 >> 4);

                        if (numBytes == 0)
                            numBytes = input[srcPos++] + 0x12U;
                        else
                            numBytes += 2;

                        for (int i = 0; i < numBytes; i++)
                            output[dstPos++] = output[copySrc++];
                    }

                    currentCodeByte <<= 1;
                    validBitCount -= 1;
                }
                return output;
            }
        }

        /// <summary>
        /// Checks if a given file is Yaz0-compressed.
        /// </summary>
        /// <param name="filePath">Path to (presumambly)encoded File.</param>
        /// <returns>Wether or not the file uses Yaz0-Compression.</returns>
        public static bool IsYaz0(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using (var reader = new EndianBinaryReader(new MemoryStream(data), Encoding.ASCII, Endian.Endian.Big))
                return new string(reader.ReadChars(4)) == "Yaz0";          
        }
        #endregion
    }
}
