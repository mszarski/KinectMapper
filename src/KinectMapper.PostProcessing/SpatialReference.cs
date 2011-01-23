using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using KinectMapper.IO;
using KinectMapper.Spatial;
using OpenTK;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace KinectMapper.PostProcessing
{
    public class SpatialReference
    {
        private KinectReader kinectReader;

        private PositionReader positionReader;

        private ICoordinateSystem wgs84;
        private ICoordinateSystem toCs;
        
        private ICoordinateTransformation toGridtrans;

        public SpatialReference(KinectReader kinectReader, PositionReader positionReader)
        {
            this.kinectReader = kinectReader;
            this.positionReader = positionReader;

            string wkt = "GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137,298.257223563]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.0174532925199433]]";
            this.wgs84 = (ICoordinateSystem)ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(wkt);

            //hardcoded UTM 55S - should parameterize
            string toWKT = "PROJCS[\"WGS 72BE / UTM zone 55S\",GEOGCS[\"WGS 72BE\",DATUM[\"WGS_1972_Transit_Broadcast_Ephemeris\",SPHEROID[\"WGS 72\",6378135,298.26,AUTHORITY[\"EPSG\",\"7043\"]],TOWGS84[0,0,1.9,0,0,0.814,-0.38],AUTHORITY[\"EPSG\",\"6324\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4324\"]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER[\"central_meridian\",147],PARAMETER[\"scale_factor\",0.9996],PARAMETER[\"false_easting\",500000],PARAMETER[\"false_northing\",10000000],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],AUTHORITY[\"EPSG\",\"32555\"]]";

            this.toCs = (ICoordinateSystem)ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(toWKT);

            CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();

            this.toGridtrans = ctfac.CreateFromCoordinateSystems(wgs84, toCs);
        }

        public KinectProcessedFrame GetRealWorldFrameAt(int id)
        {
            KinectProcessedFrame frame = this.kinectReader.ReadFrameAt(id);

            int positionIdx = this.positionReader.Index.BinarySearch(new Tuple<long,long>(frame.Ticks,0),null);

            if (positionIdx < 0)
                positionIdx = ~positionIdx;
            PositionData position = this.positionReader.ReadPositionAt(positionIdx).Item1;

            //transform coordinates
            double[] fromPoint = new double[] { position.X, position.Y };
            double[] toPoint = this.toGridtrans.MathTransform.Transform(fromPoint);

            //estimate heading
            PositionData nextPosition = this.positionReader.ReadPositionAt(positionIdx + 1).Item1;
            fromPoint[0] = nextPosition.X;
            fromPoint[1] = nextPosition.Y;
            double[] nextPoint = this.toGridtrans.MathTransform.Transform(fromPoint);

            double bearing = Math.Atan2(nextPoint[1] - toPoint[1], nextPoint[0] - toPoint[0]);

            //sensor was on the side of the car so adjust bearing. Should build in a real definition for external sensor calibration
            position.Yaw = bearing - Math.PI/2;

            //crappy HDOP means adjacent clouds might have height discontinuities
            //position.Z = 0;

            for (int i = 0; i < frame.Points.Count; i++)
            {
                var point = frame.Points[i];
                Vector3 realWorldPoint = new Vector3();
                Matrix4 rotationY = Matrix4.CreateRotationY((float)position.Yaw);
                Matrix4 rotationX = Matrix4.CreateRotationX((float)position.Roll);
                Matrix4 rotationZ = Matrix4.CreateRotationZ((float)position.Pitch);

                Matrix4 rotation = Matrix4.Mult(rotationY, rotationX);
                rotation = Matrix4.Mult(rotation, rotationZ);

                Vector3 rotatedPoint = Vector3.TransformVector(point.Item1, rotation);

                realWorldPoint.X = rotatedPoint.Z + (float)toPoint[0];
                realWorldPoint.Y = rotatedPoint.X + (float)toPoint[1]/10000;
                realWorldPoint.Z = rotatedPoint.Y + (float)position.Z;
                frame.Points[i] = new Tuple<Vector3, System.Drawing.Color>(realWorldPoint, point.Item2);
            }

            return frame;
        }

        public List<Tuple<Vector3, Color>> GetRealWorldPointCloudAt(int id)
        {
            return this.GetRealWorldFrameAt(id).Points;
        }
    }
}
