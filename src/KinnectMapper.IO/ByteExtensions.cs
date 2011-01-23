using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace KinectMapper.IO
{
    public static class ByteExtensions
    {
        public static byte[] ToByteArray(this short[] from)
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < from.Length; i++)
                bytes.AddRange(BitConverter.GetBytes(from[i]));
            return bytes.ToArray();
        }

        public static Bitmap ToBitmap(this byte[] data, PixelFormat format, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, format);

            BitmapData bmpData = bmp.LockBits(
                                 new Rectangle(0, 0, bmp.Width, bmp.Height),
                                 ImageLockMode.WriteOnly, bmp.PixelFormat);
            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);

            bmp.UnlockBits(bmpData);

            return bmp;
        }
    }
}
