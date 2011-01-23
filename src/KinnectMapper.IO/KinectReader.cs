using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK;

namespace KinectMapper.IO
{
    public class KinectReader
    {
        private BinaryReader reader;
        public int Height { get; private set; }
        public int Width { get; private set; }

        public int Count { get; private set; }

        public List<Tuple<long, long>> Index = new List<Tuple<long, long>>();

        public KinectReader(Stream fileStream, int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.reader = new BinaryReader(fileStream);
            this.createIndexes();
        }

        private void createIndexes()
        {
            this.reader.BaseStream.Seek(0, SeekOrigin.Begin);
            while (this.reader.BaseStream.Position < this.reader.BaseStream.Length)
            {
                try
                {
                    long position = this.reader.BaseStream.Position;
                    long ticks = this.reader.ReadInt64();
                    int depthByteCount = this.reader.ReadInt32();
                    this.reader.BaseStream.Position += depthByteCount;
                    int imgByteCount = this.reader.ReadInt32();
                    this.reader.BaseStream.Position += imgByteCount;

                    this.Index.Add(new Tuple<long, long>(ticks, position));
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    break;
                }

                this.Count = this.Index.Count;
            }
        }

        public KinectProcessedFrame ReadFrameAt(int id)
        {
            List<Tuple<Vector3, Color>> points = new List<Tuple<Vector3, Color>>();

            long position = this.Index[id].Item2;
            this.reader.BaseStream.Seek(position, SeekOrigin.Begin);

            long ticks = this.reader.ReadInt64();
            int depthByteCount = this.reader.ReadInt32();

            byte[] depth = this.reader.ReadBytes(depthByteCount);

            int imgByteCount = this.reader.ReadInt32();
            byte[] image = this.reader.ReadBytes(imgByteCount);

            int idx = 0;

            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    Vector3 coord = KinectCalibration.DepthToWorld(x, y, BitConverter.ToUInt16(depth, idx));
                    Vector2 imagePixel = KinectCalibration.WorldToColor(coord);
                    int imgIdx = y * this.Width * 3 + x * 3;
                    Color colour = Color.FromArgb(image[imgIdx], image[imgIdx + 1], image[imgIdx + 2]);
                    points.Add(new Tuple<Vector3, Color>(coord, colour));
                    idx += 2;
                }
            }

            return new KinectProcessedFrame(points, ticks);
        }

        public List<Tuple<Vector3, Color>> GetPointCloudAt(int id)
        {
            return this.ReadFrameAt(id).Points;
        }

        public Bitmap GetImageAt(int id)
        {
            long position = this.Index[id].Item2;
            this.reader.BaseStream.Seek(position, SeekOrigin.Begin);

            long ticks = this.reader.ReadInt64();
            int depthByteCount = this.reader.ReadInt32();

            byte[] depth = this.reader.ReadBytes(depthByteCount);

            int imgByteCount = this.reader.ReadInt32();
            byte[] image = this.reader.ReadBytes(imgByteCount);

            return image.ToBitmap(PixelFormat.Format24bppRgb,640,480);
        }

    }
}
