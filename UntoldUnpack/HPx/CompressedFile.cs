using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UntoldUnpack.HPx
{
    // @FireyFly
    // http://xen.firefly.nu/up/acmp.c.html
    // http://xen.firefly.nu/up/acmp-js/index.html

    public class CompressedFile
    {
        public CompressedFileHeader Header { get; private set; }
        public byte[] CompressedData { get; private set; }
        public CompressedFileTrailer Trailer { get; private set; }

        byte[] history;
        int i, read, written;

        public byte[] DecompressedData { get; private set; }

        public CompressedFile(Stream stream)
        {
            Header = new CompressedFileHeader(stream);

            long compressedDataStart = stream.Position;

            stream.Seek(Header.CompressedSize - CompressedFileTrailer.ExpectedTrailerSize, SeekOrigin.Current);
            Trailer = new CompressedFileTrailer(stream);

            stream.Seek(compressedDataStart, SeekOrigin.Begin);

            CompressedData = new byte[Header.CompressedSize];
            stream.Read(CompressedData, 0, CompressedData.Length);

            DecompressedData = new byte[Header.DecompressedSize];
            for (int j = 0; j < DecompressedData.Length; j++) DecompressedData[j] = 0xAA;

            Decompress();
        }

        private void Decompress()
        {
            history = new byte[0x8000];
            i = read = written = 0;

            int outOfs = (int)Header.DecompressedSize;
            int inOfs = (int)Header.CompressedSize - Trailer.TrailerSize;

            while (written < Trailer.CompressedSize + Trailer.DecompressedIncrease && inOfs >= 0)
            {
                byte flags = Read(ref inOfs);
                for (int b = 7; b >= 0 && written < Trailer.CompressedSize + Trailer.DecompressedIncrease; b--)
                {
                    if (((flags >> b) & 0x01) == 0x01)
                    {
                        // Copy from history
                        byte x = Read(ref inOfs);
                        int count = (x >> 4) + 3;
                        int offset = (((x & 0x0F) << 8) | Read(ref inOfs)) + 3;

                        for (int j = 0; j < count; j++)
                            Write(ref outOfs, history[(i - offset) & (history.Length - 1)]);
                    }
                    else
                    {
                        // Copy input to output
                        Write(ref outOfs, Read(ref inOfs));
                    }
                }
            }

            while (written < Header.DecompressedSize)
                Write(ref outOfs, Read(ref inOfs));
        }

        private byte Read(ref int inOfs)
        {
            read++;
            return CompressedData[--inOfs];
        }

        private void Write(ref int outOfs, byte value)
        {
            DecompressedData[--outOfs] = history[i] = value;
            written++;
            i = (i + 1) & (history.Length - 1);
        }
    }

    public class CompressedFileHeader
    {
        public const uint ExpectedHeaderSize = 0x20;

        public string MagicNumber { get; private set; }
        public uint CompressedSize { get; private set; }
        public uint HeaderSize { get; private set; }
        public uint Unknown0x0C { get; private set; }
        public uint DecompressedSize { get; private set; }
        public uint Unknown0x14 { get; private set; }
        public uint Unknown0x18 { get; private set; }
        public uint Unknown0x1C { get; private set; }

        public CompressedFileHeader(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            MagicNumber = Encoding.ASCII.GetString(reader.ReadBytes(4), 0, 4);
            CompressedSize = reader.ReadUInt32();
            HeaderSize = reader.ReadUInt32();
            Unknown0x0C = reader.ReadUInt32();
            DecompressedSize = reader.ReadUInt32();
            Unknown0x14 = reader.ReadUInt32();
            Unknown0x18 = reader.ReadUInt32();
            Unknown0x1C = reader.ReadUInt32();
        }
    }

    public class CompressedFileTrailer
    {
        public const uint ExpectedTrailerSize = 0x08;

        uint compressedAndTrailerSize;
        public uint DecompressedIncrease { get; private set; }

        public uint CompressedSize { get { return (uint)(compressedAndTrailerSize & 0xFFFFFF); } }
        public byte TrailerSize { get { return (byte)((compressedAndTrailerSize >> 24) & 0xFF); } }

        public CompressedFileTrailer(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            compressedAndTrailerSize = reader.ReadUInt32();
            DecompressedIncrease = reader.ReadUInt32();
        }
    }
}
