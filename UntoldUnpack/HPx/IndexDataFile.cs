using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UntoldUnpack.HPx
{
    // HPI
    public class IndexDataFile
    {
        public const string ExpectedMagicNumber = "HPIH";

        public string MagicNumber { get; private set; }
        public uint Unknown0x04 { get; private set; }
        public uint Unknown0x08 { get; private set; }
        public uint Unknown0x0C { get; private set; }
        public ushort Unknown0x10 { get; private set; }
        public ushort NumUnknownEntries { get; private set; }
        public ushort NumFileEntries { get; private set; }
        public ushort Unknown0x16 { get; private set; }

        public List<IndexUnknownEntry> UnknownEntries { get; private set; }
        public List<IndexFileEntry> FileEntries { get; private set; }

        public byte[] FilenameTableData { get; private set; }

        public IndexDataFile(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                MagicNumber = Encoding.ASCII.GetString(reader.ReadBytes(4), 0, 4);
                if (MagicNumber != ExpectedMagicNumber) throw new Exception("Invalid index file");

                Unknown0x04 = reader.ReadUInt32();
                Unknown0x08 = reader.ReadUInt32();
                Unknown0x0C = reader.ReadUInt32();
                Unknown0x10 = reader.ReadUInt16();
                NumUnknownEntries = reader.ReadUInt16();
                NumFileEntries = reader.ReadUInt16();
                Unknown0x16 = reader.ReadUInt16();

                UnknownEntries = new List<IndexUnknownEntry>();
                while (UnknownEntries.Count < NumUnknownEntries) UnknownEntries.Add(new IndexUnknownEntry(stream));

                FileEntries = new List<IndexFileEntry>();
                while (FileEntries.Count < NumFileEntries) FileEntries.Add(new IndexFileEntry(stream));

                FilenameTableData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            }
        }
    }
}
