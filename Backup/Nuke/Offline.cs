using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Microsoft.Xna.Framework;

namespace Pentacorn.Vision
{
    static class Offline
    {
        private class Camera
        {
            public IntrinsicCameraParameters Intrinsics;
            public ExtrinsicCameraParameters Extrinsics;
            public readonly int Width;
            public readonly int Height;
            public Matrix<float> UndistortX;
            public Matrix<float> UndistortY;

            public Matrix World;
            public Matrix View;
            public Matrix Projection;

            public double FovX;
            public double FovY;
            public double FocalLength;
            public double PixelAspectRatio;
            public MCvPoint2D64f PrincipalPoint;
            public BoundingFrustum BoundingFrustum;        

            public Camera(string path, int width, int height, PointF[] imageCorners, MCvPoint3D32f[] worldCorners)
            {
                Width = width;
                Height = height;

                Intrinsics = Markers.Chessboard.Load(path, width, height, out UndistortX, out UndistortY);
                Extrinsics = CameraCalibration.FindExtrinsicCameraParams2(worldCorners, imageCorners, Intrinsics);

                var ext = Extrinsics;
                View = new Matrix((float)ext.ExtrinsicMatrix[0, 0], -(float)ext.ExtrinsicMatrix[1, 0], -(float)ext.ExtrinsicMatrix[2, 0], 0,
                                         (float)ext.ExtrinsicMatrix[0, 1], -(float)ext.ExtrinsicMatrix[1, 1], -(float)ext.ExtrinsicMatrix[2, 1], 0,
                                         (float)ext.ExtrinsicMatrix[0, 2], -(float)ext.ExtrinsicMatrix[1, 2], -(float)ext.ExtrinsicMatrix[2, 2], 0,
                                         (float)ext.ExtrinsicMatrix[0, 3], -(float)ext.ExtrinsicMatrix[1, 3], -(float)ext.ExtrinsicMatrix[2, 3], 1);
                Intrinsics.GetIntrinsicMatrixValues(width, height, 0, 0,
                    out FovX, out FovY,
                    out FocalLength, out PrincipalPoint,
                    out PixelAspectRatio);
                World = Matrix.Invert(View);
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians((float)FovY), ((float)width) / ((float)height), 0.1f, 4.0f);
                BoundingFrustum = new BoundingFrustum(View * Projection);

                if (Global.No)
                    using (var img = new Image<Bgr, byte>(@"C:\Users\Jaap\My Dropbox\Data\Pentacorn.Vision\Offline\Casio Ex S5\Tape on Melamine\CIMG0606.JPG"))
                    {
                        foreach (var p in imageCorners)
                            img.Draw(new Cross2DF(p, 20, 20), new Bgr(255, 0, 255), 1);

                        var projectedCorners = CameraCalibration.ProjectPoints(worldCorners, Extrinsics, Intrinsics);
                        foreach (var p in projectedCorners)
                            img.Draw(new Cross2DF(p, 6, 6), new Bgr(255, 255, 0), 1);

                        var und = Intrinsics.Undistort(img);

                        img.Save(@"C:\Users\Jaap\Temp\img.png");
                        und.Save(@"C:\Users\Jaap\Temp\und.png");
                    }
            }
        }

        public static void Run(Checkers checkers)
        {
            Emgu.CV.Util.OptimizeCV(true);

            PointF[] imageCornersCamera = new PointF[]
            {
                new PointF(3141, 2144),
                new PointF(2035, 2599),
                new PointF(1594, 1503),
                new PointF(2683, 1019),
                new PointF(2363, 1824),
                new PointF(2137, 1258),
                new PointF(1783,  400),
                new PointF( 599, 1948),
            };

            MCvPoint3D32f[] worldCornersCamera = new MCvPoint3D32f[]
            {
                new MCvPoint3D32f(0, 0, 0),
                new MCvPoint3D32f(0, 0.5f, 0),
                new MCvPoint3D32f(0.5f, 0.5f, 0),
                new MCvPoint3D32f(0.5f, 0, 0),
                new MCvPoint3D32f(0.25f, 0.25f, 0),
                new MCvPoint3D32f(0.5f, 0.25f, 0),
                new MCvPoint3D32f(0.9f, 0.25f, 0),
                new MCvPoint3D32f(0.5f, 1.0f, 0),            
            };

            var dstDir = Path.Combine(Global.TmpDir);
            var srcDir = Path.Combine(Global.DatDir, @"Offline\Optoma EW1610\Raw");
            var proPath = Path.Combine(Global.DatDir, @"Offline\Optoma EW1610\intrinsics.txt");
            var camPath = Path.Combine(Global.DatDir, @"Offline\Casio Ex S5\intrinsics.txt");

            var camera = new Camera(camPath, 3648, 2736, imageCornersCamera, worldCornersCamera);

            var proSize = new Size(800, 1280);

            var zPlane = new Plane(Vector3.UnitZ, 0);

            var invertedBlackAndWhite = false;

            var srcFiles = Directory.EnumerateFiles(srcDir, "*.jpg").ToArray();
            PointF[][] imageCorners = new PointF[srcFiles.Length][];
            imageCorners = checkers.GetImagePointsCopy(srcFiles.Length);
            var worldCorners = Projector.GetObjectPointsCopy(srcFiles.Length, checkers.Saddles.Width, checkers.Saddles.Height, (float)checkers.Square.Width, (float)checkers.Square.Height);

            var sw = Stopwatch.StartNew();
            Console.WriteLine("Finding corners in {0} images.", srcFiles.Length);

            Parallel.ForEach(srcFiles, new ParallelOptions() { MaxDegreeOfParallelism = 1, }, (srcFile, loopState, lon) =>
            {
                Console.WriteLine("    Finding corners in image: {0}/{2}, {1}.", lon, srcFile, srcFiles.Length);
                var sw2 = Stopwatch.StartNew();

                using (var img = new Image<Bgr, byte>(srcFile))
                using (var gray = img.Convert<Gray, byte>())
                {
                    if (invertedBlackAndWhite)
                        gray._Not();

                    ImageViewer.Show(gray);

                    var und = camera.Intrinsics.Undistort(img);
                    und.Save(@"C:\Users\Jaap\Temp\und.png");

                    var patternSize = checkers.Saddles;
                    PointF[] corners;
                    var ccbt = CALIB_CB_TYPE.FILTER_QUADS | CALIB_CB_TYPE.ADAPTIVE_THRESH;
                    var found = CameraCalibration.FindChessboardCorners(gray, patternSize, ccbt, out corners);

                    CvInvoke.cvDrawChessboardCorners(img.Ptr, patternSize, corners, corners.Length, patternWasFound: found ? 1 : 0);

                    if (found)
                    {
                        var window = new Size(32, 32);
                        var zeroZone = new Size(-1, -1);
                        var criteria = new MCvTermCriteria(21, 0.0001);
                        Console.WriteLine("    Image {0}/{1} has corners, subpixing begins at {2}.", lon, srcFiles.Length, sw2.Elapsed);
                        gray.FindCornerSubPix(new PointF[][] { corners }, window, zeroZone, criteria);
                        
                        for (int i = 0; i < corners.Length; ++i)
                        {
                            img.Draw(new Cross2DF(corners[i], 20, 20), new Bgr(255, 0, 255), 1);

                            var ray = camera.BoundingFrustum.ThroughPixel(camera.World.Translation, corners[i].X, corners[i].Y, camera.Width, camera.Height);
                            var intersect = ray.Intersects(zPlane);
                            if (intersect.HasValue)
                            {
                                var rayCastWorldCorner = ray.Position + intersect.Value * ray.Direction;
                                worldCorners[lon][i] = new MCvPoint3D32f(rayCastWorldCorner.X, rayCastWorldCorner.Y, rayCastWorldCorner.Z);
                            }                            
                        }

                        corners = CameraCalibration.ProjectPoints(worldCorners[lon], camera.Extrinsics, camera.Intrinsics);
                        for (int i = 0; i < corners.Length; ++i)
                            img.Draw(new Cross2DF(corners[i], 10, 10), new Bgr(255, 255, 0), 1);                        
                    }
                    else
                    {
                        worldCorners[lon] = null;
                    }

                    img.Save(@"C:\Users\Jaap\Temp\img.png");
                    
                    imageCorners[lon] = found ? corners : null;
                    var prefix = found ? "ok." : "failed.";
                    var dstFile = Path.Combine(dstDir, prefix + Path.GetFileNameWithoutExtension(srcFile) + ".png");
                    if (File.Exists(dstFile))
                        File.Delete(dstFile);

                    img.Save(dstFile);

                    if (lon == 0)
                        ImageViewer.Show(img);

                    Console.WriteLine("    Image {0}/{1} is done {2}, found: {3}, time: {4}.", lon, srcFiles.Length, dstFile, found, sw2.Elapsed);
                }
            });

            if (Global.Yes)
                return;

            worldCorners = worldCorners.Where(c => c != null).Select(c => c).ToArray();
            imageCorners = checkers.GetImagePointsCopy(worldCorners.Length);

            var intrinsics = new IntrinsicCameraParameters();

            Console.WriteLine("Found corners in {0}/{1} images, time: {2}.", worldCorners.Length, srcFiles.Length, sw.Elapsed);
            Console.WriteLine("Now calibrating...");
            sw.Restart();

            ExtrinsicCameraParameters[] extrinsics;

            var ct = CALIB_TYPE.CV_CALIB_FIX_K3 | CALIB_TYPE.CV_CALIB_FIX_PRINCIPAL_POINT | CALIB_TYPE.CV_CALIB_ZERO_TANGENT_DIST;
                ct = CALIB_TYPE.CV_CALIB_FIX_K3 | CALIB_TYPE.CV_CALIB_ZERO_TANGENT_DIST;

            var reprojErr = CameraCalibration.CalibrateCamera(worldCorners,
            imageCorners, proSize, intrinsics, ct, out extrinsics);
            var reprojErrPerCornerSquared = reprojErr / (imageCorners.Length * checkers.Saddles.Area());
            var reprojErrPerCorner = Math.Sqrt(reprojErrPerCornerSquared);

            Console.WriteLine("Calibration done in: {0}, reprojection error = {1}, {2}, {3}", sw.Elapsed, reprojErr, reprojErrPerCornerSquared, reprojErrPerCorner);

            Markers.Chessboard.Save(proPath, intrinsics, proSize.Width, proSize.Height);

            Matrix<float> mdx, mdy;
            intrinsics.InitUndistortMap(proSize.Width, proSize.Height, out mdx, out mdy);

            Console.WriteLine("Now undistorting and saving...");
            sw.Restart();

            if (Global.No)
                Parallel.ForEach(srcFiles, new ParallelOptions() { MaxDegreeOfParallelism = 2, }, (srcFile, loopState, lon) =>
                {
                    var dstFile = Path.Combine(dstDir, Path.GetFileNameWithoutExtension(srcFile) + ".undistorted.png");
                    if (File.Exists(dstFile))
                        File.Delete(dstFile);

                    using (var img = new Image<Bgr, byte>(srcFile))
                    using (var jmg = img.Copy())
                    {
                        CvInvoke.cvRemap(jmg.Ptr, img.Ptr, mdx, mdy, (int)INTER.CV_INTER_LINEAR | (int)Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS, new MCvScalar(255.0f, 0.0f, 255.0f, 255.0f));
                        img.Save(dstFile);
                    }

                    Console.WriteLine("    Undistorted {0}/{2}, {1}", lon, dstFile, srcFiles.Length);
                });

            Console.WriteLine("Undistortion done in {0}, press a key to continue.", sw.Elapsed);
            Console.ReadKey();
            Console.WriteLine("One more key please...");
        }
    }
}
