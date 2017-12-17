using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Xml;

namespace decAAMP
{
    class AAMP
    {
        public AAMP(string filename)
        {
            Read(filename);
        }
    
        public class Node
        {
            public enum type
            {
                Boolean = 0x0,
                Float = 0x1,
                Int = 0x2,
                Vector2 = 0x3,
                Vector3 = 0x4,
                Vector4 = 0x6,
                String = 0x7,
                String2 = 0x14,
                Actor = 0x8
            }

            public type nodeType;
            public string name = null;
            public uint nameHash = 0;
            public List<Node> children = new List<Node>();
            public object value = null;
            public int ValueOffset;

            public void Read(FileData f)
            {
                nameHash = f.ReadUInt32();
                name = GetName(nameHash);
                int valueOffset = (f.ReadShort() * 4);
                byte childCount = (byte)f.ReadByte();
                nodeType = (type)f.ReadByte();
                int currentPosition = f.Pos();
                f.Seek(valueOffset + currentPosition - 8);
                ValueOffset = valueOffset + currentPosition - 8;
                if(childCount > 0)
                {
                    for(int i = 0; i < childCount; i++)
                    {
                        Node child = new Node();
                        child.Read(f);
                        children.Add(child);
                    }
                }
                else
                {
                    //Nintendo
                    switch (nodeType)
                    {
                        case type.Int:
                            value = f.ReadUInt32();
                            break;
                        case type.Float:
                            value = f.ReadFloat();
                            break;
                        case type.Actor:
                        case type.String:
                        case type.String2:
                            value = f.ReadString();
                            break;
                        case type.Boolean:
                            value = (f.ReadByte() != 0);
                            break;
                        case type.Vector2:
                            value = new float[2] { f.ReadFloat(), f.ReadFloat() };
                            break;
                        case type.Vector3:
                            value = new float[3] { f.ReadFloat(), f.ReadFloat(), f.ReadFloat() };
                            break;
                        case type.Vector4:
                            value = new float[4] { f.ReadFloat(), f.ReadFloat(), f.ReadFloat(), f.ReadFloat() };
                            break;
                    }
                }
                f.Seek(currentPosition);
            }

            public XmlElement ToXmlElement(XmlDocument doc)
            {
                string nodeName = name;
                if (name == null)
                    nodeName = "UnknownName";
                XmlElement node = doc.CreateElement(nodeName);
                if(name == null)
                    node.SetAttribute("hash", "0x" + nameHash.ToString("x"));
                if(children.Count == 0)
                {
                    node.SetAttribute("type", nodeType.ToString());
                    string Value = "";
                    //Nintendo
                    switch (nodeType)
                    {
                        case type.Int:
                            Value = value.ToString();
                            break;
                        case type.Float:
                            Value = value.ToString();
                            break;
                        case type.Actor:
                        case type.String:
                        case type.String2:
                            Value = value.ToString();
                            break;
                        case type.Boolean:
                            Value = "0";
                            if ((bool)value)
                                Value = "1";
                            break;
                        case type.Vector2:
                            Value = $"{((float[])value)[0]} {((float[])value)[1]}";
                            break;
                        case type.Vector3:
                            Value = $"{((float[])value)[0]} {((float[])value)[1]} {((float[])value)[2]}";
                            break;
                        case type.Vector4:
                            Value = $"{((float[])value)[0]} {((float[])value)[1]} {((float[])value)[2]} {((float[])value)[3]}";
                            break;
                        default:
                            Value = "Offset 0x" + ValueOffset.ToString("X");
                            break;
                    }
                    node.InnerText = Value;
                }else
                {
                    foreach(Node child in children)
                    {
                        node.AppendChild(child.ToXmlElement(doc));
                    }
                }
                return node;
            }
        }

        public Node rootNode;

        public void Read(string filename)
        {
            Read(new FileData(filename));
        }

        public void Read(FileData f)
        {
            f.endian = Endianness.Little;
            f.Skip(4); //AAMP (Magic ID)
            uint version = f.ReadUInt32();
            uint unknown0x8 = f.ReadUInt32();
            if (version != 2)
                throw new NotImplementedException("AAMP-Version is not yet supported by the application!");
            uint fileSize = f.ReadUInt32();
            f.Skip(4); //Padding 00 00 00 00
            uint xmlStringLength = f.ReadUInt32();
            uint unknown0x18 = f.ReadUInt32();
            uint unknown0x1C = f.ReadUInt32();
            uint unknown0x20 = f.ReadUInt32();
            uint dataBufferSize = f.ReadUInt32();
            uint stringBufferSize = f.ReadUInt32();
            uint unknown0x2C = f.ReadUInt32();
            f.Skip((int)xmlStringLength); //"xml" Null Terminated String
            rootNode = new Node();
            rootNode.nameHash = f.ReadUInt32();
            rootNode.name = GetName(rootNode.nameHash);
            f.Skip(4); //No Idea, always 0x3?
            int dataOffset = (f.Pos() - 8) + (f.ReadShort() * 4);
            ushort childCount = (ushort)f.ReadShort();
            f.Seek(dataOffset);
            for(int i = 0; i < childCount; i++)
            {
                Node child = new Node();
                child.Read(f);
                rootNode.children.Add(child);
            }
        }

        private int GetDataSize(Node n)
        {
            if(n.children.Count > 0)
            {
                int size = 0;
                foreach (Node child in n.children)
                    size += GetDataSize(child);
                return size;
            }else
            {
                //Nintendo
                switch (n.nodeType)
                {
                    case Node.type.Boolean:
                    case Node.type.Float:
                    case Node.type.Int:
                        return 4;
                    case Node.type.Vector2:
                        return 8;
                    case Node.type.Vector3:
                        return 0xC;
                    case Node.type.Vector4:
                        return 0x10;
                    case Node.type.Actor:
                    case Node.type.String:
                    case Node.type.String2:
                    default:
                        return 0;
                }
            }
        }

        private int GetStringSize(Node n)
        {
            if (n.children.Count > 0)
            {
                int size = 0;
                foreach (Node child in n.children)
                    size += GetStringSize(child);
                return size;
            }else
            {
                //Nintendo
                switch (n.nodeType)
                {
                    case Node.type.Actor:
                    case Node.type.String:
                    case Node.type.String2:
                        int size = ((string)n.value).Length;
                        do
                            size++;
                        while (size % 4 != 0);
                        return size;
                    case Node.type.Boolean:
                    case Node.type.Float:
                    case Node.type.Int:
                    case Node.type.Vector2:
                    case Node.type.Vector3:
                    case Node.type.Vector4:
                    default:
                        return 0;
                }
            }
        }

        private int GetChildNodeCount(Node n)
        {
            int childCount = n.children.Count;
            foreach (Node child in n.children)
                childCount += GetChildNodeCount(child);
            return childCount;
        }

        private int GetGrandChildNodeCount(Node n)
        {
            return GetChildNodeCount(n) - n.children.Count;
        }

        private void WriteChildren(Node node, FileOutput f, FileOutput dataBuffer, FileOutput stringBuffer, ref int dataBufferOffset, ref int stringBufferOffset)
        {
            int childNodeOffset = f.Pos() + (node.children.Count * 8);
            foreach(Node child in node.children)
            {
                int offset;
                if(child.children.Count > 0){
                    offset = childNodeOffset;
                    childNodeOffset += child.children.Count * 8;
                }else
                {
                    //Nintendo
                    switch (child.nodeType)
                    {
                        case Node.type.Actor:
                        case Node.type.String:
                        case Node.type.String2:
                            offset = stringBufferOffset;
                            break;
                        case Node.type.Boolean:
                        case Node.type.Float:
                        case Node.type.Int:
                        case Node.type.Vector2:
                        case Node.type.Vector3:
                        case Node.type.Vector4:
                        default:
                            offset = dataBufferOffset;
                            break;
                    }
                }
                offset = (offset - f.Pos()) / 4;
                f.WriteInt(child.nameHash);
                f.WriteShort(offset);
                f.WriteByte(child.children.Count);
                f.WriteByte((byte)child.nodeType);

                if(child.children.Count == 0)
                {
                    //Nintendo
                    switch (child.nodeType)
                    {
                        case Node.type.Boolean:
                            if ((bool)child.value)
                                dataBuffer.WriteInt(1);
                            else
                                dataBuffer.WriteInt(0);
                            break;
                        case Node.type.Float:
                            dataBuffer.WriteFloat((float)child.value);
                            break;
                        case Node.type.Int:
                            dataBuffer.WriteInt((uint)child.value);
                            break;
                        case Node.type.Vector2:
                            dataBuffer.WriteFloat(((float[])child.value)[0]);
                            dataBuffer.WriteFloat(((float[])child.value)[1]);
                            break;
                        case Node.type.Vector3:
                            dataBuffer.WriteFloat(((float[])child.value)[0]);
                            dataBuffer.WriteFloat(((float[])child.value)[1]);
                            dataBuffer.WriteFloat(((float[])child.value)[2]);
                            break;
                        case Node.type.Vector4:
                            dataBuffer.WriteFloat(((float[])child.value)[0]);
                            dataBuffer.WriteFloat(((float[])child.value)[1]);
                            dataBuffer.WriteFloat(((float[])child.value)[2]);
                            dataBuffer.WriteFloat(((float[])child.value)[3]);
                            break;
                        case Node.type.Actor:
                        case Node.type.String:
                        case Node.type.String2:
                            stringBuffer.WriteString((string)child.value);
                            do
                                stringBuffer.WriteByte(0);
                            while (stringBuffer.Pos() % 4 != 0);
                            break;
                    }
                    dataBufferOffset += GetDataSize(child);
                    stringBufferOffset += GetStringSize(child);
                }
            }
            foreach (Node child in node.children)
                WriteChildren(child, f, dataBuffer, stringBuffer, ref dataBufferOffset, ref stringBufferOffset);
        }

        public byte[] Rebuild()
        {
            FileOutput f = new FileOutput(), dataBuffer = new FileOutput(), stringBuffer = new FileOutput();
            f.endian = Endianness.Little;
            dataBuffer.endian = Endianness.Little;
            f.WriteString("AAMP");
            f.WriteInt(2);
            f.WriteInt(3);
            int dataSize = GetDataSize(rootNode);
            int stringSize = GetDataSize(rootNode);
            int nodeCount = GetChildNodeCount(rootNode);
            int nonDirectChildCount = GetGrandChildNodeCount(rootNode);
            f.WriteInt(0x40 + (nodeCount * 8) + dataSize + stringSize);
            f.WriteInt(0);
            f.WriteInt(4);
            f.WriteInt(1);
            f.WriteInt(nodeCount - nonDirectChildCount);
            f.WriteInt(nonDirectChildCount);
            f.WriteInt(dataSize);
            f.WriteInt(stringSize);
            f.WriteInt(0);
            f.WriteString("xml");
            f.WriteByte(0);
            f.WriteInt(rootNode.nameHash);
            f.WriteInt(3);
            f.WriteShort(3);
            f.WriteShort(rootNode.children.Count);
            int dataBufferPos = 0x40 + (nodeCount * 8);
            int stringBufferPos = dataBufferPos + dataSize;
            WriteChildren(rootNode, f, dataBuffer, stringBuffer, ref dataBufferPos, ref stringBufferPos);
            f.WriteBytes(dataBuffer.GetBytes());
            f.WriteBytes(stringBuffer.GetBytes());
            return f.GetBytes();
        }

        public XmlDocument ToXML()
        {
            XmlDocument xml = new XmlDocument();
            xml.AppendChild(rootNode.ToXmlElement(xml));
            return xml;
        }

        public static Dictionary<uint, string> hashName = new Dictionary<uint, string>();

        private static void GenerateHashes()
        {
            foreach(string hashStr in Resources.U_King.Split('\n'))
            {
                uint hash = Crc32.Compute(hashStr);
                if (!hashName.ContainsKey(hash))
                    hashName.Add(hash, hashStr);
            }
        }

        public static string GetName(uint hash)
        {
            if (hashName.Count == 0)
                GenerateHashes();
            string name = null;
            hashName.TryGetValue(hash, out name);
            return name;
        }
    }
}
