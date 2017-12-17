
using System;
using System.IO;
using System.Text;

namespace GameFormatUtil.Endian
{
    /// <summary>
    /// <see cref="BinaryWriter"/> Implementation that can write in different <see cref="Endian"/> Formats.
    /// </summary>
    public sealed class EndianBinaryWriter : BinaryWriter
    {
        #region Fields

        private static readonly bool systemLittleEndian = BitConverter.IsLittleEndian;

        #endregion

        #region Properties

        /// <summary>
        /// Current <see cref="Endian"/> this EndianBinaryWriter is using.
        /// </summary>
        public Endian currentEndian { get; set; }

        #endregion

        #region Constructors


        /// <summary>
        /// Constructor. Uses UTF-8 by default for Character-Encoding.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to wrap within this EndianBinaryWriter.</param>
        /// <param name="endian">The <see cref="Endian"/> to use when reading files.</param>
        public EndianBinaryWriter(Stream stream, Endian endian) : base(stream)
        {
            currentEndian = endian;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to wrap within this EndianBinaryWriter.</param>
        /// /// <param name="encoding"> The <see cref="Encoding"/> to use for characters.</param>
        /// <param name="endian">The <see cref="Endian"/> to use when reading files.</param>
        public EndianBinaryWriter(Stream stream, Encoding encoding, Endian endian) : base(stream, encoding)
        {
            currentEndian = endian;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to wrap within this EndianBinaryWriter.</param>
        /// /// <param name="encoding"> The <see cref="Encoding"/> to use for characters.</param>
        /// /// <param name="leaveOpen"> Whether or not to leave the <see cref="Stream"/> open after this EndianBinaryWriter is disposed.</param>
        /// <param name="endian">The <see cref="Endian"/> to use when reading files.</param>
        public EndianBinaryWriter(Stream stream, Encoding encoding, bool leaveOpen, Endian endian) : base(stream, encoding, leaveOpen)
        {
            currentEndian = endian;
        }

        #endregion

        #region Public Methods

        #region Overriders

        public override void Write(short value)
        {
            if (systemLittleEndian && currentEndian == Endian.Little || !systemLittleEndian && currentEndian == Endian.Big)
                base.Write(value);
            else
                base.Write(value.SwapBytes());
        }

        public override void Write(ushort value)
        {
            if (systemLittleEndian && currentEndian == Endian.Little || !systemLittleEndian && currentEndian == Endian.Big)
                base.Write(value);
            else
                base.Write(value.SwapBytes());
        }

        public override void Write(int value)
        {
            if (systemLittleEndian && currentEndian == Endian.Little || !systemLittleEndian && currentEndian == Endian.Big)
                base.Write(value);
            else
                base.Write(value.SwapBytes());
        }

        public override void Write(uint value)
        {
            if (systemLittleEndian && currentEndian == Endian.Little || !systemLittleEndian && currentEndian == Endian.Big)
                base.Write(value);
            else
                base.Write(value.SwapBytes());
        }

        public override void Write(long value)
        {
            if (systemLittleEndian && currentEndian == Endian.Little || !systemLittleEndian && currentEndian == Endian.Big)
                base.Write(value);
            else
                base.Write(value.SwapBytes());
        }

        public override void Write(ulong value)
        {
            if (systemLittleEndian && currentEndian == Endian.Little || !systemLittleEndian && currentEndian == Endian.Big)
                base.Write(value);
            else
                base.Write(value.SwapBytes());
        }

        public override void Write(float value)
        {
            if (systemLittleEndian && currentEndian == Endian.Little || !systemLittleEndian && currentEndian == Endian.Big)
                base.Write(value);
            else
            {
                byte[] floatBytes = BitConverter.GetBytes(value);
                Array.Reverse(floatBytes);
                base.Write(BitConverter.ToSingle(floatBytes, 0));
            }
        }

        public override void Write(double value)
        {
            if (systemLittleEndian && currentEndian == Endian.Little || !systemLittleEndian && currentEndian == Endian.Big)
                base.Write(value);
            else
            {
                byte[] doubleBytes = BitConverter.GetBytes(value);
                Array.Reverse(doubleBytes);
                base.Write(BitConverter.ToDouble(doubleBytes, 0));
            }
        }

        public override string ToString()
        {
            string endianness = (currentEndian == Endian.Little) ? "Little Endian" : "Big Endian";
            return $"EndianBinaryWriter - Endianness: {endianness}";
        }

        #endregion

        #region Write[x] Methods

        /// <summary>
        /// Writes a specific number of characters to the underlying 
        /// <see cref="Stream"/>. If <paramref name="length"/> is greater
        /// than <paramref name="str"/>'s length, the length will be
        /// padded with zeros. Similarly, if <paramref name="length"/> is
        /// smaller than <paramref name="str"/>'s length then the string
        /// will be truncated.
        /// </summary>
        /// <param name="str">String to write to the stream.</param>
        /// <param name="length">Maximum number of characters to write.</param>
        public void WriteFixedString(string str, int lenght)
        {
            if(str == null)
                throw new ArgumentException(nameof(str));
            if (lenght < 0)
                throw new ArgumentException("Cannot write a negative length String.");
            for (int i = 0; i < str.Length; i++)
                Write((i < str.Length) ? str[i] : '\0');
        }

        #endregion

        #endregion
    }
}
