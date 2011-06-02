using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;
using Pentacorn.Vision.Captures;
using Pentacorn.Vision.Graphics;
using Pentacorn.Vision.Graphics.Primitives;
using PointF = System.Drawing.PointF;
using Size = System.Drawing.Size;

namespace Pentacorn.Vision
{
    class OldProgram
    {
        private static void Run()
        {
            /*
            PointF[] imageCorners = new PointF[]
            {
                new PointF(10, 10),
                new PointF(20, 20),
                new PointF(30, 30),
                new PointF(40, 40),
                new PointF(10, 40),
                new PointF(40, 10),
            };

            MCvPoint3D32f[] worldCorners = new MCvPoint3D32f[]
            {
                new MCvPoint3D32f(10, 10, 0),
                new MCvPoint3D32f(20, 20, 0),
                new MCvPoint3D32f(30, 30, 0),
                new MCvPoint3D32f(40, 40, 0),
                new MCvPoint3D32f(10, 40, 0),
                new MCvPoint3D32f(40, 10, 0),
            };

            var kamera = new Kamera("Casio Ex S5", "0", 3648, 2736);
            kamera.CalibrateExtrinsics(imageCorners, worldCorners);
            */

            var sqw = 100;
            var m = 7;
            var n = 10;
            var checkers = new Checkers(m, n, new Point((800 - m * sqw) / 2 + 0, (1280 - n * sqw) / 2 - 0), new Size(sqw, sqw))
            {
                BlackColor = Color.Black,
                WhiteColor = Color.White,
                OtherColor = Color.White,
            };

            // ConsoleHelper.Create();

            if (Global.No)
                Offline.Run(checkers);
            else
            {
                var program = new OldProgram();
                if (Global.No)
                    program.CalibrateCamera();
                else
                    program.Run(checkers);
            }

            // ConsoleHelper.Destroy();
        }
        
        private OldProgram()
        {
            r = null;            
        }

        private void Try(Kamera kam)
        {
            PointF[] imageCorners = new PointF[]
            {
                new PointF(110, 110),
                new PointF(220, 420),
                new PointF(520, 1130),
                new PointF(1340, 1440),
                new PointF(1510, 340),
                new PointF(440, 210),
                new PointF(320, 200),
            };

            MCvPoint3D32f[] worldCorners = new MCvPoint3D32f[]
            {
                new MCvPoint3D32f(110, 110, 0),
                new MCvPoint3D32f(220, 420, 0),
                new MCvPoint3D32f(520, 1130, 0),
                new MCvPoint3D32f(1340, 1440, 0),
                new MCvPoint3D32f(1510, 340, 0),
                new MCvPoint3D32f(440, 210, 0),
                new MCvPoint3D32f(320, 200, 0),
            };                    

            kam.CalibrateExtrinsics(imageCorners, worldCorners);
        }

        private async void CalibrateCamera()
        {
            var sqw = 50;
            var M = 8;
            var N = 7;
            var W = m.Size.Width;
            var H = m.Size.Height;
            var checkers = new Checkers(M, N, new Point((W - M * sqw) / 2 + 0, (H - N * sqw) / 2 - 0), new Size(sqw, sqw));
            
            var cap = CLEyeCapture.AllDevices.FirstOrDefault();
            if (cap == null)
                return;

            var kam = new Kamera(cap.Name, cap.Uuid, cap.Width, cap.Height);
            if (kam.HasCalibratedIntrinsics)
            {
                Try(kam);
                return;
            }
            else
            {
                List<PointF[]> imageCorners = new List<PointF[]>();
                List<MCvPoint3D32f[]> worldCorners = new List<MCvPoint3D32f[]>();

                for (; ; )
                {
                    // r.Present(null, m);
                    r.Render<Chessboard2d>(checkers);

                    var key = await KeyPress(m, p, "{0} Photos", imageCorners.Count);
                    if (key == Keys.F1)
                        break;

                    using (var pic = cap.TryDequeue())
                        if (pic != null)
                            using (var gray = pic.Bgra.Convert<Gray, byte>())
                            using (var bgr = pic.Bgra.Convert<Bgr, byte>())
                            {
                                PointF[] foundCorners;
                                var calibChessboardType = CALIB_CB_TYPE.FILTER_QUADS | CALIB_CB_TYPE.ADAPTIVE_THRESH;
                                var found = CameraCalibration.FindChessboardCorners(gray, checkers.Saddles, calibChessboardType, out foundCorners);
                                if (found)
                                {
                                    var window = new Size(sqw * 100 / 10, sqw * 100 / 10);
                                    var zeroZone = new Size(-1, -1);
                                    var criteria = new MCvTermCriteria(21, 0.0001);
                                    gray.FindCornerSubPix(new PointF[][] { foundCorners }, window, zeroZone, criteria);
                                }

                                CvInvoke.cvDrawChessboardCorners(bgr.Ptr, checkers.Saddles, foundCorners, foundCorners.Length, patternWasFound: (found ? 1 : 0));
                                bgr.Save(Path.Combine(Global.TmpDir, cap.Name + imageCorners.Count.ToString() + ".png"));

                                if (found)
                                {
                                    imageCorners.Add(foundCorners);
                                    worldCorners.Add(checkers.GetWorldPointsCopy());
                                }
                            }
                }

                kam.CalibrateIntrinsics(imageCorners.ToArray(), worldCorners.ToArray());
            }
        }

        private static DateTime DateTimeFromInt(int wpfInputTimeStamp)
        {
            var dt = DateTime.Now;
            dt.AddMilliseconds(wpfInputTimeStamp - Environment.TickCount);
            return dt;
        }

        private async void Run(Checkers checkers)
        {
            /*
            m.MouseEvents.Subscribe(mea => 
                {
                    Input.Global.Update(m.Hwnd);
                    r.Render<Circle2d>(new Circle2d.Context() { 
                        Position = new Vector2(mea.X, mea.Y),
                        InnerRadius = 16,
                        OuterRadius = 20,
                        Color = Color.HotPink
                    });

                    r.Render<Circle2d>(new Circle2d.Context() { 
                        Position = Input.Global.MousePosition,
                        InnerRadius = 8,
                        OuterRadius = 12,
                        Color = Color.Beige
                    });
                    
                    r.Render<Circle2d>(new Circle2d.Context()
                    {
                        Position = Input.Global.MousePosition,
                        InnerRadius = 0,
                        OuterRadius = 4,
                        Color = Color.Yellow
                    });                    
                    r.Present(m, m);
                });
            */
            var numPhotosNeeded = 1;
            for (int i = 0; i < numPhotosNeeded; ++i)
            {
                // r.Present(null, p);
                await KeyPress(p, m, "Photo {0}/{1}", i, numPhotosNeeded);
            }

            r.Render<Clear>(Color.Black);
            await KeyPress(p, m, "Black");

            r.Render<Clear>(Color.Gray);
            await KeyPress(p, m, "Gray");

            r.Render<Clear>(Color.White);
            await KeyPress(p, m, "White");

            var sgc = new SafeGrayCode(p.Size.Width);
            for (int i = 0; i < sgc.NumBits; ++i)
            {
                r.Render<GrayCodeSweep>(new GrayCodeSweep.Context()
                    {
                        Bit = i, Sgc = sgc,
                        Sweep = p.Size.Height,
                        Dir = GrayCodeSweep.Dir.Horizontal,
                    });
                await KeyPress(p, m, "Horizontal Gray Code {0}/{1}", i + 1, sgc.NumBits);
            }

            sgc = new SafeGrayCode((int)p.Size.Width);
            for (int i = 0; i < sgc.NumBits; ++i)
            {
                r.Render<GrayCodeSweep>(new GrayCodeSweep.Context()
                {
                    Bit = i,
                    Sgc = sgc,
                    Sweep = (int)p.Size.Height,
                    Dir = GrayCodeSweep.Dir.Vertical,
                });
                await KeyPress(p, m, "Horizontal Gray Code {0}/{1}", i + 1, sgc.NumBits);
            }

            r.Render<Clear>(Color.HotPink);
            await KeyPress(p, m, "Done");
        }

        private async Task<Keys> KeyPress(Window p, Window m, string fmt, params object[] args)
        {
            // r.Present(p, m);
            r.Render<Clear>(Color.DarkGray);
            r.Render(fmt, args);
            // r.Present(m, p);
            return await TaskEx.Run(() => Keys.Q);// return await m.NextKeyUp();
        }

        private Renderer r;
        private Window p;
        private Window m;
    }
}
