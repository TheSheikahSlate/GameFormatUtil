using System;
using System.IO;

namespace extSARC
{
    class Program
    {

        private static FileStream input;

        public static void Main(string[] args)
        {
            if (args.Length == 0 || args.Length > 3)
            {
                Console.WriteLine("ERROR, YOU FUCKED THE ARGUMENTS UP!");
                Environment.Exit(1);
            }

            input = new FileStream(args[0], FileMode.Open, FileAccess.Read);

            Boolean isValid = compareMagicID("SARC", 4, 0);
            if (!isValid)
                throw new InputException("The Input-File is not a SARC-Archive!");

            Console.WriteLine("The file is valid ;)");
        }

        public static Boolean compareMagicID(string mid, int length, int offset)
        {
            byte[] midb = new byte[length];
            input.Read(midb, offset, length);
            string mids = System.Text.Encoding.UTF8.GetString(midb);
            return mids.Equals(mid);
        }
    }
}
