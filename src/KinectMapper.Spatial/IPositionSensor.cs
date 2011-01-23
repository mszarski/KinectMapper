using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectMapper.Spatial
{
    public interface IPositionSensor
    {
        PositionData LastPosition {get;}

        event EventHandler<PositionEventArgs> PositionReceived;
        void Start();
        void Stop();
        void Connect();
        void Disconnect();
    }
}
