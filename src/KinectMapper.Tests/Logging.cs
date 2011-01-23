using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;
using KinectMapper.IO;
using KinectMapper.Spatial;
using KinectMapper.PostProcessing;
using OpenTK;
using System.Drawing;

namespace KinectMapper.Tests
{
    public class LoggingTests
    {
        [Fact]
        public void TestLoggedKinectData()
        {
            Stream fileStream = new FileStream(@"C:\Temp\2011_1_22_18_11.kinect", FileMode.Open);
            KinectReader reader = new KinectReader(fileStream, 640, 480);
            KinectExporter exporter = new KinectExporter(reader);
            using (Stream outputStream = new FileStream(@"C:\Temp\export.ply", FileMode.Create))
            {
                exporter.ExportToPLYAtId(10, outputStream);
            }
        }

        [Fact]
        public void TestLoggedPositionData()
        {
            Stream fileStream = new FileStream(@"C:\Temp\temp.location", FileMode.Open);
            PositionReader reader = new PositionReader(fileStream);
            using (Stream outputStream = new FileStream(@"C:\Temp\export.xyz", FileMode.Create))
            using(StreamWriter writer = new StreamWriter(outputStream))
            {
                for (int i = 0; i < reader.Count; i++)
                {
                    Tuple<PositionData,long> data = reader.ReadPositionAt(i);
                    writer.WriteLine(String.Format("{0},{1},{2}", data.Item1.X, data.Item1.Y, data.Item1.Z));
                }
            }
        }

        [Fact]
        public void TestRealWorld()
        {
            Stream fileStream = new FileStream(@"C:\Temp\2011_1_22_20_39.kinect", FileMode.Open);
            KinectReader kinectReader = new KinectReader(fileStream, 640, 480);

            Stream positionFileStream = new FileStream(@"C:\Temp\2011_1_22_20_39.location", FileMode.Open);
            PositionReader positionReader = new PositionReader(positionFileStream);

            SpatialReference reference = new SpatialReference(kinectReader, positionReader);

            KinectExporter exporter = new KinectExporter(null);
            using (Stream outputStream = new FileStream(@"C:\Temp\export.ply", FileMode.Create))
            {
                var points = reference.GetRealWorldPointCloudAt(33);

                for (int i = 34; i <50; i++)
                {
                    points.AddRange(reference.GetRealWorldPointCloudAt(i));
                }
                exporter.ExportPointsToPLY(points, outputStream);
            }
        }

        [Fact]
        public void TestLoggedImageData()
        {
            Stream fileStream = new FileStream(@"C:\Temp\temp.kinect", FileMode.Open);
            KinectReader reader = new KinectReader(fileStream, 640, 480);
            Bitmap frame = reader.GetImageAt(3);
            frame.Save(@"C:\Temp\export.bmp");
        }
    }
}
