using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Pentacorn.Graphics;
using Keyx = Microsoft.Xna.Framework.Input.Keys;
using Capture = Pentacorn.Captures.Capture;
using PointF = System.Drawing.PointF;
using Size = System.Drawing.Size;
using Point = System.Drawing.Point;
using Color = Microsoft.Xna.Framework.Color;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Pentacorn;
using Pentacorn.Captures;
using Microsoft.Xna.Framework;
using System.Disposables;

namespace Pentacorn.Tasks
{
    class LocationTask
    {
        public async Task Run(Services services, Capture capture)
        {
            var monitor = services.Monitor;
            
            var tileCount = new Size(10, 7);
            var chessboard = new Chessboard(tileCount) { BlackColor = Color.Black.Alpha(0.6f), WhiteColor = Color.White.Alpha(0.5f), };
            var texture2D = services.Renderer.LeaseFor(capture);
            var circles = new Circle2D[tileCount.Area()];

            var orbitCamera = new OrbitCamera3D(MathHelper.ToRadians(60), monitor.Size.Ratio(), 0.1f, 100) { Orbital = tileCount.CenterF().ToVector3(), };
            var orbitCameraScaleTrans = 3.0f / Math.Min(monitor.Width, monitor.Height) * Math.Max(tileCount.Width, tileCount.Height);
            var orbitCameraScaleRotate = MathHelper.TwoPi / Math.Min(monitor.Width, monitor.Height);

            var virtualCamera = new OrbitCamera3D(capture.Size, capture.Size.CenterF().ToVector2() + new Vector2(0, 40.0f), 570, 0.1f, 20)
            { 
                Orbital = tileCount.CenterF().ToVector3(),
                Distance = 12,            
            };

            var secondary = new Window("Pentacorn Capture", Screen.PrimaryScreen) { ClientSize = capture.Size };
            var projector = new Window("Pentacorn Projector", Screen.AllScreens.FirstOrDefault(s => s != Screen.PrimaryScreen) ?? Screen.PrimaryScreen) { FullScreen = true };
                        
            var frustReal = null as Frustum;
            var frustVirt = null as Frustum;
            var frustProj = null as Frustum;
            
            var objects = new List<object>()
            {
                MakeColoredAxisOfLength(Vector3.UnitX, Color.Red, length: 20.0f),
                MakeColoredAxisOfLength(Vector3.UnitY, Color.Green, length: 20.0f),
                MakeColoredAxisOfLength(Vector3.UnitZ, Color.Blue, length: 20.0f),

                new Grid(Matrix.CreateRotationX(MathHelper.PiOver2),  Color.Red.Lerp(Color.White, 0.9f).Alpha(0.3f), Color.Red),
                new Grid(Matrix.CreateRotationY(-MathHelper.PiOver2), Color.Green.Lerp(Color.White, 0.9f).Alpha(0.3f), Color.Green),
                new Grid(Matrix.Identity,                             Color.Blue.Lerp(Color.White, 0.9f).Alpha(0.3f), Color.Blue),

                chessboard,
            };

            var spaceTrigger = from key in monitor.KeyDown where key.KeyData == Keys.Space select key;
            var closeTrigger = from close in monitor.FormClosingObservable select close;

            monitor.Show();
            secondary.Show();
            projector.Show();

            var scbReprojectionError = 0.0;
            var scbIntrinsics = null as Intrinsics;
            var scbHomographyEmgu = null as HomographyMatrix;
            var scbHomography = Matrix.Identity;
            var scbSaddles = new List<PointF[]>();
            var scbLatch = false;
            var scbTake = false;
            var scbFixedCamera = new FixedCamera3D();
            var scbWorldSaddles = chessboard.Saddles.ToArrayOf(p => new MCvPoint3D32f(p.X, p.Y, 0));
            
            var undistortLatch = false;
            services.Input.Where(input => input.KeyDown(Keyx.U)).Subscribe(input => undistortLatch = !undistortLatch);

            var circleLatch = false;
            services.Input.Where(input => input.KeyDown(Keyx.C)).Subscribe(input => circleLatch = !circleLatch);

            var pcbLatch = false;
            services.Input.Where(input => input.KeyDown(Keyx.P)).Subscribe(input => pcbLatch = true);

            var pcbSaddles = new List<PointF[]>();
            var pcbImageSaddles = chessboard.Saddles;
            var pcbIntrinsics = null as Intrinsics;            
            var pcbName = "Optoma EW1610";
            var pcbUuid = "Jaap";
            var pcbIntrinsicsPath = Path.Combine(Global.DatDir, pcbName + ".txt");
            new SculptChessboardTask().Run(services, tileCount, projector, (saddles) => { pcbImageSaddles = saddles; return pcbLatch && saddles != null; });
            
            if (File.Exists(capture.PathToIntrinsics))
                using (var fs = File.OpenText(capture.PathToIntrinsics))
                    scbIntrinsics = new Intrinsics(capture.Name, capture.Uuid, capture.Size, fs);

            if (File.Exists(pcbIntrinsicsPath))
                using (var fs = File.OpenText(pcbIntrinsicsPath))
                    scbIntrinsics = new Intrinsics(pcbName, pcbUuid, projector.Size, fs);
            
            using (AttachInputToCamera(services.Input, orbitCamera, virtualCamera, orbitCameraScaleTrans, orbitCameraScaleRotate))
            using (services.Input.Subscribe(input => scbTake = scbTake || input.KeyDown(Keyx.Space)))
            using (capture.Subscribe(picture =>
            {
                using (picture)
                {
                    if (scbIntrinsics != null)
                        if (undistortLatch)
                            scbIntrinsics.Undistort(picture);
                
                    texture2D.SetData(picture.Bytes);
                }
            }))
            using (capture.Subscribe(async picture => 
            {
                if (scbLatch)
                    using (picture)
                        return;

                scbLatch = true;
                await services.SwitchToCompute();

                using (Disposable.Create(() => scbLatch = false))
                using (picture)
                {
                    var saddles = await picture.FindChessboardCornersAsync(chessboard.SaddleCount);
                    if (saddles == null)
                        return;

                    await services.SwitchToRender();

                    for (int i = 0; i < saddles.Length; ++i)
                        circles[i] = new Circle2D(saddles[i].ToVector3(), innerRadius: 10, thickness: 4, color: Palette.Get(i)) { AlreadyInScreenSpace = true, };

                    if (pcbLatch)
                    {
                        if (scbHomographyEmgu != null)
                            scbHomographyEmgu.ProjectPoints(saddles);
                        else
                            Console.WriteLine("Huh?");

                        if (scbTake)
                        {
                            pcbSaddles.Add(saddles);

                            if (pcbSaddles.Count > 8)
                            {
                                ExtrinsicCameraParameters[] extrinsics;

                                var worldSaddles = pcbSaddles.Select(s => s.ToArrayOf(p => new MCvPoint3D32f(p.X, p.Y, 0))).ToArray();
                                var imageSaddles = Enumerable.Repeat(pcbImageSaddles, pcbSaddles.Count).ToArray();

                                pcbIntrinsics = new Intrinsics(pcbName, pcbUuid, projector.Size, pcbIntrinsics,
                                    worldSaddles, imageSaddles, out scbReprojectionError, out extrinsics);

                                using (var fs = File.CreateText(pcbIntrinsicsPath))
                                    pcbIntrinsics.Save(fs);
                            }
                        }
                    
                        if (pcbIntrinsics != null)
                        {
                            var pbcWorldSaddles = saddles.ToArrayOf(p => new MCvPoint3D32f(p.X, p.Y, 0));
                            var view = pcbIntrinsics.LocateExtrinsics(pbcWorldSaddles, pcbImageSaddles).ExtrinsicMatrix.ToMatrixFrom3x4();
                            var proj = pcbIntrinsics.Projection;
                            frustProj = new Frustum(view, proj, null, Matrix.Identity, projector.ClientSize) { Color = Color.Green.Alpha(0.8f), };
                        }
                    }
                    else
                    {
                        scbHomographyEmgu = CameraCalibration.FindHomography(saddles, chessboard.Saddles, HOMOGRAPHY_METHOD.LMEDS, 0);
                        scbHomography = scbHomographyEmgu.ToMatrixFromHomogeneous3x3();
                    
                        if (scbTake && Global.No)
                        {
                            scbSaddles.Add(saddles);
                        
                            if (scbSaddles.Count > 6)
                            {
                                ExtrinsicCameraParameters[] extrinsics;

                                var worldSaddles = Enumerable.Repeat(scbWorldSaddles, scbSaddles.Count).ToArray();
                                var imageSaddles = scbSaddles.ToArray();

                                scbIntrinsics = new Intrinsics(capture.Name, capture.Uuid, capture.Size, scbIntrinsics, worldSaddles, imageSaddles, out scbReprojectionError, out extrinsics);

                                using (var fs = File.CreateText(capture.PathToIntrinsics))
                                    scbIntrinsics.Save(fs);
                            }
                        }

                        if (scbIntrinsics != null)
                        {
                            var view = scbIntrinsics.LocateExtrinsics(scbWorldSaddles, saddles).ExtrinsicMatrix.ToMatrixFrom3x4();
                            var proj = scbIntrinsics.Projection;
                            frustVirt = new Frustum(view, proj, texture2D, scbHomography, capture.Size) { Color = Color.Blue.Alpha(0.7f), };
                            scbFixedCamera.View = view;
                            scbFixedCamera.Projection = proj;
                        }
                    }

                    scbTake = false;
                }
            }))
            for (;;)
            {
                {
                    var virtualCapture = capture as VirtualCapture;
                    if (virtualCapture != null)
                    {
                        var cf = virtualCapture.Begin(services.Renderer);
                        cf.Add(Color.White, virtualCamera);
                        cf.Add(chessboard);
                        await cf.PresentAsync();

                        var sf = secondary.Begin(services.Renderer);
                        sf.Add(new Picture2D(sf.Rectangle, virtualCapture.RenderTarget2D));
                        sf.Add(circles);
                        await sf.PresentAsync();

                        frustReal = new Frustum(virtualCamera.View, virtualCamera.Projection, virtualCapture.RenderTarget2D, Matrix.Identity, virtualCapture.Size) { Color = Color.Red.Alpha(0.7f), };

                    }
                    else
                    {
                        var sf = secondary.Begin(services.Renderer);
                        
                        var screenCamera = new ScreenCamera2D(Matrix.Identity, sf.Size.WorldViewProjectionForPixels2d());
                        sf.Add(screenCamera);
                        sf.Add(new Picture2D(sf.Rectangle, texture2D));
                        
                        if (scbIntrinsics != null)
                            sf.Add(scbFixedCamera, objects);

                        if (circleLatch)
                            sf.Add(screenCamera, circles);

                        await sf.PresentAsync();
                    }

                    var mf = monitor.Begin(services.Renderer);
                    mf.Add(Color.CornflowerBlue, orbitCamera);
                    mf.Add(objects);
                    mf.Add(frustReal, frustVirt, frustProj);
                    mf.Add(new Quad(capture.Size, texture2D) { Homography = scbHomography, Color = Color.Yellow.Alpha(0.2f), });
                    mf.Add(new Picture2D(capture.Size.Scale(0.3f).AnchorWithin(mf.Rectangle, AnchorPoints.BottomLeft), texture2D));
                    mf.Add(String.Format("{0} {1} {2} {3} {4}", scbSaddles.Count, pcbSaddles.Count, pcbLatch, circleLatch, undistortLatch));
                    await mf.PresentAsync();
                }
            }
        }

        private static LineSegment MakeColoredAxisOfLength(Vector3 axis, Color color, float length)
        {
            return new LineSegment(Vector3.Zero, length * axis, color, thickness: 0.1f);
        }

        private static IDisposable AttachInputToCamera(IObservable<Input> observableInput, OrbitCamera3D orbitCamera, OrbitCamera3D virtualCamera, float scaleTrans, float scaleRotate)
        {
            return observableInput.Subscribe(input =>
            {
                var camera = input.KeyDown(Keyx.LeftControl) ? virtualCamera : orbitCamera;

                camera.Distance -= input.WheelDelta;

                if (input.LeftMousePressed)
                    if (input.KeyPressed(Keyx.LeftShift))
                        camera.Orbital += camera.World.Right * input.MouseDelta.X * scaleTrans + camera.World.Up * -input.MouseDelta.Y * scaleTrans;
                    else
                        camera.IncrementYawPitchBy(input.MouseDelta.X * scaleRotate, input.MouseDelta.Y * scaleRotate);
            });
        }
    }
}
