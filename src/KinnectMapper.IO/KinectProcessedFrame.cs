using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Drawing;

namespace KinectMapper.IO
{
    public class KinectProcessedFrame
    {
        public long Ticks { get; private set; }
        public List<Tuple<Vector3, Color>> Points { get; private set; }

        public KinectProcessedFrame(List<Tuple<Vector3, Color>> points, long ticks)
        {
            this.Ticks = ticks;
            this.Points = points;
        }
    }
}
