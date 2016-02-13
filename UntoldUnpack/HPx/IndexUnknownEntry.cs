using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UntoldUnpack.HPx
{
    public class IndexUnknownEntry
    {
        public ushort FirstFileIndex { get; private set; }
        public ushort NumFiles { get; private set; }

        public IndexUnknownEntry(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            FirstFileIndex = reader.ReadUInt16();
            NumFiles = reader.ReadUInt16();
        }
    }
}
