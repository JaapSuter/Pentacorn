using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Point = System.Drawing.Point;
using PointF = System.Drawing.PointF;
using Size = System.Drawing.Size;

namespace Pentacorn
{
    static class PictureEx
    {
        public static Picture<TColor, TDepth> Copy<TColor, TDepth>(this Picture<TColor, TDepth> that)
            where TColor : struct, IColor
            where TDepth : new()
        {
            var copy = new Picture<TColor, TDepth>(that.Width, that.Height);
            that.Emgu.CopyTo(copy.Emgu);
            return copy;
        }

        public static void Errorize<TColor, TDepth>(this Picture<TColor, TDepth> that)
            where TColor : struct, IColor
            where TDepth : new()
        {
            for (var b = 0; b < that.Bytes.Length; b += 2)
            {
                that.Bytes[b + 0] = byte.MinValue;
                that.Bytes[b + 1] = byte.MaxValue;
            }
        }

        public static void Remap<TColor, TDepth>(this Picture<TColor, TDepth> picture, Matrix<float> undistortX, Matrix<float> undistortY)
            where TColor : struct, IColor
            where TDepth : new()
        {
            using (var copy = picture.Emgu.Copy())
                CvInvoke.cvRemap(copy.Ptr, picture.Emgu.Ptr, undistortX, undistortY, (int)INTER.CV_INTER_LINEAR | (int)Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS, new MCvScalar(255.0f, 0.0f, 255.0f, 255.0f));
        }

        public static void DrawChessboardCorners(this Picture<Gray, byte> picture, PointF[] corners, Size saddleCount)
        {
            CameraCalibration.DrawChessboardCorners(picture.Emgu, saddleCount, corners, true);
            var thickness = 3;
            picture.Emgu.Draw(new LineSegment2DF(corners[0], corners.GridAxisMostX(saddleCount)), new Gray(byte.MaxValue / 2), thickness);
            picture.Emgu.Draw(new LineSegment2DF(corners[0], corners.GridAxisMostY(saddleCount)), new Gray(byte.MaxValue / 2), thickness);
            picture.Emgu.Draw("O: (0, 0)",                                      ref Font, Point.Round(corners.GridOrigin(saddleCount)), new Gray(byte.MaxValue));
            picture.Emgu.Draw("X: ({0}, 0)".FormatWith(saddleCount.Width - 1),  ref Font, Point.Round(corners.GridAxisMostX(saddleCount)), new Gray(byte.MaxValue));
            picture.Emgu.Draw("Y: (0, {0})".FormatWith(saddleCount.Height - 1), ref Font, Point.Round(corners.GridAxisMostY(saddleCount)), new Gray(byte.MaxValue));
        }

        public static async Task<PointF[]> FindChessboardCornersAsync(this Picture<Gray, byte> picture, Size saddleCount, bool disposePictureWhenDone = false)
        {
            picture.AddRef();
            using (picture)
            {
                await Program.SwitchToCompute();

                var dispose = disposePictureWhenDone ? picture : null;
                using (dispose)
                    return picture.FindChessboardCorners(saddleCount);
            }
        }

        public static PointF[] FindChessboardCorners(this Picture<Gray, byte> picture, Size saddleCount)
        {
            PointF[] corners;
            var calibCbType = CALIB_CB_TYPE.FAST_CHECK | CALIB_CB_TYPE.ADAPTIVE_THRESH | CALIB_CB_TYPE.FILTER_QUADS | CALIB_CB_TYPE.NORMALIZE_IMAGE;
            var found = CameraCalibration.FindChessboardCorners(picture.Emgu, saddleCount, calibCbType, out corners);

            // Todo, Jaap Suter, as of OpenCV version 2.2, FindCornerSubPix is supposedly done as part of the FindChessboardCorners, but my 
            // not thoroughly verified impression is that doing it myself improves things nonetheless. I should investigate this in the near future.
            if (found)
                picture.Emgu.FindCornerSubPix(new[] { corners }, new Size(7, 7), new Size(-1, -1), new MCvTermCriteria(10, 0.01));

            return found ? corners : null;
        }

        private static MCvFont Font = new MCvFont(FONT.CV_FONT_HERSHEY_PLAIN, 1.2, 1.2);
    }
}