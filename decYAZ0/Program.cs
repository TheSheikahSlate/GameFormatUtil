using System;
using System.IO;
using GameFormatUtil.Compression;


namespace decYAZ0
{
    public class Program
    {

        static Program()
        {
            CosturaUtility.Initialize();    
        }

        public static void Main(string[] args)
        {
            if (args.Length < 1)
                throw new ArgumentOutOfRangeException();

            string f = args[0];
                
            if (f == null)
                throw new ArgumentNullException(nameof(f));

            if (!File.Exists(f))
                throw new FileNotFoundException($"{f} does not exist!", f);

            if (Yaz0.IsYaz0(File.ReadAllBytes(f)))
                Console.WriteLine("Yes, this is legit.");
            else
                Console.WriteLine("Nope, screw you.");

            byte[] output = Yaz0.Decode(f);

            using (FileStream outFile = File.Create(new FileInfo(f).Directory.FullName + '/' +new FileInfo(f).Name + ".dY0"))
            {
                outFile.Write(output, 0, output.Length);
            }
        }
    }
}
