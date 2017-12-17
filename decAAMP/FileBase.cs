using System;
using System.IO;

namespace decAAMP
{
    public abstract class FileBase
    {

        public abstract Endianness Endian { get; set; }

        public abstract void Read(string filename);
        public abstract byte[] Rebuild();

        public void Save(string filename)
        {
            var data = Rebuild();
            if (data.Length <= 0)
                throw new Exception("Warning: No data to be saved was found!");

            File.WriteAllBytes(filename, data);
        }
    }
}

namespace System.IO
{
    public enum Endianness
    {
        Big = 0,
        Little = 1
    }
}
