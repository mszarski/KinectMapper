using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace KinectMapper.IO
{
    public static class KinectCalibration
    {
        public static float RawDepthToMeters(int depthValue)
        {
            if (depthValue < 2047)
            {
                return (float)(1.0 / ((double)depthValue * -0.0030711016 + 3.3309495161));
            }
            return 0.0f;
        }

        public static Vector3 DepthToWorld(int x, int y, int depthValue)
        {
            const double fx_d = 1.0 / 5.9421434211923247e+02;
            const double fy_d = 1.0 / 5.9104053696870778e+02;
            const double cx_d = 3.3930780975300314e+02;
            const double cy_d = 2.4273913761751615e+02;

            Vector3 result;
            double depth = RawDepthToMeters(depthValue);
            result.X = (float)((x - cx_d) * depth * fx_d);
            result.Y = (float)((y - cy_d) * depth * fy_d);
            result.Z = (float)(depth);
            return result;
        }

        public static Vector2 WorldToColor(Vector3 pt)
        {

            Matrix4 rotationMatrix = new Matrix4(
                                    new Vector4(9.9984628826577793e-01f, 1.2635359098409581e-03f, -1.7487233004436643e-02f, 0),
                                    new Vector4(-1.4779096108364480e-03f, 9.9992385683542895e-01f, -1.2251380107679535e-02f, 0),
                                    new Vector4(1.7470421412464927e-02f, 1.2275341476520762e-02f, 9.9977202419716948e-01f, 0),
                                    new Vector4(0, 0, 0, 1));
            Vector3 translation = new Vector3(1.9985242312092553e-02f, -7.4423738761617583e-04f, -1.0916736334336222e-02f);
            rotationMatrix.Transpose();
            Matrix4 finalMatrix = rotationMatrix * Matrix4.CreateTranslation(-translation);

            const double fx_rgb = 5.2921508098293293e+02f;
            const double fy_rgb = 5.2556393630057437e+02f;
            const double cx_rgb = 3.2894272028759258e+02f;
            const double cy_rgb = 2.6748068171871557e+02f;

            Vector3 transformedPos = Vector3.Transform(pt, finalMatrix);
            float invZ = 1.0f / transformedPos.Z;

            Vector2 result;
            result.X = (float)Math.Round((transformedPos.X * fx_rgb * invZ) + cx_rgb);
            result.Y = (float)Math.Round((transformedPos.Y * fy_rgb * invZ) + cy_rgb);
            return result;
        }
    }
}
