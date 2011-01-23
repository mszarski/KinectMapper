using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK;
using System.Drawing;

namespace KinectMapper.IO
{
    public class KinectExporter
    {
        private KinectReader reader;

        public KinectExporter(KinectReader reader)
        {
            this.reader = reader;
        }

        public void ExportPointsToPLY(List<Tuple<Vector3, Color>> points, Stream outputStream)
        {
            //PLY ascii format with RGB
            using (StreamWriter writer = new StreamWriter(outputStream))
            {
                writer.WriteLine("ply");
                writer.WriteLine("format ascii 1.0");
                writer.WriteLine("comment author: Martin Szarski");
                writer.WriteLine("comment object: Kinect Scan");
                writer.WriteLine(String.Format("element vertex {0}", points.Count));
                writer.WriteLine("property float x");
                writer.WriteLine("property float y");
                writer.WriteLine("property float z");
                writer.WriteLine("property uchar red");
                writer.WriteLine("property uchar green");
                writer.WriteLine("property uchar blue");
                writer.WriteLine("end_header");

                foreach (var point in points)
                {
                    writer.WriteLine(String.Format("{0} {1} {2} {3} {4} {5}", point.Item1.X, point.Item1.Y, point.Item1.Z, point.Item2.R, point.Item2.G, point.Item2.B));
                }
            }
        }

        public void ExportToPLYAtId(int id, Stream outputStream)
        {
            var points = this.reader.GetPointCloudAt(id);
            ExportPointsToPLY(points, outputStream);
        }
            
    }
}
