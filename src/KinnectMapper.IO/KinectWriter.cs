using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KinectMapper.IO
{
    public class KinectWriter
    {
        private BinaryWriter writer;

        public KinectWriter(string fileName)
        {
            this.writer = new BinaryWriter(new FileStream(fileName, FileMode.Create));
        }

        public void Write(KinectRawCompositeFrame data, long ticks)
        {
            writer.Write(ticks);
            byte[] depth = data.depth.ToByteArray();
            writer.Write(depth.Length);
            writer.Write(depth);
            writer.Write(data.image.Length);
            writer.Write(data.image);
        }
    }
}
