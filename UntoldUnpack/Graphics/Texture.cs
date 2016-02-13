using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace UntoldUnpack.Graphics
{
    public static class Texture
    {
        public static Bitmap ToBitmap(PicaDataTypes dataType, PicaPixelFormats pixelFormat, int width, int height, Stream inputStream)
        {
            BinaryReader reader = new BinaryReader(inputStream);

            return ToBitmap(dataType, pixelFormat, width, height, reader);
        }

        public static Bitmap ToBitmap(PicaDataTypes dataType, PicaPixelFormats pixelFormat, int width, int height, byte[] data)
        {
            using (MemoryStream inputStream = new MemoryStream(data))
            {
                return ToBitmap(dataType, pixelFormat, width, height, new BinaryReader(inputStream));
            }
        }

        public static Bitmap ToBitmap(PicaDataTypes dataType, PicaPixelFormats pixelFormat, int width, int height, BinaryReader reader)
        {
            TileDecoderDelegate decoder = TileCodecs.GetDecoder(dataType, pixelFormat);

            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            byte[] targetData = new byte[bmpData.Height * bmpData.Stride];
            Marshal.Copy(bmpData.Scan0, targetData, 0, targetData.Length);

            for (int y = 0; y < height; y += 8)
                for (int x = 0; x < width; x += 8)
                    decoder(reader, targetData, x, y, (int)width, (int)height);

            Marshal.Copy(targetData, 0, bmpData.Scan0, targetData.Length);
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }
    }
}
