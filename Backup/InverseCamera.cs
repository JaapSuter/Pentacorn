using System;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Drawing;
using Pentacorn.Captures;
using System.IO;
using Emgu.CV.Structure;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;
using Emgu.CV;
using System.Threading.Tasks;
using Capture = Pentacorn.Captures.Capture;

namespace Pentacorn.Graphics
{
    class InverseCamera : IViewProject
    {
        public Matrix View { get; private set; }
        public Matrix Projection { get { return Intrinsics.Projection; } }
        public Frustum Frustum { get; private set; }
        public Texture2D Texture2D { get; private set; }

        public InverseCamera(Screen screen)
            : this(screen, DefaultNearPlaneDistance, DefaultFarPlaneDistance, DefaultVerticalFov, screen.Bounds.Size.CenterF(), DefaultNumRadialDistortionCoefficients)
        {}

        public InverseCamera(Screen screen, double nearPlaneDistance, double farPlaneDistance, double verticalFovEstimate, PointF principalPointEstimate, int numRadialDistortionCoefficients)
        {
            View = Matrix.Identity;
            Screen = screen;
            NumRadialDistortionCoefficients = numRadialDistortionCoefficients;

            PathToIntrinsics = Path.Combine(Global.DatDir, screen.DeviceName.ToSanitizedFileName() + ".txt");
            if (File.Exists(PathToIntrinsics))
                using (var fs = File.OpenRead(PathToIntrinsics))
                    Intrinsics = new Intrinsics(Screen.Bounds.Size, nearPlaneDistance, farPlaneDistance, fs);
            else
                Intrinsics = new Intrinsics(Screen.Bounds.Size, nearPlaneDistance, farPlaneDistance, verticalFovEstimate, principalPointEstimate, numRadialDistortionCoefficients);

            Frustum = new Frustum(this, Texture2D, Matrix.Identity, Screen.Bounds.Size); 
        }

        public IObserver<Chessboard> AsIndirectChessboardObserverAsync(CaptureCamera camera)
        {
            return Observer.Create<Chessboard>(chessboard =>
            {
                // Calibrate(chessboard.Saddles, imagePoints, listOfImagePoints);
                // Locate(chessboard.Saddles, imagePoints);                
            });
        }

        private void Locate(PointF[] worldPoints, PointF[] imagePoints)
        {
            View = Intrinsics.LocateExtrinsics(worldPoints, imagePoints);
            World = Matrix.Invert(View);
        }

        private void Calibrate(PointF[] worldPoints, PointF[] imagePoints, List<PointF[]> listOfImagePoints)
        {
            var minCapturesNecessaryForCalibration = 7;
            
            listOfImagePoints.Add(imagePoints);
            if (listOfImagePoints.Count < minCapturesNecessaryForCalibration)
                return;

            var listOfWorldPoints = Enumerable.Repeat(worldPoints.ToArrayOf(p => new MCvPoint3D32f(p.X, p.Y, 0)), listOfImagePoints.Count);

            Intrinsics.Recalibrate(Screen.Bounds.Size, listOfWorldPoints, listOfImagePoints, NumRadialDistortionCoefficients);

            using (var fs = File.Create(PathToIntrinsics))
                Intrinsics.Save(Screen.DeviceName, Screen.Bounds.ToString(), fs);
        }

        private int NumRadialDistortionCoefficients;
        private Matrix World = Matrix.Identity;
        private Screen Screen;
        private Intrinsics Intrinsics;
        private string PathToIntrinsics;

        private const double DefaultVerticalFov = 50;
        private const double DefaultNearPlaneDistance = 1;
        private const double DefaultFarPlaneDistance = 100;
        private const int DefaultNumRadialDistortionCoefficients = 1;
    }
}
