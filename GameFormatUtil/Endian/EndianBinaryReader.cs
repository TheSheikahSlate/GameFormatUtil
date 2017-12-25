using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameFormatUtil.Endian
{
    ///<summary> 
    ///<see cref="BinaryReader"/>Implementation that can read in different <see cref="Endian"/> Formats.
    ///</summary> 
    public sealed class EndianBinaryReader : BinaryReader
    {
        #region Fields

        public static readonly bool systemLittleEndian = BitConverter.IsLittleEndian;   
        
        #endregion

        #region Properties

        /// <summary>
        /// Current <see cref="Endian"/> this EndianBinaryReader is using.
        /// </summary>
        public Endian currentEndian
        {
            get;
            set;
        }
       
        #endregion

        #region Constructors

        /// <summary>
        /// Constructor. Uses UTF-8 by default for Character-Encoding.
        /// </summary>
        /// <param name="stream"> The <see cref="Stream"/> to wrap within his EndianBinaryReader. </param>
        /// <param name="endian"> The <see cref="Endian"/> to use when reading files. </param>
        public EndianBinaryReader(Stream stream, Endian endian) : base(stream)
        {
            currentEndian = endian;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream"> The <see cref="Stream"/> to wrap within this EndianBinaryReader. </param>
        /// <param name="encoding"> The <see cref="Encoding"/> to use for characters. </param>
        /// <param name="endian"> The <see cref="Endian"/> to use when reading files. </param>
        public EndianBinaryReader(Stream stream, Encoding encoding, Endian endian) : base(stream, encoding)
        {
            currentEndian = endian;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream"> The <see cref="Stream"/> to wrap within this EndianBinaryReader. </param>
        /// <param name="encoding"> The <see cref="Encoding"/> to use for characters. </param>
        /// <param name="leaveOpen"> Whether or not to leave the <see cref="Stream"/> open after this EndianBinaryReader is disposed. </param>
        /// <param name="endian"> The <see cref="Endian"/> to use when reading files. </param>
        public EndianBinaryReader(Stream stream, Encoding encoding, bool leaveOpen, Endian endian) : base(stream, encoding, leaveOpen)
        {
            currentEndian = endian;
        }

        /// <summary>
        /// Constructor. Uses UTF-8 by default for Character-Encoding.
        /// </summary>
        /// <param name="data"> A <see cref="Byte"/>-<see cref="Array"/> of data to be encapsulated. </param>
        /// <param name="endian"> The <see cref="Endian"/> to use when reading files. </param>
        public EndianBinaryReader(byte[] data, Endian endian) : base(new MemoryStream(data))
        {
            currentEndian = endian;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data"> A <see cref="Byte"/>-<see cref="Array"/> of data to be encapsulated. </param>
        /// <param name="encoding"> The <see cref="Encoding"/> to use for characters. </param>
        /// <param name="endian"> The <see cref="Endian"/> to use when reading files. </param>
        public EndianBinaryReader(byte[] data, Encoding encoding, Endian endian) : base(new MemoryStream(data), encoding)
        {
            currentEndian = endian;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data"> A <see cref="Byte"/>-<see cref="Array"/> of data to be encapsulated. </param>
        /// <param name="encoding"> The <see cref="Encoding"/> to use for characters. </param>
        /// <param name="leaveOpen"> Whether or not to leave the <see cref="Stream"/> open after this EndianBinaryReader is disposed. </param>
        /// <param name="endian"> The <see cref="Endian"/> to use when reading files. </param>
        public EndianBinaryReader(byte[] data, Encoding encoding, bool leaveOpen, Endian endian) : base(new MemoryStream(data), encoding, leaveOpen)
        {
            currentEndian = endian;
        }

        #endregion

        #region Public Methods

        #region Overrides

        public override short ReadInt16()
        {
            if (systemLittleEndian && currentEndian == Endian.Little || !systemLittleEndian && currentEndian == Endian.Big)
                return base.ReadInt16();
            return base.ReadInt16().SwapBytes();
        }

        public override ushort ReadUInt16()
        {
            if (systemLittleEndian && currentEndian == Endian.Little || !systemLittleEndian && currentEndian == Endian.Big)
                return base.ReadUInt16();
            return base.ReadUInt16().SwapBytes();
        }

        public override int ReadInt32()
        {
            if (systemLittleEndian && currentEndian == Endian.Little || !systemLittleEndian && currentEndian == Endian.Big)
                return base.ReadInt32();
            return base.ReadInt32().SwapBytes();
        }

        public override uint ReadUInt32()
        {
            if (systemLittleEndian && currentEndian == Endian.Little || !systemLittleEndian && currentEndian == Endian.Big)
                return base.ReadUInt32();
            return base.ReadUInt32().SwapBytes();
        }

        public override long ReadInt64()
        {
            if (systemLittleEndian && currentEndian == Endian.Little || !systemLittleEndian && currentEndian == Endian.Big)
                return base.ReadInt64();
            return base.ReadInt64().SwapBytes();
        }

        public override ulong ReadUInt64()
        {
            if (systemLittleEndian && currentEndian == Endian.Little || !systemLittleEndian && currentEndian == Endian.Big)
                return base.ReadUInt64();
            return base.ReadUInt64().SwapBytes();
        }

        public override float ReadSingle()
        {
            if (systemLittleEndian && currentEndian == Endian.Little || !systemLittleEndian && currentEndian == Endian.Big)
                return base.ReadSingle();

            float temp = base.ReadSingle();
            byte[] floatBytes = BitConverter.GetBytes(temp);
            Array.Reverse(floatBytes);
            return BitConverter.ToSingle(floatBytes, 0);
        }

        public override double ReadDouble()
        {
            if (systemLittleEndian && currentEndian == Endian.Little || !systemLittleEndian && currentEndian == Endian.Big)
                return base.ReadDouble();

            double temp = base.ReadDouble();
            byte[] doubleBytes = BitConverter.GetBytes(temp);
            Array.Reverse(doubleBytes);
            return BitConverter.ToDouble(doubleBytes, 0);
        }

        #region ReadString

        //TODO: Summary

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public String ReadZeroTerminatedString(Encoding encoding = null)
        {
            byte[] buffer = ReadBytesUntil(0x00);

            return encoding.GetString(buffer);
        }

        #endregion

        #region ReadCrc

        public String ReadCrc16()
        {
            byte[] hash = ReadBytes(2);
            String s = "";

            foreach (byte b in hash)
                s += b.ToString("X2").ToLower();

            return s;
        }

        public uint ReadCrc32()
        {
           return  ReadUInt32();
        }

        public String ReadCrc64()
        {
            byte[] hash = ReadBytes(8);
            String s = "";

            foreach (byte b in hash)
                s += b.ToString("X2").ToLower();

            return s;
        }

        #endregion

        public uint Read1MsbByte(uint value)
        {
            if (currentEndian == Endian.Big || !systemLittleEndian)
                return value & 0x000000FF;

            return value >> 24;
            
        }

        public uint Get3MsbBytes(uint value)
        {
            if (currentEndian == Endian.Big || !systemLittleEndian)
                return value >> 8;

            return value & 0x00FFFFFF;
        }

        public uint Get3LsbBytes(uint value)
        {
            if (currentEndian == Endian.Big || !systemLittleEndian)
                return value & 0x00FFFFFF;

            return value >> 8;
        }

        public override string ToString()
        {
            string endianness = (currentEndian == Endian.Little) ? "Little Endian" : "Big Endian";
            return $"EndianBinaryReader - Endianness: {endianness}";
        }

        #endregion

        #region ReadMultiple

        //TODO: Summary

        public uint[] ReadUInt32s(int count)
        {
            List<uint> uints = new List<uint>();

            for (int i = 0; i < count; i++)
                uints.Add(ReadUInt32());

            return uints.ToArray();
        }

        #endregion

        #region PeekRead[x] Methods

        /// <summary>
        /// Reads a signed byte relative to the current position
        /// in the underlying <see cref="Stream"/> without advancing position.
        /// </summary>
        /// <returns>Signed Byte that was read.</returns>
        public sbyte PeekReadSByte()
        {
            sbyte res = ReadSByte();
            BaseStream.Position -= sizeof(SByte);
            return res;
        }

        /// <summary>
        /// Reads a unsigned byte relative to the current position
        /// in the underlying <see cref="Stream"/> without advancing position.
        /// </summary>
        /// <returns>Byte that was read.</returns>
        public byte PeekReadByte()
        {
            byte res = ReadByte();
            BaseStream.Position -= sizeof(Byte);
            return res;
        }

        /// <summary>
        /// Reads a signed 16-bit Integer relative to the current position
        /// in the underlying <see cref="Stream"/> without advancing position.
        /// </summary>
        /// <returns>Signed 16-bit Integer that was read.</returns>
        public short PeekReadInt16()
        {
            short res = ReadInt16();
            BaseStream.Position -= sizeof(Int16);
            return res;
        }

        /// <summary>
        /// Reads a unsigned 16-bit Integer relative to the current position
        /// in the underlying <see cref="Stream"/> without advancing position.
        /// </summary>
        /// <returns>Unsigned 16-bit Integer that was read.</returns>
        public ushort PeekReadUInt16()
        {
            ushort res = ReadUInt16();
            BaseStream.Position -= sizeof(UInt16);
            return res;
        }

        /// <summary>
        /// Reads a signed 32-bit Integer relative to the current position
        /// in the underlying <see cref="Stream"/> without advancing position.
        /// </summary>
        /// <returns>Signed 32-bit Integer that was read.</returns>
        public int PeekReadInt32()
        {
            int res = ReadInt32();
            BaseStream.Position -= sizeof(Int32);
            return res;
        }

        /// <summary>
        /// Reads a unsigned 32-bit Integer relative to the current position
        /// in the underlying <see cref="Stream"/> without advancing position.
        /// </summary>
        /// <returns>Unsigned 32-bit Integer that was read.</returns>
        public uint PeekReadUInt32()
        {
            uint res = ReadUInt32();
            BaseStream.Position -= sizeof(UInt32);
            return res;
        }

        /// <summary>
        /// Reads a signed 64-bit Integer relative to the current position
        /// in the underlying <see cref="Stream"/> without advancing position.
        /// </summary>
        /// <returns>Signed 64-bit Integer that was read.</returns>
        public long PeekReadInt64()
        {
            long res = ReadInt64();
            BaseStream.Position -= sizeof(Int64);
            return res;
        }

        /// <summary>
        /// Reads a unsigned 64-bit Integer relative to the current position
        /// in the underlying <see cref="Stream"/> without advancing position.
        /// </summary>
        /// <returns>Unsigned 64-bit Integer that was read.</returns>
        public ulong PeekReadUInt64()
        {
            ulong res = ReadUInt64();
            BaseStream.Position -= sizeof(UInt64);
            return res;
        }

        /// <summary>
        /// Reads a 32-bit Floating-Point Number relative to the current position
        /// in the underlying <see cref="Stream"/> without advancing position.
        /// </summary>
        /// <returns>32-bit Floating-Point Number that was read.</returns>
        public float PeekReadSingle()
        {
            float res = ReadSingle();
            BaseStream.Position -= sizeof(Single);
            return res;
        }

        /// <summary>
        /// Reads a 64-bit Floating-Point Number relative to the current position
        /// in the underlying <see cref="Stream"/> without advancing position.
        /// </summary>
        /// <returns>64-bit Floating-Point Number that was read.</returns>
        public double PeekReadDouble()
        {
            double res = ReadDouble();
            BaseStream.Position -= sizeof(Double);
            return res;
        }

        /// <summary>
        /// Reads x Bytes into an array, relative to the current position
        /// in the underlying <see cref="Stream"/> without advancing position.
        /// </summary>
        /// <param name="count">Amount of Bytes to read.</param>
        /// <returns>Byte-Array containing the Bytes that were read.</returns>
        public byte[] PeekReadBytes(int count)
        {
            if (count < 0)
                throw new ArgumentException($"{nameof(count)} cannot be negative!", nameof(count));

            byte[] res = ReadBytes(count);
            BaseStream.Position -= count;
            return res;
        }

        #endregion
        
        #region ReadAt[x] Methods

        /// <summary>
        /// Reads a boolean at a given offset without
        /// changing the underlying <see cref="Stream"/> position.
        /// </summary>
        /// <param name="offset">The offset to read at.</param>
        /// <returns>Boolean value read at the given offset.</returns>
        public bool ReadBooleanAt(long offset)
        {
            long origPos = BaseStream.Position;
            BaseStream.Position = offset;
            bool res = ReadBoolean();
            BaseStream.Position = origPos;
            return res;
        }

        /// <summary>
        /// Reads a signed Byte at a given offset without
        /// changing the underlying <see cref="Stream"/> position.
        /// </summary>
        /// <param name="offset">The offset to read at.</param>
        /// <returns>Signed Byte read at the given offset.</returns>
        public sbyte ReadSByteAt(long offset)
        {
            long origPos = BaseStream.Position;
            BaseStream.Position = offset;
            sbyte res = ReadSByte();
            BaseStream.Position = origPos;
            return res;
        }

        /// <summary>
        /// Reads a unsigned Byte at a given offset without
        /// changing the underlying <see cref="Stream"/> position.
        /// </summary>
        /// <param name="offset">The offset to read at.</param>
        /// <returns>Unsigned Byte read at the given offset.</returns>
        public byte ReadByteAt(long offset)
        {
            long origPos = BaseStream.Position;
            BaseStream.Position = offset;
            byte res = ReadByte();
            BaseStream.Position = origPos;
            return res;
        }

        /// <summary>
        /// Reads a signed 16-bit Integer at a given offset without
        /// changing the underlying <see cref="Stream"/> position.
        /// </summary>
        /// <param name="offset">The offset to read at.</param>
        /// <returns>Signed 16-bit Integer read at the given offset.</returns>
        public short ReadInt16At(long offset)
        {
            long origPos = BaseStream.Position;
            BaseStream.Position = offset;
            short res = ReadInt16();
            BaseStream.Position = origPos;
            return res;
        }

        /// <summary>
        /// Reads a unsigned 16-bit Integer at a given offset without
        /// changing the underlying <see cref="Stream"/> position.
        /// </summary>
        /// <param name="offset">The offset to read at.</param>
        /// <returns>Unsigned 16-bit Integer read at the given offset.</returns>
        public ushort ReadUInt16At(long offset)
        {
            long origPos = BaseStream.Position;
            BaseStream.Position = offset;
            ushort res = ReadUInt16();
            BaseStream.Position = origPos;
            return res;
        }

        /// <summary>
        /// Reads a signed 32-bit Integer at a given offset without
        /// changing the underlying <see cref="Stream"/> position.
        /// </summary>
        /// <param name="offset">The offset to read at.</param>
        /// <returns>Signed 32-bit Integer read at the given offset.</returns>
        public int ReadInt32At(long offset)
        {
            long origPos = BaseStream.Position;
            BaseStream.Position = offset;
            int res = ReadInt32();
            BaseStream.Position = origPos;
            return res;
        }

        /// <summary>
        /// Reads a unsigned 32-bit Integer at a given offset without
        /// changing the underlying <see cref="Stream"/> position.
        /// </summary>
        /// <param name="offset">The offset to read at.</param>
        /// <returns>Unsigned 32-bit Integer read at the given offset.</returns>
        public uint ReadUInt32At(long offset)
        {
            long origPos = BaseStream.Position;
            BaseStream.Position = offset;
            uint res = ReadUInt32();
            BaseStream.Position = origPos;
            return res;
        }

        /// <summary>
        /// Reads a signed 64-bit Integer at a given offset without
        /// changing the underlying <see cref="Stream"/> position.
        /// </summary>
        /// <param name="offset">The offset to read at.</param>
        /// <returns>Signed 64-bit Integer read at the given offset.</returns>
        public long ReadInt64At(long offset)
        {
            long origPos = BaseStream.Position;
            BaseStream.Position = offset;
            long res = ReadInt64();
            BaseStream.Position = origPos;
            return res;
        }

        /// <summary>
        /// Reads a unsigned 64-bit Integer at a given offset without
        /// changing the underlying <see cref="Stream"/> position.
        /// </summary>
        /// <param name="offset">The offset to read at.</param>
        /// <returns>Unsigned 64-bit Integer read at the given offset.</returns>
        public ulong ReadUInt64At(long offset)
        {
            long origPos = BaseStream.Position;
            BaseStream.Position = offset;
            ulong res = ReadUInt64();
            BaseStream.Position = origPos;
            return res;
        }

        /// <summary>
        /// Reads a 32-bit Floating-Point Integer at a given offset without 
        /// changing the underlying <see cref="Stream"/> position.
        /// </summary>
        /// <param name="offset">The offset to read at.</param>
        /// <returns>32-bit Floating-Point Integer read at the offset.</returns>
        public float ReadSingleAt(long offset)
        {
            long origPos = BaseStream.Position;
            BaseStream.Position = offset;
            float res = ReadSingle();
            BaseStream.Position = origPos;
            return res;
        }


        /// <summary>
        /// Reads a 64-bit Floating-Point Integer at a given offset without 
        /// changing the underlying <see cref="Stream"/> position.
        /// </summary>
        /// <param name="offset">The offset to read at.</param>
        /// <returns>64-bit Floating-Point Integer read at the offset.</returns>
        public double ReadDoubleAt(long offset)
        {
            long origPos = BaseStream.Position;
            BaseStream.Position = offset;
            double res = ReadDouble();
            BaseStream.Position = origPos;
            return res;
        }

        /// <summary>
        /// Reads a 128-bit Floating-Point Integer at a given offset without 
        /// changing the underlying <see cref="Stream"/> position.
        /// </summary>
        /// <param name="offset">The offset to read at.</param>
        /// <returns>128-bit Floating-Point Integer read at the offset.</returns>
        public decimal ReadDecimalAt(long offset)
        {
            long origPos = BaseStream.Position;
            BaseStream.Position = offset;
            decimal res = ReadDecimal();
            BaseStream.Position = origPos;
            return res;
        }

        /// <summary>
        /// Reads a Char at a given offset without
        /// changing the underlying <see cref="Stream"/> position.
        /// </summary>
        /// <param name="offset">The offset to read at.</param>
        /// <returns>Char read at the given offset.</returns>
        public ulong ReadCharAt(long offset)
        {
            long origPos = BaseStream.Position;
            BaseStream.Position = offset;
            char res = ReadChar();
            BaseStream.Position = origPos;
            return res;
        }

        /// <summary>
        /// Reads x Bytes starting at the given offset
        /// without changing the position of the underlying <see cref="Stream"/>.
        /// </summary>
        /// <param name="offset">The offset to read at.</param>
        /// <param name="count">Number of Bytes to read.</param>
        /// <returns>Byte-Array containing the read Bytes.</returns>
        public byte[] ReadBytesAt(long offset, int count)
        {
            long origPos = BaseStream.Position;
            BaseStream.Position = offset;
            byte[] res = ReadBytes(count);
            BaseStream.Position = origPos;
            return res;
        }

        /// <summary>
        /// Reads x Chars starting at the given offset
        /// without changing the position of the underlying <see cref="Stream"/>.
        /// </summary>
        /// <param name="offset">The offset to read at.</param>
        /// <param name="count">Number of Chars to read.</param>
        /// <returns>Char-Array containing the read Chars.</returns>
        public char[] ReadCharsAt(long offset, int count)
        {
            long origPos = BaseStream.Position;
            BaseStream.Position = offset;
            char[] res = ReadChars(count);
            BaseStream.Position = origPos;
            return res;
        }

        #endregion

        #region ReadUntil[x] Methods

        /// <summary>
        /// Reads Bytes from the underlying <see cref="Stream"/>
        /// until the given terminating Byte is hit.
        /// </summary>
        /// <param name="terminator">The terminator to stop reading at.</param>
        /// <returns>Byte-Array read until the terminator was hit.</returns>
        public byte[] ReadBytesUntil(byte terminator)
        {
            List<byte> bytes = new List<byte>();
            byte b;
            while ((b = ReadByte()) != terminator)
                bytes.Add(b);
            return bytes.ToArray();
        }

        /// <summary>
        /// Reads Chars from the underlying <see cref="Stream"/>
        /// until the given terminating Char is hit.
        /// </summary>
        /// <param name="terminator">The terminator to stop reading at.</param>
        /// <returns>Char-Array read until the terminator was hit.</returns>
        public char[] ReadBytesUntil(char terminator)
        {
            List<char> chars = new List<char>();
            char c;
            while ((c = ReadChar()) != terminator)
                chars.Add(c);
            return chars.ToArray();
        }

        /// <summary>
        /// Reads Chars from the underlying <see cref="Stream"/>
        /// until the given terminating Char is hit.
        /// </summary>
        /// <param name="terminator">The terminator to stop reading at.</param>
        /// <returns>String of Chars read until the terminator was hit.</returns>
        public string ReadStringUntil(char terminator)
        {
            StringBuilder sb = new StringBuilder();
            char c;

            while ((c = ReadChar()) != terminator)
                sb.Append(c);
            return sb.ToString();
        }

        #endregion

        #region Skip[x] Methods

        /// <summary>
        /// Skips the underlying <see cref="Stream"/> ahead by
        /// count Bytes from its current position.
        /// </summary>
        /// <param name="count">The number of Bytes to skip.</param>
        public void Skip(long count)
        {
            if (count >= BaseStream.Length)
                throw new ArgumentException($"{nameof(count)} cannot be larger than the length of the underlying stream!", nameof(count));

            if ((BaseStream.Position + count) >= BaseStream.Length)
                throw new ArgumentException("Skipping " + count + " bytes would exceed the underlying stream's length!");

            BaseStream.Position += count;
        }

        /// <summary>
        /// Skips the underlying <see cref="Stream"/>
        /// ahead by the size of an unsigned Byte.
        /// </summary>
        public void SkipByte()
        {
            Skip(sizeof(Byte));
        }

        /// <summary>
        /// Skips the underlying <see cref="Stream"/>
        /// ahead by the size of a signed Byte.
        /// </summary>
        public void SkipSByte()
        {
            Skip(sizeof(SByte));
        }

        /// <summary>
        /// Skips the underlying <see cref="Stream"/>
        /// ahead by the size of a signed 16-bit Integer.
        /// </summary>
        public void SkipInt16()
        {
            Skip(sizeof(Int16));
        }

        /// <summary>
        /// Skips the underlying <see cref="Stream"/>
        /// ahead by the size of an unsigned 16-bit Integer.
        /// </summary>
        public void SkipUInt16()
        {
            Skip(sizeof(UInt16));
        }

        /// <summary>
        /// Skips the underlying <see cref="Stream"/>
        /// ahead by the size of a signed 32-bit Integer.
        /// </summary>
        public void SkipInt32()
        {
            Skip(sizeof(Int32));
        }

        /// <summary>
        /// Skips the underlying <see cref="Stream"/>
        /// ahead by the size of an unsigned 32-bit Integer.
        /// </summary>
        public void SkipUInt32()
        {
            Skip(sizeof(UInt32));
        }

        /// <summary>
        /// Skips the underlying <see cref="Stream"/>
        /// ahead by the size of a signed 64-bit Integer.
        /// </summary>
        public void SkipInt64()
        {
            Skip(sizeof(Int64));
        }

        /// <summary>
        /// Skips the underlying <see cref="Stream"/>
        /// ahead by the size of an unsigned 64-bit Integer.
        /// </summary>
        public void SkipUInt64()
        {
            Skip(sizeof(UInt64));
        }

        /// <summary>
        /// Skips the underlying <see cref="Stream"/>
        /// ahead by the size of a 32-bit Floating-Point Number.
        /// </summary>
        public void SkipSingle()
        {
            Skip(sizeof(Single));
        }

        /// <summary>
        /// Skips the underlying <see cref="Stream"/>
        /// ahead by the size of a 64-bit Floating-Point Number.
        /// </summary>
        public void SkipDouble()
        {
            Skip(sizeof(Double));
        }

        /// <summary>
        /// Skips the underlying <see cref="Stream"/>
        /// ahead by the size of a 128-bit Floating-Point Number.
        /// </summary>
        public void SkipDecimal()
        {
            Skip(sizeof(Decimal));
        }

        #endregion

        #region Utils

        //TODO: Summary

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alignment"></param>
        /// <param name="grow"></param>
        /// <returns></returns>
        public long Align(int alignment, bool grow = false)
        {
            if(alignment <= 0)
                throw new ArgumentOutOfRangeException("Alignment must be bigger than 0.");
            long position = BaseStream.Seek((-BaseStream.Position % alignment + alignment) % alignment, SeekOrigin.Current);
            if (grow && position > BaseStream.Length)
                BaseStream.SetLength(position);
            return position;
        }

        #endregion

        #endregion
    }
}