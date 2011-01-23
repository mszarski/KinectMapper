using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectMapper.IO
{
    public class KinectRawCompositeFrame
    {
        public byte[] image;
        public short[] depth;

        public KinectRawCompositeFrame(short[] depth, byte[] image)
        {
            this.depth = depth;
            this.image = image;
        }
    }
}
