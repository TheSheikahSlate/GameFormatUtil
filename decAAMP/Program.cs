using System;
using System.Xml;
using System.IO;

namespace decAAMP
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName;
            string arg = args[0];

            if (!arg.EndsWith(".xml"))
            {
                AAMP testAAMP = new AAMP(arg);
                fileName = Path.ChangeExtension(arg, ".xml");
                XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
                XmlWriter writer = XmlWriter.Create(fileName, settings);
                testAAMP.ToXML().Save(writer);
            }
            else
            {
                Console.WriteLine("This is a .XML file already, what are you trying to do?");
            }
        }
    }
}
