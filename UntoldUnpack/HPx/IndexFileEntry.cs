using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UntoldUnpack.HPx
{
    public class IndexFileEntry
    {
        public uint FilenameOffset { get; private set; }
        public uint FileOffset { get; private set; }
        public uint CompressedFileSize { get; private set; }
        public uint DecompressedFileSize { get; private set; }

        public IndexFileEntry(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            FilenameOffset = reader.ReadUInt32();
            FileOffset = reader.ReadUInt32();
            CompressedFileSize = reader.ReadUInt32();
            DecompressedFileSize = reader.ReadUInt32();
        }
    }
}
