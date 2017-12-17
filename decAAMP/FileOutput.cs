using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace decAAMP
{
    class FileOutput
    {

        List<byte> data = new List<byte>();

        public Endianness endian;

        public byte[] GetBytes()
        {
            return data.ToArray();
        }

        public void WriteString(string s)
        {
            char[] c = s.ToCharArray();
            for (int i = 0; i < c.Length; i++)
                data.Add((byte)c[i]);
        }

        public int Size()
        {
            return data.Count;
        }

        public void WriteOutput(FileOutput d)
        {
            foreach (byte b in d.data)
                data.Add(b);
        }

        private static char[] HexToCharArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .Select(x => Convert.ToChar(x))
                .ToArray();
        }

        public void WriteHex(string s) {
            char[] c = HexToCharArray(s);
            for (int i = 0; i < c.Length; i++)
                data.Add((byte)c[i]);
        }

        public void WriteInt(long i)
        {
            if(endian == Endianness.Little)
            {
                data.Add((byte)((i) & 0xFF));
                data.Add((byte)((i >> 8) & 0xFF));
                data.Add((byte)((i >> 16) & 0xFF));
                data.Add((byte)((i >> 24) & 0xFF));
            }else
            {
                data.Add((byte)((i >> 24) & 0xFF));
                data.Add((byte)((i >> 16) & 0xFF));
                data.Add((byte)((i >> 8) & 0xFF));
                data.Add((byte)((i) & 0xFF));
            }
        }

        public void WriteIntAt(int i, int p)
        {
            if(endian == Endianness.Big)
            {
                data[p++] = (byte)((i) & 0xFF);
                data[p++] = (byte)((i >> 8) & 0xFF);
                data[p++] = (byte)((i >> 16) & 0xFF);
                data[p++] = (byte)((i >> 24) & 0xFF);
            }else
            {
                data[p++] = (byte)((i >> 24) & 0xFF);
                data[p++] = (byte)((i >> 16) & 0xFF);
                data[p++] = (byte)((i >> 8) & 0xFF);
                data[p++] = (byte)((i) & 0xFF);
            }
        }

        public void WriteShortAt(int i, int p)
        {
            if(endian == Endianness.Little)
            {
                data[p++] = (byte)((i) & 0xFF);
                data[p++] = (byte)((i >> 8) & 0xFF);
            }else
            {
                data[p++] = (byte)((i >> 8) & 0xFF);
                data[p++] = (byte)((i) & 0xFF);
            }
        }

        public void Align(int i)
        {
            while (data.Count % i != 0)
                WriteByte(0);
        }

        public void Align(int i, int v)
        {
            while (data.Count % i != 0)
                WriteByte(v);
        }

        public void WriteFloat(float f)
        {
            int i = SingleToInt32(f, endian == Endianness.Big);
            data.Add((byte)((i) & 0xFF));
            data.Add((byte)((i >> 8) & 0XFF));
            data.Add((byte)((i >> 16) & 0xFF));
            data.Add((byte)((i >> 24) & 0xFF));
        }

        public static int SingleToInt32(float value, bool littleEndian)
        {
            byte[] b = BitConverter.GetBytes(value);
            int p = 0;

            if (littleEndian)
                return (b[p++] & 0xFF) | ((b[p++] & 0xFF) << 8) | ((b[p++] & 0xFF) << 16) | ((b[p++] & 0xFF) << 24);
            else
                return ((b[p++] & 0xFF) << 24) | ((b[p++] & 0xFF) << 16) | ((b[p++] & 0xFF) << 8) | (b[p++] & 0xFF);
        }

        public void WriteHalfFloat(float f)
        {
            int i = FileData.FromFloat(f, endian == Endianness.Little);
            data.Add((byte)((i >> 8) & 0xFF));
            data.Add((byte)((i) & 0xFF));
        }

        public void WriteShort(int i)
        {
            if(endian == Endianness.Little)
            {
                data.Add((byte)((i) & 0xFF));
                data.Add((byte)((i >> 8) & 0xFF));
            }else
            {
                data.Add((byte)((i >> 8) & 0xFF));
                data.Add((byte)((i) & 0xFF));
            }
        }

        public void WriteSignedShort(short i)
        {
            int signBit = 0;
            if(i < 0)
            {
                signBit = 8;
                i = (short)(~(-1));
            }
            if(endian == Endianness.Little)
            {
                data.Add((byte)((i) & 0xFF));
                data.Add((byte)(((i >> 8) & 0x7F) | signBit));
            }else
            {
                data.Add((byte)(((i >> 8) & 0x7F) | signBit));
                data.Add((byte)((i) & 0xFF));
            }
        }

        public void WriteByte(int i)
        {
            data.Add((byte)((i) & 0xFF));
        }

        public void WriteChars(char[] c)
        {
            foreach (char ch in c)
                WriteByte(Convert.ToByte(ch));
        }

        public void WriteBytes(byte[] bytes)
        {
            foreach (byte b in bytes)
                WriteByte(b);
        }

        public void WriteFlag(bool b)
        {
            if (b)
                WriteByte(1);
            else
                WriteByte(0);
        }

        public int Pos()
        {
            return data.Count;
        }

        public void Save(string filename)
        {
            File.WriteAllBytes(filename, data.ToArray());
        }
    }
}
