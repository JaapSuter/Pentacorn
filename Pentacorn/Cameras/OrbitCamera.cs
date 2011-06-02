using System.Diagnostics;
using System.Drawing;
using Microsoft.Xna.Framework;

namespace Pentacorn.Graphics
{
    class OrbitCamera : Camera
    {
        public float Distance { get { return Dist; } set { Dist = value; Refresh(); } }
        public Vector3 Center { get { return Cent; } set { Cent = value; Refresh(); } }
        public Vector2 YawPitch { get { return YP; } set { YP = value; Refresh(); } }

        public OrbitCamera(string name, string uuid, Size imageSize, PointF principalPoint, double focalLength, double nearPlaneDistance, double farPlaneDistance)
            : base(name, uuid, imageSize, principalPoint, focalLength, nearPlaneDistance, farPlaneDistance, 0)
        {
            Distance = (float)(nearPlaneDistance + 0.1 * (farPlaneDistance - nearPlaneDistance));
            
            Refresh();

            Debug.Assert(Util.Equalish(focalLength, Intrinsics.FocalLength));
        }

        private void Refresh()
        {
            var orient = Matrix.CreateFromYawPitchRoll(YP.X, YP.Y, 0);
            var center = Matrix.CreateTranslation(Cent);
            var distan = Matrix.CreateTranslation(Dist * orient.Backward);

            World = orient * center * distan;
            View = Matrix.Invert(World);
        }

        private float Dist;
        private Vector3 Cent;
        private Vector2 YP;
    }
}
