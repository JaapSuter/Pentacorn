using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pentacorn.Graphics;
using System.Threading.Tasks;
using System.Threading;
using Capture = Pentacorn.Captures.Capture;
using Rectangle = System.Drawing.Rectangle;
using System.Concurrency;
using System.Collections.Generic;

namespace Pentacorn
{
    class PinHole
    {   
        public Intrinsics Intrinsics { get; private set; }

        public BoundingFrustum BoundingFrustum { get; private set; }
        public Matrix World { get; private set; }
        public Matrix View { get; private set; }
        public Vector3 Position { get; private set; }

        public Size ImageSize { get { return Capture.Size; } }
        public int ImageWidth { get { return Capture.Size.Width; } }
        public int ImageHeight { get { return Capture.Size.Height; } }

        public double ImageAspectRatio { get { return (double)ImageWidth / (double)ImageHeight; } }
        public double PixelAspectRatio { get; private set; }
        
        public PinHole(Capture capture, Intrinsics intrinsics, Renderer renderer)
        {
            Intrinsics = intrinsics;
            Capture = capture;
            View = Matrix.Identity;
            CalculateWorldAndFrustum();
        }


        private static Ray[] RaysThrough(IntrinsicCameraParameters intrinsics, PointF[] pixels)
        {
            var rays = new Ray[pixels.Length];
            var undistorted = intrinsics.Undistort(pixels, null, intrinsics.IntrinsicMatrix);

            var cx = intrinsics.IntrinsicMatrix.Data[0, 2];
            var cy = intrinsics.IntrinsicMatrix.Data[1, 2];
            var fx = intrinsics.IntrinsicMatrix.Data[0, 0];
            var fy = intrinsics.IntrinsicMatrix.Data[1, 1];

            var direction = Vector3.Zero;
            for (int i = 0; i < undistorted.Length; ++i)
            {
                PointF pixel = undistorted[i];
                rays[i] = new Ray(Vector3.Zero, new Vector3((float)((pixel.X - cx) / fx), (float)((pixel.Y - cy) / fy), 1));
            }

            return rays;
        }

        private void CalculateWorldAndFrustum()
        {
            World = Matrix.Invert(View);
            BoundingFrustum = new BoundingFrustum(View * Intrinsics.Projection);
        }

        private Capture Capture;        
    }
}
