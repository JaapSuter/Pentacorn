using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;

namespace Pentacorn
{
    class Intrinsics
    {
        public PointF PrincipalPoint { get; private set; }
        public Matrix Projection { get; private set; }               
        public Size ImageSize { get; private set; }
        public float ImageAspectRatio { get { return ImageSize.Ratio(); } }
        public double NearPlaneDistance { get; private set; }
        public double FarPlaneDistance { get; private set; }

        public double PixelAspectRatio { get; private set; }
        public double HorizontalFov { get; private set; }
        public double VerticalFov { get; private set; }
        public double FocalLength { get; private set; }        
        public double ReprojectionError { get; private set; }

        public bool ExpectTangentDistortion { get; set; }

        public Intrinsics(Size imageSize, PointF principalPoint, double focalLength, double nearPlaneDistance, double farPlaneDistance, int numRadialDistortionCoefficients)
            : this(imageSize, EstimateLens(imageSize, principalPoint, focalLength), nearPlaneDistance, farPlaneDistance)
        { }

        public Intrinsics(Size imageSize, double nearPlaneDistance, double farPlaneDistance, Stream stream)
            : this(imageSize, LoadCameraParameters(imageSize, stream), nearPlaneDistance, farPlaneDistance)
        { }

        public void Recalibrate(Size imageSize, IEnumerable<PointF[]> worldPoints, IEnumerable<PointF[]> imagePoints, int numRadialDistortionCoefficients)
        {
            double reprojectionError;
            Calibrate(Lens, ImageSize, worldPoints, imagePoints, numRadialDistortionCoefficients, ExpectTangentDistortion, out reprojectionError);
            ReprojectionError = reprojectionError;

            Calculate(ImageSize, Lens, NearPlaneDistance, FarPlaneDistance, ReprojectionError);
        }

        public Matrix LocateExtrinsics(PointF[] worldVertices, PointF[] imagePoints)
        {
            var worldPoints = worldVertices.ToArrayOf(v => new MCvPoint3D32f(v.X, v.Y, 0));
            return CameraCalibration.FindExtrinsicCameraParams2(worldPoints, imagePoints, Lens).ToViewMatrix() * FlipYZ;
        }

        private static void Calibrate(IntrinsicCameraParameters lens, Size imageSize,
            IEnumerable<PointF[]> worldPoints, IEnumerable<PointF[]> imagePoints,
            int numRadialDistortionCoefficients, bool expectTangentDistortion, out double reprojectionError)
        {
            var calibType = CALIB_TYPE.CV_CALIB_USE_INTRINSIC_GUESS;

            if (!expectTangentDistortion) calibType |= CALIB_TYPE.CV_CALIB_ZERO_TANGENT_DIST;
            if (numRadialDistortionCoefficients < 3) calibType |= CALIB_TYPE.CV_CALIB_FIX_K3;
            if (numRadialDistortionCoefficients < 2) calibType |= CALIB_TYPE.CV_CALIB_FIX_K2;
            if (numRadialDistortionCoefficients < 1) calibType |= CALIB_TYPE.CV_CALIB_FIX_K1;

            var worldPointsArray = worldPoints.Select(ps => ps.ToArrayOf(p => new MCvPoint3D32f(p.X, p.Y, 0))).ToArray();
            var imagePointsArray = imagePoints.ToArray();

            WorkAroundForPrincipalPointMustResideWithinImage(lens, imageSize);

            ExtrinsicCameraParameters[] extrinsics;
            reprojectionError = CameraCalibration.CalibrateCamera(worldPointsArray, imagePointsArray, imageSize, lens, calibType, out extrinsics);
        }

        private static IntrinsicCameraParameters EstimateLens(Size imageSize, PointF principalPoint, double focalLength)
        {
            var lens = new IntrinsicCameraParameters();

            lens.IntrinsicMatrix[0, 2] = principalPoint.X;
            lens.IntrinsicMatrix[1, 2] = principalPoint.Y;

            lens.IntrinsicMatrix[0, 0] = focalLength;
            lens.IntrinsicMatrix[1, 1] = focalLength;
            lens.IntrinsicMatrix[2, 2] = 1;

            return lens;
        }

        private Intrinsics(Size imageSize, IntrinsicCameraParameters cameraParameters, double nearPlaneDistance, double farPlaneDistance, double reprojectionError = 0)
        {
            Calculate(imageSize, cameraParameters, nearPlaneDistance, farPlaneDistance, reprojectionError);
        }

        private void Calculate(Size imageSize, IntrinsicCameraParameters cameraParameters, double nearPlaneDistance, double farPlaneDistance, double reprojectionError)
        {
            NearPlaneDistance = nearPlaneDistance;
            FarPlaneDistance = farPlaneDistance;

            ImageSize = imageSize;
            Lens = cameraParameters;
            ReprojectionError = reprojectionError;

            double hfov, vfov, flen, pratio;
            MCvPoint2D64f principal;

            Lens.GetIntrinsicMatrixValues(ImageSize.Width, ImageSize.Height, 0, 0, out hfov, out vfov, out flen, out principal, out pratio);

            FocalLength = flen;
            HorizontalFov = hfov;
            VerticalFov = vfov;
            PixelAspectRatio = pratio;
            PrincipalPoint = new PointF((float)principal.x, (float)principal.y);

            Lens.InitUndistortMap(ImageSize.Width, ImageSize.Height, out UndistortMapX, out UndistortMapY);

            Projection = CreateProjectionFrom(imageSize, principal, flen, NearPlaneDistance, FarPlaneDistance);
        }

        private static Matrix CreateProjectionFrom(Size imageSize, MCvPoint2D64f principalPoint,
                                                  double focalLength, double near, double far)
        {
            var l = near / focalLength * -principalPoint.x;
            var r = near / focalLength * (imageSize.Width - principalPoint.x);
            var t = near / focalLength * principalPoint.y;
            var b = near / focalLength * (principalPoint.y - imageSize.Height);

            return Matrix.CreatePerspectiveOffCenter((float)l, (float)r, (float)b, (float)t, (float)near, (float)far);            
        }

        private static void WorkAroundForPrincipalPointMustResideWithinImage(IntrinsicCameraParameters lens, Size imageSize)
        {
            var im = lens.IntrinsicMatrix;
            
            // OpenCV's calibration routine doesn't like starting with principal points outside of the image, so we just
            // clamp to the image borders, and the calibration itself will then move it back to the suitable place.
            im[0, 2] = Util.Clamp(im[0, 2], 0, imageSize.Width - 1);
            im[1, 2] = Util.Clamp(im[0, 2], 0, imageSize.Height - 1);
        }
        
        private static IntrinsicCameraParameters LoadCameraParameters(Size imageSize, Stream s)
        {
            using (var tr = new StreamReader(s))
            {
                DictionaryFile df = new DictionaryFile(tr);
                var cp = new IntrinsicCameraParameters();

                cp.DistortionCoeffs[CoefficientIndexOfK1, 0] = df.Get<double>("Lens.K1");
                cp.DistortionCoeffs[CoefficientIndexOfK2, 0] = df.Get<double>("Lens.K2");
                cp.DistortionCoeffs[CoefficientIndexOfK3, 0] = df.Get<double>("Lens.K3");

                cp.IntrinsicMatrix[0, 2] = df.Get<double>("Lens.Cx") * imageSize.Width;
                cp.IntrinsicMatrix[1, 2] = df.Get<double>("Lens.Cy") * imageSize.Height;

                cp.IntrinsicMatrix[0, 0] = df.Get<double>("Lens.Fx") * imageSize.Width;
                cp.IntrinsicMatrix[1, 1] = df.Get<double>("Lens.Fy") * imageSize.Height;
                cp.IntrinsicMatrix[2, 2] = 1;                
                
                return cp;
            }
        }

        public void Save(string name, string uuid, Stream s)
        {
            using (var sw = new StreamWriter(s))
            {
                var df = new DictionaryFile();

                df.Set("Lens.K1", Lens.DistortionCoeffs[CoefficientIndexOfK1, 0]);
                df.Set("Lens.K2", Lens.DistortionCoeffs[CoefficientIndexOfK2, 0]);
                df.Set("Lens.K3", Lens.DistortionCoeffs[CoefficientIndexOfK3, 0]);

                df.Set("Lens.Cx", Lens.IntrinsicMatrix[0, 2] / ImageSize.Width);
                df.Set("Lens.Cy", Lens.IntrinsicMatrix[1, 2] / ImageSize.Height);

                df.Set("Lens.Fx", Lens.IntrinsicMatrix[0, 0] / ImageSize.Width);
                df.Set("Lens.Fy", Lens.IntrinsicMatrix[1, 1] / ImageSize.Height);

                df.Set("Name", name);
                df.Set("UUID", uuid);

                df.Set("Image.Width", ImageSize.Width);
                df.Set("Image.Height", ImageSize.Height);
                df.Set("Image.Focal.Length", FocalLength);
                df.Set("Image.AspectRatio", ImageAspectRatio);
                df.Set("Pixel.AspectRatio", PixelAspectRatio);
                df.Set("Reprojection.Error", ReprojectionError);

                df.Set("HorizontalFieldOfView", HorizontalFov);
                df.Set("VerticalFieldOfView", VerticalFov);

                df.Set("NearPlaneDistance", NearPlaneDistance);
                df.Set("FarPlaneDistance", FarPlaneDistance);

                df.Set("Principal.X", PrincipalPoint.X);
                df.Set("Principal.Y", PrincipalPoint.Y);

                df.Save(sw);
            }
        }

        private IntrinsicCameraParameters Lens;
        private Matrix<float> UndistortMapX;
        private Matrix<float> UndistortMapY;

        // Note, index 2 and 3 are tangential distortion
        // coefficients, so K3 comes at 4 - these constants are
        // just to call that out, so another person won't go in
        // and think 0, 1, 4 is a bug and change it to 0, 1, 2
        private const int CoefficientIndexOfK1 = 0;
        private const int CoefficientIndexOfK2 = 1;
        private const int CoefficientIndexOfK3 = 4;

        private static readonly Matrix FlipYZ = Matrix.CreateScale(1, -1, -1);
    }
}
