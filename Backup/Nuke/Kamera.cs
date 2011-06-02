using System;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;

namespace Pentacorn.Vision
{
    class Kamera
    {
        public bool HasCalibratedIntrinsics { get { return Intrinsics != null; } }

        public int ImageWidth { get; private set; }
        public int ImageHeight { get; private set; }

        public double ImageAspectRatio { get { return (double)ImageWidth / (double)ImageHeight; } }
        public double PixelAspectRatio { get; private set; }
        
        public Matrix World { get; private set; }
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }
        public Vector3 Position { get { return World.Translation; } }
        
        public double FovX { get; private set; }
        public double FovY { get; private set; }
        public double FocalLength { get; private set; }
        public MCvPoint2D64f PrincipalPoint { get; private set; }
        public BoundingFrustum BoundingFrustum { get; private set; }

        public Kamera(string name, string uuid, int imageWidth, int imageHeight)
        {
            Name = name;
            Uuid = uuid;
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            IntrinsicsDir = Path.Combine(Global.DatDir, name, uuid.ToSanitizedFileName());
            
            if (!Directory.Exists(IntrinsicsDir))
                Directory.CreateDirectory(IntrinsicsDir);
            if (!Directory.Exists(IntrinsicsDir))
                throw new Exception(String.Format("Unable to create directory at '{0}'.", IntrinsicsDir));
            
            IntrinsicsFile = Path.Combine(IntrinsicsDir, IntrinsicsFileName);
            Intrinsics = TryLoad(IntrinsicsFile, ImageWidth, ImageHeight);

            if (Intrinsics == null)
                return;

            Intrinsics.InitUndistortMap(imageWidth, imageHeight, out UndistortMapX, out UndistortMapY);
            InitializeProjectionMatrix();
        }

        public void CalibrateIntrinsics(PointF[][] imagePoints, MCvPoint3D32f[][] worldPoints)
        {
            if (Intrinsics == null)
                Intrinsics = new IntrinsicCameraParameters();

            var calibType = CALIB_TYPE.CV_CALIB_FIX_K3 | CALIB_TYPE.CV_CALIB_ZERO_TANGENT_DIST;
            ExtrinsicCameraParameters[] extrinsics;

            var totalErr = CameraCalibration.CalibrateCamera(worldPoints, imagePoints, new Size(ImageWidth, ImageHeight), Intrinsics, calibType, out extrinsics);
            var err = Math.Sqrt(totalErr / (imagePoints.Length + imagePoints[0].Length));

            Console.WriteLine("Calibration Finished, Reprojection Error: {0}", err);

            Save(IntrinsicsFile, Intrinsics, ImageWidth, ImageHeight);            
        }

        public void CalibrateExtrinsics(PointF[] imagePoints, MCvPoint3D32f[] worldPoints)
        {
            if (Intrinsics == null)
                throw new Exception("Intrinsics of camera are still unknown, unable to calibrate extrinsic paramters.");
            
            Extrinsics = CameraCalibration.FindExtrinsicCameraParams2(worldPoints, imagePoints, Intrinsics);

            InitializeWorldAndViewMatrices();
            BoundingFrustum = new BoundingFrustum(View * Projection);

            using (var img = new Image<Bgr, byte>(ImageWidth, ImageHeight))
            {
                foreach (var p in imagePoints)
                    img.Draw(new Cross2DF(p, 20, 20), new Bgr(255, 0, 255), 1);

                var projectedCorners = CameraCalibration.ProjectPoints(worldPoints, Extrinsics, Intrinsics);
                foreach (var p in projectedCorners)
                    img.Draw(new Cross2DF(p, 6, 6), new Bgr(255, 255, 0), 1);

                var und = Intrinsics.Undistort(img);

                img.Save(Path.Combine(Global.TmpDir, "reproject.png"));
                und.Save(Path.Combine(Global.TmpDir, "undistorted.png"));
            }
        }

        private void InitializeProjectionMatrix()
        {
            double fovX, fovY, focalLength, pixelAspectRatio;
            MCvPoint2D64f principalPoint;
            Intrinsics.GetIntrinsicMatrixValues(ImageWidth, ImageHeight, 0, 0, out fovX, out fovY, out focalLength, out principalPoint, out pixelAspectRatio);
            FovY = fovY;
            FovX = fovX;
            FocalLength = focalLength;
            PrincipalPoint = principalPoint;
            PixelAspectRatio = pixelAspectRatio;

            Projection = Matrix.CreatePerspectiveFieldOfView((float)(FovY / 180.0f * Math.PI), (float)ImageAspectRatio, NearPlaneDistance, FarPlaneDistance);
        }

        private void InitializeWorldAndViewMatrices()
        {
            var m = Extrinsics.ExtrinsicMatrix;
            View = new Matrix((float)m[0, 0], -(float)m[1, 0], -(float)m[2, 0], 0,
                              (float)m[0, 1], -(float)m[1, 1], -(float)m[2, 1], 0,
                              (float)m[0, 2], -(float)m[1, 2], -(float)m[2, 2], 0,
                              (float)m[0, 3], -(float)m[1, 3], -(float)m[2, 3], 1);
            World = Matrix.Invert(View);
        }

        private static void Save(string path, IntrinsicCameraParameters intr, double imgWidth, double imgHeight)
        {
            var im = intr.IntrinsicMatrix.Clone();
            im[0, 0] /= imgWidth;
            im[0, 2] /= imgWidth;
            im[1, 1] /= imgHeight;
            im[1, 2] /= imgHeight;

            using (var str = File.CreateText(path))
            {
                str.WriteLine(imgWidth);
                str.WriteLine(imgHeight);
                for (int i = 0; i < 5; ++i)
                    str.WriteLine(intr.DistortionCoeffs[i, 0]);
                for (int i = 0; i < 3; ++i)
                    for (int j = 0; j < 3; ++j)
                        str.WriteLine(im[i, j]);
            }
        }

        private static IntrinsicCameraParameters TryLoad(string intrinsicsFile, double imageWidth, double imageHeight)
        {
            if (!File.Exists(intrinsicsFile))
                return null;

            var intr = new IntrinsicCameraParameters();

            using (var str = File.OpenText(intrinsicsFile))
            {
                var sxl = double.Parse(str.ReadLine());
                var syl = double.Parse(str.ReadLine());
                
                for (int i = 0; i < 5; ++i)
                    intr.DistortionCoeffs[i, 0] = double.Parse(str.ReadLine());
                for (int i = 0; i < 3; ++i)
                    for (int j = 0; j < 3; ++j)
                        intr.IntrinsicMatrix[i, j] = double.Parse(str.ReadLine());
            }

            intr.IntrinsicMatrix[0, 0] *= imageWidth;
            intr.IntrinsicMatrix[0, 2] *= imageWidth;
            intr.IntrinsicMatrix[1, 1] *= imageHeight;
            intr.IntrinsicMatrix[1, 2] *= imageHeight;

            return intr;
        }

        private const string IntrinsicsFileName = "intrinsics.txt";
        private const float NearPlaneDistance = 0.1f;
        private const float FarPlaneDistance = 4.0f;

        private IntrinsicCameraParameters Intrinsics;
        private ExtrinsicCameraParameters Extrinsics;
        
        private Matrix<float> UndistortMapX;
        private Matrix<float> UndistortMapY;

        private string Name;
        private string Uuid;
        private string IntrinsicsDir;
        private string IntrinsicsFile;
    }
}
