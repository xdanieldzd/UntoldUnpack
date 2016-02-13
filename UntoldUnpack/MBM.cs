using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace UntoldUnpack
{
    public class MBM
    {
        public const string ExpectedMagicNumber = "MSG2";

        public uint MaybeAlwaysZero { get; private set; }       /* Always zero? */
        public string MagicNumber { get; private set; }
        public uint MaybeAlways65536 { get; private set; }      /* Always 0x00010000? */
        public uint FileSize { get; private set; }              /* NOTE: Not always correct! Not used & messed up during localization? */
        public uint NumEntries { get; private set; }
        public uint EntryOffset { get; private set; }

        public List<Entry> Entries { get; private set; }

        public uint ActualFileSize { get; private set; }

        public MBM(string filePath)
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                MaybeAlwaysZero = reader.ReadUInt32();
                MagicNumber = Encoding.ASCII.GetString(reader.ReadBytes(4), 0, 4);
                MaybeAlways65536 = reader.ReadUInt32();
                FileSize = reader.ReadUInt32();
                NumEntries = reader.ReadUInt32();
                EntryOffset = reader.ReadUInt32();

                if (MagicNumber != ExpectedMagicNumber) throw new Exception("Invalid MBM file");

                reader.BaseStream.Seek(EntryOffset, SeekOrigin.Begin);

                int validEntries = 0;
                Entries = new List<Entry>();
                while (validEntries < NumEntries)
                {
                    Entry newEntry = new Entry(reader);
                    Entries.Add(newEntry);
                    if (newEntry.NumBytes != 0) validEntries++;
                }

                ActualFileSize = (uint)reader.BaseStream.Length;
            }
        }

        public void Export(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.WriteLine("File: {0}", Path.GetFileNameWithoutExtension(filePath));
                writer.Write("Given filesize: 0x{0:X}", FileSize);
                if (FileSize != ActualFileSize) writer.WriteLine(" (actual size: 0x{0:X})", ActualFileSize);
                else writer.WriteLine();
                writer.WriteLine("Number of entries: {0}", NumEntries);
                writer.WriteLine();

                foreach (Entry entry in Entries.Where(x => x.NumBytes != 0 && x.StringOffset != 0))
                {
                    writer.WriteLine("-- ID {0} --", entry.ID);
                    writer.WriteLine(entry.String);
                    writer.WriteLine();
                }
            }
        }

        public class Entry
        {
            public uint ID { get; private set; }
            public uint NumBytes { get; private set; }
            public uint StringOffset { get; private set; }
            public uint Padding { get; private set; }

            public string String { get; private set; }

            public Entry(BinaryReader reader)
            {
                ID = reader.ReadUInt32();
                NumBytes = reader.ReadUInt32();
                StringOffset = reader.ReadUInt32();
                Padding = reader.ReadUInt32();

                long streamPosition = reader.BaseStream.Position;
                reader.BaseStream.Seek(StringOffset, SeekOrigin.Begin);

                String = EO4String.GetInstance().GetString(reader.ReadBytes((int)NumBytes));

                reader.BaseStream.Seek(streamPosition, SeekOrigin.Begin);
            }
        }
    }
}
