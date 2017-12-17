using GameFormatUtil.Compression;
using System;
using System.IO;

namespace decBYAML
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

            Byaml byaml = new Byaml();

            using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(f)))
            {
                try
                {
                    byaml.Read(stream);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[decBYAML] Unable to read BYAML content!");
                    Console.WriteLine($"[decBYAML] Reason: '{e.Message}'");
                    Console.Write(e.StackTrace);
                    Console.WriteLine("[decBYAML] Press 'Enter' to close the application.");
                    Console.Read();
                    Environment.Exit(1);
                }
                Console.WriteLine("[decBYAML] Successfully decompiles BYAML data.");
                Console.Read();
            }
        }
    }
}
