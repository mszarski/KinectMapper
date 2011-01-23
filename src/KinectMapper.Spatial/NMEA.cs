using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KinectMapper.Spatial
{
    public struct GPGGAString
    {
        public int Hour;
        public int Minute;
        public int Second;
        public double Latitude;
        public CompassDirection LatitudeHemisphere;
        public double Longitude;
        public CompassDirection LongitudeHemisphere;
        public GPSQuality GPSQuality;
        public int NumSats;
        public double HDOP;
        public double Altitude;
    }

    public enum GPSQuality : uint
    {
        NoFix = 0,
        Fix = 1,
        DiffFix = 2
    }

    public enum CompassDirection : uint
    {
        North = 0,
        South = 1,
        East = 2,
        West = 3,
    }

    public static class NMEA
    {
        public static GPGGAString ProcessGPGGA(string data)
        {
            GPGGAString GPGGA = new GPGGAString();

            try
            {
                //skip the message type
                data = data.Substring(data.IndexOf(',')+1);

                string[] fields = Regex.Split(data, ",");

                GPGGA.Hour = Convert.ToInt32(fields[0].Substring(0, 2));
                GPGGA.Minute = Convert.ToInt32(fields[0].Substring(2, 2));
                GPGGA.Second = Convert.ToInt32(fields[0].Substring(4, 2));

                GPGGA.Latitude = Convert.ToDouble(fields[1]) / 100;
                if (fields[2] == "S")
                    GPGGA.LatitudeHemisphere = CompassDirection.South;
                else
                    GPGGA.LatitudeHemisphere = CompassDirection.North;

                GPGGA.Longitude = Convert.ToDouble(fields[3]) / 100;
                if (fields[4] == "E")
                    GPGGA.LatitudeHemisphere = CompassDirection.East;
                else
                    GPGGA.LatitudeHemisphere = CompassDirection.West;

                GPGGA.GPSQuality = (GPSQuality)Convert.ToUInt32(fields[5]);

                GPGGA.NumSats = Convert.ToInt32(fields[6]);

                GPGGA.HDOP = Convert.ToDouble(fields[7]);

                GPGGA.Altitude = Convert.ToDouble(fields[8]);

            }
            catch (Exception e)
            {
               
            }

            return GPGGA;
        }
    }
}
