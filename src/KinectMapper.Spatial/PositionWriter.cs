using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KinectMapper.Spatial
{
    public class PositiontWriter
    {
        private BinaryWriter writer;

        public PositiontWriter(string fileName)
        {
            this.writer = new BinaryWriter(new FileStream(fileName, FileMode.Create));
        }

        public void Write(PositionData data, long ticks)
        {
            writer.Write(ticks);
            writer.Write(data.X);
            writer.Write(data.Y);
            writer.Write(data.Z);
            writer.Write(data.Roll);
            writer.Write(data.Pitch);
            writer.Write(data.Yaw);

        }
    }
}
