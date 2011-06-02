using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;
using Pentacorn.Vision.Cameras;
using Size = System.Drawing.Size;

namespace Pentacorn.Vision.Markers
{
    class Chessboard
    {
        public bool Calibrated { get { return intrinsics != null; } }
        public Size Dim { get; private set; }
        public int Count { get { return imageCorners.Count; } }
        public int Max { get { return numTakes; } }

        public Chessboard(int m, int n, string uniqueSourceId, int width, int height)
        {
            this.Dim = new Size(m, n);
            var f = UniqueCamFileName(uniqueSourceId);
            if (File.Exists(f))
                intrinsics = Load(f, width, height, out mapx, out mapy);
        }

        public void MaybeUndistort(Picture picture)
        {
            if (intrinsics != null && this.mapx != null && this.mapy != null)
            {
                using (var other = new Picture(picture.Width, picture.Height))
                {
                    picture.Bgra.CopyTo(other.Bgra);
                    CvInvoke.cvRemap(other.Bgra.Ptr, picture.Bgra.Ptr, mapx, mapy, (int)INTER.CV_INTER_LINEAR | (int)Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS, new MCvScalar(255.0f, 0.0f, 255.0f, 255.0f));
                }
            }
        }

        private void Calibrate(Picture picture, string uniqueSourceId, PhysicalCamera camera)
        {
            var corners = FindChessBoardCorners(picture, uniqueSourceId);
            if (corners == null)
                return;

            if (intrinsics != null)
            {
                extrinsics = CameraCalibration.FindExtrinsicCameraParams2(Projector.GetObjectPointsCopy(), corners, intrinsics);
                var ecp = extrinsics;
                var matrix = new Matrix((float)ecp.ExtrinsicMatrix[0, 0], -(float)ecp.ExtrinsicMatrix[1, 0], -(float)ecp.ExtrinsicMatrix[2, 0], 0,
                                        (float)ecp.ExtrinsicMatrix[0, 1], -(float)ecp.ExtrinsicMatrix[1, 1], -(float)ecp.ExtrinsicMatrix[2, 1], 0,
                                        (float)ecp.ExtrinsicMatrix[0, 2], -(float)ecp.ExtrinsicMatrix[1, 2], -(float)ecp.ExtrinsicMatrix[2, 2], 0,
                                        (float)ecp.ExtrinsicMatrix[0, 3], -(float)ecp.ExtrinsicMatrix[1, 3], -(float)ecp.ExtrinsicMatrix[2, 3], 1);
                camera.World = Matrix.Invert(matrix);
                camera.Localized = true;

                double fovx;
                double fovy;
                double focalLength;
                MCvPoint2D64f principalPoint;
                double pixelAspectRatio;
                intrinsics.GetIntrinsicMatrixValues(picture.Width, picture.Height, 0, 0, out fovx, out fovy, out focalLength, out principalPoint, out pixelAspectRatio);

                camera.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians((float)fovy), ((float)picture.Width) / ((float)picture.Height), 0.1f, 2.0f);
            }
            else if (imageCorners.Count < numTakes)
            {
                CvInvoke.cvDrawChessboardCorners(picture.Bgra.Ptr, this.Dim, corners, corners.Length, patternWasFound: 1);

                imageCorners.Add(corners);

                Picture.ExchangeAndFree(ref camera.calib2E, ref picture);                
            }
            else
            {
                CalibrateWithChessBoardCorners(picture, uniqueSourceId);
            }
        }

        private PointF[] FindChessBoardCorners(Picture picture, string uniqueSourceId)
        {
            using (var gray = picture.Bgra.Convert<Gray, byte>())
            {
                PointF[] corners;
                var ok = CameraCalibration.FindChessboardCorners(gray, this.Dim, CALIB_CB_TYPE.ADAPTIVE_THRESH | CALIB_CB_TYPE.FILTER_QUADS | CALIB_CB_TYPE.NORMALIZE_IMAGE, out corners);
                foreach (var c in corners)
                    if (float.IsNaN(c.X) || float.IsNaN(c.Y))
                        ok = false;

                if (ok)
                {
                    var window = new Size(15, 15);
                    var zeroZone = new Size(-1, -1);
                    var criteria = new MCvTermCriteria(16, 0.001);
                    gray.FindCornerSubPix(new PointF[][] { corners }, window, zeroZone, criteria);

                    picture.Bgra.ConvertFrom(gray);

                    return corners;
                }
            }

            return null;
        }

        private void CalibrateWithChessBoardCorners(Picture picture, string uniqueSourceId)
        {
            MCvPoint3D32f[][] objectPoints = new MCvPoint3D32f[imageCorners.Count][];
            for (int i = 0; i < imageCorners.Count; ++i)
                objectPoints[i] = Projector.GetObjectPointsCopy();
            var imagePoints = imageCorners.ToArray();
            intrinsics = new IntrinsicCameraParameters();
            intrinsics.IntrinsicMatrix[0, 0] = picture.Bgra.Width;
            intrinsics.IntrinsicMatrix[0, 1] = 0;
            intrinsics.IntrinsicMatrix[0, 2] = picture.Bgra.Width / 2.0;
            intrinsics.IntrinsicMatrix[1, 0] = 0;
            intrinsics.IntrinsicMatrix[1, 1] = picture.Bgra.Height;
            intrinsics.IntrinsicMatrix[1, 2] = picture.Bgra.Height / 2;
            intrinsics.IntrinsicMatrix[2, 0] = 0;
            intrinsics.IntrinsicMatrix[2, 1] = 0;
            intrinsics.IntrinsicMatrix[2, 2] = 1;

            var calibrationType = CALIB_TYPE.CV_CALIB_FIX_K3 |
                                  CALIB_TYPE.CV_CALIB_FIX_PRINCIPAL_POINT |
                                  CALIB_TYPE.CV_CALIB_ZERO_TANGENT_DIST;
            ExtrinsicCameraParameters[] extrinsicsArray;

            CameraCalibration.CalibrateCamera(objectPoints, imagePoints, new Size(picture.Bgra.Width, picture.Bgra.Height),
                intrinsics, calibrationType, out extrinsicsArray);

            extrinsics = extrinsicsArray[0];

            Matrix<float> mx, my;
            intrinsics.InitUndistortMap(picture.Width, picture.Height, out mx, out my);
            mapx = mx;
            mapy = my;

            Save(UniqueCamFileName(uniqueSourceId), intrinsics, picture.Width, picture.Height);
        }

        public static void Save(string path, IntrinsicCameraParameters icp, int imgWidth, int imgHeight)
        {
            var im = icp.IntrinsicMatrix.Clone();
            var sx = (double)imgWidth;
            var sy = (double)imgHeight;
            im[0, 0] /= sx;
            im[0, 2] /= sx;
            im[1, 1] /= sy;
            im[1, 2] /= sy;

            using (var str = File.CreateText(path))
            {
                str.WriteLine(sx);
                str.WriteLine(sy);
                for (int i = 0; i < 5; ++i)
                    str.WriteLine(icp.DistortionCoeffs[i, 0]);
                for (int i = 0; i < 3; ++i)
                    for (int j = 0; j < 3; ++j)
                        str.WriteLine(im[i, j]);
            }
        }

        public static IntrinsicCameraParameters Load(string path, int width, int height, out Matrix<float> mdx, out Matrix<float> mdy)
        {
            var icp = new IntrinsicCameraParameters();
            var sx = (double)width;
            var sy = (double)height;
            using (var str = File.OpenText(path))
            {
                var sxl = double.Parse(str.ReadLine());
                var syl = double.Parse(str.ReadLine());
                Console.WriteLine("Intrinsics Load/Used Width {0}/{1} Height {2}/{3}", sxl, sx, syl, sy);
                for (int i = 0; i < 5; ++i)
                    icp.DistortionCoeffs[i, 0] = double.Parse(str.ReadLine());
                for (int i = 0; i < 3; ++i)
                    for (int j = 0; j < 3; ++j)
                        icp.IntrinsicMatrix[i, j] = double.Parse(str.ReadLine());
            }

            icp.IntrinsicMatrix[0, 0] *= sx;
            icp.IntrinsicMatrix[0, 2] *= sx;
            icp.IntrinsicMatrix[1, 1] *= sy;
            icp.IntrinsicMatrix[1, 2] *= sy;

            icp.InitUndistortMap(width, height, out mdx, out mdy);

            return icp;
        }

        private static string UniqueCamFileName(string uniqueSourceId) { return Path.Combine(Global.DatDir, "Shots", uniqueSourceId.ToSanitizedFileName() + ".txt"); }

        private const int numTakes = 17;
        private List<PointF[]> imageCorners = new List<PointF[]>();
        private IntrinsicCameraParameters intrinsics;
        private ExtrinsicCameraParameters extrinsics;
        private Matrix<float> mapx, mapy;

        public static TimeSpan FadeOut = TimeSpan.FromSeconds(0.8);
    }
}
