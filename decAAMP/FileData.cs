using System;
using System.IO;

namespace decAAMP
{
    class FileData
    {
        private byte[] b;
        private int p = 0;
        public Endianness endian;

        public FileData(string f)
        {
            b = File.ReadAllBytes(f);
        }

        public FileData(byte[] b)
        {
            this.b = b;
        }

        public int Eof()
        {
            return b.Length;
        }

        public byte[] Read(int lenght)
        {
            if (lenght + p > b.Length)
                throw new IndexOutOfRangeException();

            var data = new byte[lenght];
            for(int i = 0; i < lenght; i++, p++)
            {
                data[i] = b[p];
            }
            return data;
        }

        public uint ReadUInt32()
        {
            if (endian == Endianness.Little)
                return (uint)((b[p++] & 0xFF) | ((b[p++] & 0xFF) << 8) | ((b[p++] & 0xFF) << 16) | ((b[p++] & 0xFF) << 24));
            else
                return (uint)(((b[p++] & 0xFF) << 24) | ((b[p++] & 0xFF) << 16) | ((b[p++] & 0xFF) << 8) | (b[p++] & 0xFF));
        }

        public int ReadThree()
        {
            if (endian == Endianness.Little)
                return (b[p++] & 0xFF) | ((b[p++] & 0xFF) << 8) | ((b[p++] & 0xFF) << 16);
            else
                return ((b[p++] & 0xFF) << 16) | ((b[p++] & 0xFF) << 8) | (b[p++] & 0xFF);
        }

        public int ReadShort()
        {
            if (endian == Endianness.Little)
                return (b[p++] & 0xFF) | ((b[p++] & 0xFF) << 8);
            else
                return ((b[p++] & 0xFF) << 8) | (b[p++] & 0xFF);
        }

        public short ReadSignedShort()
        {
            int num;
            if(endian == Endianness.Little)
            {
                num = (b[p++] & 0xFF) | ((b[p++] & 0x7F) << 8);
                if ((b[p] & 0x8) != 0)
                    num = -(~num);
            }else
            {
                num = ((b[p++] & 0x7F) << 8) | (b[p++] & 0xFF);
                if ((b[p - 1] & 0x8) != 0)
                    num = -(~num);
            }
            return (short)num;
        }

        public int ReadByte()
        {
            return (b[p++] & 0xFF);
        }

        public float ReadFloat()
        {
            byte[] by = new byte[4];
            if (endian == Endianness.Little)
                by = new byte[4] { b[p], b[p + 1], b[p + 2], b[p + 3] };
            else
                by = new byte[4] { b[p + 3], b[p + 2], b[p + 1], b[p] };

            p += 4;
            return BitConverter.ToSingle(by, 0);
        }

        public float ReadHalfFloat()
        {
            return ToFloat((short)ReadShort());
        }

        public static float ToFloat(int hbits)
        {
            int mant = hbits & 0x03ff;      
            int exp = hbits & 0x7c00;        
            if (exp == 0x7c00)                 
                exp = 0x3fc00;               
            else if(exp != 0)               
            {
                exp += 0x1c000;            
                if (mant == 0 && exp > 0x1c400)
                    return BitConverter.ToSingle(BitConverter.GetBytes((hbits & 0x800) << 16 | exp << 13 | 0x3ff), 0);
            } else if(mant != 0)          
            {
                exp = 0x1c400;                
                do
                {
                    mant <<= 1;               
                    exp -= 0x400;         
                } while ((mant & 0x400) == 0); 
                mant &= 0x3ff;              
            }                                 
            return BitConverter.ToSingle(BitConverter.GetBytes((hbits & 0x8000) << 16 | (exp | mant) << 13), 0);
        }

        public static int FromFloat(float fval, bool littleEndian)
        {
            int fbits = FileOutput.SingleToInt32(fval, littleEndian);
            int sign = fbits >> 16 & 0x8000;
            int val = (fbits & 0x7fffffff) + 0x1000;

            if(val >= 0x47800000)
            {
                if((fbits & 0x7fffffff) >= 0x47800000)
                {
                    if (val < 0x7f800000)
                        return sign | 0x7c00;
                    return sign | 0x7c00 | (fbits & 0x007fffff) >> 13;
                }
                return sign | 0x7bff;
            }
            if (val >= 0x38800000)
                return sign | val - 0x38000000 >> 13;
            if (val < 0x33000000)
                return sign;
            val = (fbits & 0x7fffffff) >> 23;
            return sign | ((fbits & 0x7fffff | 0x800000) + (0x800000 >> val - 102) >> 126 - val);
        }

        public static int Sign12Bit(int i)
        {
            if(((i >> 11) & 0x1) == 1)
            {
                i = ~i;
                i = i & 0xFFF;
                i += 1;
                i *= -1;
            }

            return i;
        }

        public void Skip(int i)
        {
            p += i;
        }

        public void Seek(int i)
        {
            p = i;
        }

        public int Pos()
        {
            return p;
        }

        public int Size()
        {
            return b.Length;
        }

        public string ReadString()
        {
            string s = "";
            while(b[p] != 0x00)
            {
                s += (char)b[p];
                p++;
            }
            return s;
        }

        public string ReadString(int p, int size)
        {
            if(size == -1)
            {
                string str = "";
                while(p < b.Length)
                {
                    if ((b[p] & 0xFF) != 0x00)
                        str += (char)(b[p] & 0xFF);
                    else
                        break;
                    p++;
                }
                return str;
            }

            string str2 = "";
            for(int i = p; i < p + size; i++)
            {
                if ((b[i] & 0xFF) != 0x00)
                    str2 += (char)(b[i] & 0xFF);
            }
            return str2;
        }

        public byte[] GetSection(int offset, int size)
        {
            byte[] by = new byte[size];
            Array.Copy(b, offset, by, 0, size);
            return by;
        }

        public void Align(int i)
        {
            while (p % i != 0)
                p++;
        }

        public int ReadOffset()
        {
            return p + (int)ReadUInt32();
        }
    }
}