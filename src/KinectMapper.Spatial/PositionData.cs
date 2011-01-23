using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectMapper.Spatial
{
    public class PositionData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public double Pitch { get; set; }
        public double Yaw { get; set; }
        public double Roll { get; set; }

        public PositionData(double x, double y, double z, double pitch, double yaw, double roll)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Pitch = pitch;
            this.Yaw = yaw;
            this.Roll = roll;
        }
    }
}
