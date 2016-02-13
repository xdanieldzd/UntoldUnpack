using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

using UntoldUnpack.Graphics;

namespace UntoldUnpack
{
    public class STEX
    {
        public const string ExpectedMagicNumber = "STEX";

        public string MagicNumber { get; private set; }
        public uint ConstantZero { get; private set; }
        public uint Constant3553 { get; private set; }
        public uint Width { get; private set; }
        public uint Height { get; private set; }
        public PicaDataTypes DataType { get; private set; }
        public PicaPixelFormats PixelFormat { get; private set; }
        public uint NumImageBytes { get; private set; }
        public uint ImageOffset { get; private set; }
        public byte[] RawPixelData { get; private set; }

        public STEX(string filePath)
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                MagicNumber = Encoding.ASCII.GetString(reader.ReadBytes(4), 0, 4);
                if (MagicNumber != ExpectedMagicNumber) throw new Exception("Invalid STEX file");

                ConstantZero = reader.ReadUInt32();
                Constant3553 = reader.ReadUInt32();
                Width = reader.ReadUInt32();
                Height = reader.ReadUInt32();
                DataType = (PicaDataTypes)reader.ReadUInt32();
                PixelFormat = (PicaPixelFormats)reader.ReadUInt32();
                NumImageBytes = reader.ReadUInt32();
                ImageOffset = reader.ReadUInt32();

                if (ImageOffset == 0x80 || NumImageBytes + ImageOffset == reader.BaseStream.Length)
                {
                    reader.BaseStream.Seek(ImageOffset, SeekOrigin.Begin);
                    RawPixelData = reader.ReadBytes((int)(NumImageBytes > reader.BaseStream.Length ? reader.BaseStream.Length - ImageOffset : NumImageBytes));
                }
                else
                {
                    reader.BaseStream.Seek(-4, SeekOrigin.Current);
                    RawPixelData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
                }
            }
        }

        public Bitmap ToBitmap()
        {
            return Texture.ToBitmap(DataType, PixelFormat, (int)Width, (int)Height, RawPixelData);
        }
    }
}
