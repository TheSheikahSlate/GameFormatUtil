using System;
using System.Collections.Generic;
using System.Text;
using GameFormatUtil.Endian;

namespace GameFormatUtil.Archive
{
    public static class SARRC
    {
        #region Constants

        private const int SARC_HEADER_SIZE = 0x14;
        private const int SFAT_HEADER_SIZE = 0x0C;
        private const int NODE_SIZE = 0x10;
        private const int HASH_MULT = 0x00000065;

        #endregion

        #region SARC Header Class

        private sealed class SARCHeader
        {
            #region Properties

            public uint totalDataSize { get; set; }
            public uint dataOffset { get; private set; }

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="reader"><see cref="EndianBinaryReader"/> used to get relevant data from.</param>
            public SARCHeader(EndianBinaryReader reader)
            {
                reader.BaseStream.Position += 8;        //Skip Magic, Header Size(0x14) and BOM
                totalDataSize = reader.ReadUInt32();    //Total size of the Archive Data
                dataOffset = reader.ReadUInt32();       //DataOffset 
                reader.SkipUInt32();                    //Unknown UInt32, probably V
                                               
            }

            #endregion
        }
        #endregion

        #region SFAT Header Class

        public sealed class SFATHeader
        {
            #region Properties

            public uint nodeCount { get; private set; }

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="reader"><see cref="EndianBinaryReader"/> used to get relevant data from.</param>
            public SFATHeader(EndianBinaryReader reader)
            {
                reader.BaseStream.Position += 6; //Skip Magic and Header Size(0x0C)
                nodeCount = reader.ReadUInt16(); //Node count
                reader.SkipUInt32();             //Skip Hash Multiplier(0x00000065)
            }

            #endregion
        }

        #endregion

        #region Node Class

        private sealed class Node
        {
            #region Properties

            public uint nameHash { get; private set; }
            public byte type { get; private set; }
            public byte[] data { get; private set; }

            #endregion
        }

        #endregion
    }
}
