using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using GameFormatUtil.Endian;

namespace GameFormatUtil.Compression
{
    public class Byaml
    {
        #region Constants

        private const ushort MAGIC = 0x4259;
        private const ushort VERSION = 2;

        #endregion

        #region Properties

        public ByamlHeader header { get; internal set; }
        private List<string> nameArray { get; set; }
        private List<string> stringArray { get; set; }

        #endregion

        #region Public Methods

        public dynamic Read(Stream stream)
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(stream, Encoding.UTF8, Endian.Endian.Big))
            {
                header = new ByamlHeader(reader);

                reader.BaseStream.Seek(header.nameTableOffset, SeekOrigin.Begin);
                Console.WriteLine($"[decBYAML] Starting to decompile NameTable at Offset {header.nameTableOffset}...");
                nameArray = ReadNode(reader);

                if (header.stringTableOffset != 0x000000)
                {
                    reader.BaseStream.Seek(header.stringTableOffset, SeekOrigin.Begin);
                    Console.WriteLine($"[decBYAML] Starting to decompile StringTable at Offset {header.stringTableOffset}...");
                    stringArray = ReadNode(reader);
                }
                else
                    Console.WriteLine("[decBYAML] StringTableOffset is 0, skipping...");

                reader.BaseStream.Seek(header.rootNodeOffset, SeekOrigin.Begin);
                Console.WriteLine($"[decBYAML] Starting to decompile RootNode at Offset {header.rootNodeOffset}...");
                dynamic data = ReadNode(reader);
                WriteXML(data);
                return data;
            }
        }

        public void WriteXML(Dictionary<KeyValuePair<string, NodeType>, dynamic> data)
        {

            XmlWriter writer = XmlWriter.Create("test.xml");
            writer.WriteStartDocument();

            foreach (var i in data)
            {
                KeyValuePair<String, NodeType> part1 = i.Key;
                dynamic value = i.Value;

                WriteNode(writer, part1.Key, part1.Value, i.Value);
            }
            writer.WriteEndDocument();
            writer.Close();
        }

        #endregion

        #region Private Methods

        private void WriteNode(XmlWriter writer, String name, NodeType type, dynamic value)
        {
            writer.WriteStartElement(name);
            writer.WriteAttributeString("TYPE", type.ToString());

            if (type == NodeType.DICTIONARY)
                foreach (var i in (Dictionary<KeyValuePair<string, NodeType>, dynamic>) value)
                {
                    KeyValuePair<string, NodeType> nameType = i.Key;
                    WriteNode(writer, nameType.Key, nameType.Value, i.Value);
                }
            else if (type == NodeType.ARRAY)
                foreach (var i in (List<KeyValuePair<NodeType, dynamic>>) value)
                    WriteNode(writer, "entry", i.Key, i.Value);         
            else
                writer.WriteString(value.ToString());

            writer.WriteEndElement();
        }

        private dynamic ReadNode(EndianBinaryReader reader, NodeType type = 0)
        {
            bool typeGiven = type != 0;
            if (!typeGiven)
                type = (NodeType) reader.ReadByte();

            if (type >= NodeType.ARRAY && type <= NodeType.STRING_ARRAY)
            {
                long? oldPos = null;
                if (typeGiven)
                {
                    uint offset = reader.ReadUInt32();
                    oldPos = reader.BaseStream.Position;
                    reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                }
                else
                {
                    reader.BaseStream.Seek(-1, SeekOrigin.Current);
                }

                int length = (int)reader.Get3LsbBytes(reader.ReadUInt32());
                dynamic value;
                switch (type)
                {
                    case NodeType.ARRAY:
                        value = ReadArrayNode(reader, length);
                        break;
                    case NodeType.DICTIONARY:
                        value = ReadDictionaryNode(reader, length);
                        break;
                    case NodeType.STRING_ARRAY:
                        value = ReadStringArrayNode(reader, length);
                        break;
                    default:
                        throw new ByamlException($"Unknown Node Type '{((byte)type).ToString("X2")}'!");
                }
                if (oldPos.HasValue)
                    reader.BaseStream.Seek(oldPos.Value, SeekOrigin.Begin);

                return value;
            }
            else
            {
                switch(type)
                {
                    case NodeType.STRING:
                        return stringArray[reader.ReadInt32()];
                    case NodeType.BOOLEAN:
                        return reader.ReadBoolean();
                    case NodeType.INTEGER:
                        return reader.ReadInt32();
                    case NodeType.FLOAT:
                        return reader.ReadSingle();
                    case NodeType.HASHID:
                        uint hash = reader.ReadCrc32();
                        Console.WriteLine(hash);
                        return hash;
                    case NodeType.NULL:
                        reader.Skip(3);
                        return "";
                    default:
                        Console.WriteLine($"Error at {reader.BaseStream.Position}/{reader.BaseStream.Length}");
                        throw new ByamlException($"Unknown Node Type '{((byte)type).ToString("X2")}'!");
                }
            }
        }

        private List<KeyValuePair<NodeType, dynamic>> ReadArrayNode(EndianBinaryReader reader, int length)
        {
            List<KeyValuePair<NodeType, dynamic>> array = new List<KeyValuePair<NodeType, dynamic>>(length);

            byte[] nodeTypes = reader.ReadBytes(length);
            reader.Align(4);
            for (int i = 0; i < length; i++)
                array.Add(new KeyValuePair<NodeType, dynamic>((NodeType)nodeTypes[i], ReadNode(reader, (NodeType)nodeTypes[i])));

            return array;
        }

        private Dictionary<KeyValuePair<string, NodeType>, dynamic> ReadDictionaryNode(EndianBinaryReader reader, int length)
        {
            Dictionary<KeyValuePair<string, NodeType>, dynamic> dictionary = new Dictionary<KeyValuePair<string, NodeType>, dynamic>();

            for (int i = 0; i < length; i++)
            {
                uint lengthType = reader.ReadUInt32();
                int nameIndex = (int)reader.Get3MsbBytes(lengthType);
                NodeType type = (NodeType) reader.Read1MsbByte(lengthType);
                String name = nameArray[nameIndex];
                dynamic value = ReadNode(reader, type);
                Console.WriteLine(name);
                dictionary.Add(new KeyValuePair<string, NodeType>(name, type), value);
            }

            return dictionary;
        }

        private List<string> ReadStringArrayNode(EndianBinaryReader reader, int length)
        {
            List<string> stringArray = new List<string>(length);

            long nodeOffset = reader.BaseStream.Position - 4;
            uint[] offsets = reader.ReadUInt32s(length);

            long oldPos = reader.BaseStream.Position;
            for (int i = 0; i < length; i++)
            {
                reader.BaseStream.Seek(nodeOffset + offsets[i], SeekOrigin.Begin);
                String s = reader.ReadZeroTerminatedString(Encoding.ASCII);
                Console.WriteLine($"[StringArray] Found String #{i}: '{s}'.");
                stringArray.Add(s);
            }
            Console.WriteLine("");
            Console.WriteLine($"[decBYAML] Finished decompiling StringArray. Total Size: {stringArray.Count}");
            reader.BaseStream.Seek(oldPos, SeekOrigin.Begin);

            return stringArray;
        }

        #endregion

        #region ByamlHeader

        public sealed class ByamlHeader
        {

            #region Properties

            public uint nameTableOffset { get; internal set; }
            public uint stringTableOffset { get; internal set; }
            public uint rootNodeOffset { get; internal set; }

            #endregion

            #region Constructors

            public ByamlHeader(EndianBinaryReader reader)
            {
                if(reader.ReadUInt16() != MAGIC) throw new ByamlException("Input file is not a Byaml compressed file!");
                if(reader.ReadUInt16() != VERSION) throw new ByamlException("Outdated version! Cannot decompress Byaml Version 1!");
                nameTableOffset = reader.ReadUInt32();
                stringTableOffset = reader.ReadUInt32();
                rootNodeOffset = reader.PeekReadUInt32();
                Console.WriteLine($"[decBYAML] HeaderCheck passed. MagicID: '{MAGIC}', Version: '{VERSION}', NameTableOffset: '{nameTableOffset}', StringTableOffset: '{stringTableOffset}', RootNodeOffset: '{rootNodeOffset}'");
                Console.WriteLine("");
            }

            #endregion
        }

        #endregion

        #region NodeType Enum

        public enum NodeType : byte
        {
            NULL = 0x00,
            STRING = 0xA0,
            ARRAY = 0xC0,
            DICTIONARY = 0xC1,
            STRING_ARRAY = 0xC2,
            BOOLEAN = 0xD0,
            INTEGER = 0xD1,
            FLOAT = 0xD2,
            HASHID = 0xD3
        }

        #endregion

        #region ByamlException

        private class ByamlException : Exception
        {

            public ByamlException(string message) : base(message) {}

            public ByamlException(string message, Exception inner) : base(message, inner) {}
        }

        #endregion
    }
}
