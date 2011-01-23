using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectMapper.Spatial
{
    public class PositionEventArgs : System.EventArgs
    {
        public PositionData Position { get; private set; }

        public PositionEventArgs(PositionData position)
        {
            this.Position = position;
        }
    }
}
