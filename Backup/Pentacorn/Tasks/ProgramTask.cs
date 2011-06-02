using System;
using System.Collections.Generic;
using System.Disposables;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Pentacorn.Captures;
using Pentacorn.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using PointF = System.Drawing.PointF;
using Size = System.Drawing.Size;
using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Pentacorn.Tasks
{
    class ProgramTask
    {
        private const int M = 10;
        private const int N = 7;
        private Chessboard Chessboard = new Chessboard(M, N);
        private ColoredChessboard ColoredChessboard;

        private Window Monitor = Program.Monitor;
        
        private Projector Projector;
        
        private List<CaptureCamera> CaptureCameras;
        private OrbitCamera WorldCamera;
        
        private Scene WorldScene;

        public ProgramTask()
        {
            Monitor.ClientSize = new Size(1300, 900);
            ColoredChessboard = new ColoredChessboard(Chessboard, Color.CornflowerBlue.Alpha(0.9f), Color.Orange.Alpha(0.3f));

            var maybeSecondaryScreen = Screen.AllScreens.Where(s => s != Screen.PrimaryScreen).FirstOrDefault() ?? Screen.PrimaryScreen;

            // Projector = new RealProjector("Optoma EW1610", "1018", maybeSecondaryScreen.Bounds.Size, maybeSecondaryScreen);
            var virtualProjectorOrbit = new OrbitCamera("Virtual Projector Orbit", "N/A", new Size(1280, 800), new PointF(640, 850), 100, 2, 134);
            Projector = new VirtualProjector(Monitor, virtualProjectorOrbit);
            
            WorldCamera = new OrbitCamera("World Orbit", "1018", Monitor.Size, Monitor.Size.CenterF(), Monitor.Size.Width, 0.1f, 100)
            {
                Center = Chessboard.VertexCenter,
                Distance = 56,
                YawPitch = new Vector2(0.2f, -0.2f),
            };

            WorldScene = new Scene(WorldCamera)
            {
                MakeColoredAxisOfLength(Vector3.UnitX, Color.Red, length: 20.0f),
                MakeColoredAxisOfLength(Vector3.UnitY, Color.Green, length: 20.0f),
                MakeColoredAxisOfLength(Vector3.UnitZ, Color.Blue, length: 20.0f),
                
                ColoredChessboard,

                new Grid(Matrix.CreateRotationX(MathHelper.PiOver2),  Color.Red.Lerp(Color.White, 0.9f).Alpha(0.3f), Color.Red.Alpha(0.5f)),
                new Grid(Matrix.CreateRotationY(-MathHelper.PiOver2), Color.Green.Lerp(Color.White, 0.9f).Alpha(0.1f), Color.Green.Alpha(0.3f)),
                new Grid(Matrix.Identity,                             Color.Blue.Lerp(Color.White, 0.9f).Alpha(0.1f), Color.Blue.Alpha(0.4f)),

                new MeshModel("ManStanding", Matrix.CreateScale(0.1f) * Matrix.CreateRotationY(-MathHelper.PiOver2) * Matrix.CreateTranslation(4, 0, 2)) { ProjectorViewProject = Projector },
                new MeshModel("MutantStanding", Matrix.CreateScale(0.4f) * Matrix.CreateRotationY(0) * Matrix.CreateTranslation(8, 0, 1)) { ProjectorViewProject = Projector },
                new MeshModel("HeadFemale", Matrix.CreateScale(0.2f) * Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateTranslation(-3, 6, 0)) { ProjectorViewProject = Projector },
            };

            CaptureCameras = CreateCaptureCameras().ToList();
            if (CaptureCameras.IsEmpty())
                AddVirtualCaptureCamera("VirCam 1", "1018", Color.Red);

            CaptureCameraSelectionPictureGray = new Picture2D(CaptureCameraSelected.Intrinsics.ImageSize.LimitProportional(290, 290).AnchorWithin(Monitor.ClientRectangle, AnchorPoints.BottomLeft), CaptureCameraSelected.Capture.GrayTexture2D);
            CaptureCameraSelectionPictureRgba = new Picture2D(CaptureCameraSelected.Intrinsics.ImageSize.LimitProportional(290, 290).ToRect(CaptureCameraSelectionPictureGray.Rectangle.Left, CaptureCameraSelectionPictureGray.Rectangle.Top, AnchorPoints.BottomLeft), CaptureCameraSelected.Capture.RgbaTexture2D);
            CaptureCameraSelected.Highlight = true;

            WorldScene.Add(CaptureCameras);
            WorldScene.Add(Projector);

            Monitor.Scene = new Scene(new ScreenCamera(Monitor))
            {
                new Clear(Color.LightGray),
                WorldScene,
                CaptureCameraSelectionPictureGray,
                CaptureCameraSelectionPictureRgba,
            };

            LocateTrigger = new Trigger(() => new LocateTask(Monitor, CaptureCameraSelected, Chessboard).Run(), Keys.L);
            CalibrateTrigger = new Trigger(() => new CalibrateTask(Monitor, CaptureCameraSelected, Chessboard).Run(CaptureCameraSelected), Keys.C);
            ChessboardShapeTrigger = new Trigger(() => new ChessboardShapeTask(Monitor, Projector, Chessboard).Run(), Keys.S);
            CalibrateIndirectlyTrigger = new Trigger(() => { WorldScene.Remove(ColoredChessboard); return new CalibrateIndirectlyTask(Monitor, CaptureCameras, Projector, Chessboard).Run(WorldScene); }, Keys.I);
            GrayScanTrigger = new Trigger(() => new GrayScanTask(Monitor, CaptureCameras.First(), Projector).Run(WorldScene), Keys.G);

            AttachInputToCamera(Program.WhenInput.Where(input => !input.KeyPressed(Keys.LeftAlt)), Monitor, WorldCamera);
            AttachInputToCamera(Program.WhenInput.Where(input =>  input.KeyPressed(Keys.LeftAlt)), Monitor, virtualProjectorOrbit);

            AttachPerformanceBar();

            Program.WhenInput.Where(input => input.KeyDown(Keys.Tab)).Subscribe(input =>
                {
                    CaptureCameraSelected.Highlight = false;
                    CaptureCameraSelectionIdx = (CaptureCameraSelectionIdx + 1) % CaptureCameras.Count;
                    CaptureCameraSelected.Highlight = true;
                    CaptureCameraSelectionPictureGray.Texture2D = CaptureCameraSelected.Capture.GrayTexture2D;
                    CaptureCameraSelectionPictureRgba.Texture2D = CaptureCameraSelected.Capture.RgbaTexture2D;

                });
        }

        private CaptureCamera CaptureCameraSelected { get { return CaptureCameras[CaptureCameraSelectionIdx]; } } 

        private static Cloud MakeCloudTest()
        {
            using (var dmap = new Image<Gray, byte>(Path.Combine(Global.DatDir, @"Cloud Tests\dmap1.jpg")))
            using (var cmap = new Image<Bgr, byte>(Path.Combine(Global.DatDir, @"Cloud Tests\cmap1.jpg")).Convert<Rgba, byte>())
            using (var mask = new Image<Gray, byte>(Path.Combine(Global.DatDir, @"Cloud Tests\dmap1.jpg")))
            {
                var w = 100.0f;
                var h = 100.0f;
                var z = 5.0f;
            
                var pts = new VertexPositionNormalTexture[dmap.Size.Area()];
                var masked = mask[0, dmap.Width - 1].Intensity;

                for (int y = 0; y < dmap.Height; ++y)
                    for (int x = 0; x < dmap.Width; ++x)
                    {
                        var fx = x / w;
                        var fy = (dmap.Height - y) / h;
                        var fz = (float)dmap[y, x].Intensity / 255.0f * z;

                        if (masked != mask[y, x].Intensity)
                            pts[y * dmap.Width + x] = new VertexPositionNormalTexture(
                                new Vector3(fx, fy, fz),
                                Vector3.UnitZ,
                                new Vector2(x / (float)dmap.Width, y / (float)dmap.Height));

                    }

                GrayScanTask.CalculateNormals(pts, dmap.Width, dmap.Height);                

                using (var ticture = new Picture<Rgba, byte>(cmap.Width, cmap.Height))
                {
                    var texture2D = Program.Renderer.LeaseFor(ticture);
                
                    cmap.CopyTo(ticture.Emgu);
                    texture2D.SetData(ticture.Bytes);
                    return new Cloud(Color.Pink, dmap.Size) { Points = pts, Texture2D = texture2D };
                }
            }
        }

        public async Task Run()
        {
            Monitor.Show();
            Projector.Window.Show();

            while (!Monitor.IsDisposed)
            {
                var trigger = await Program.WhenInput.Where(input => input.KeyDown(Keys.Escape)).TakeNext();
            
                await Program.SwitchToRender();
            }
        }

        private IEnumerable<CaptureCamera> CreateCaptureCameras()
        {
            var cidx = 0;

            foreach (var capture in DirectShowCapture.Devices)
                yield return new CaptureCamera(capture, 2) { Color = Palette.Get(cidx++) };

            foreach (var capture in CLEyeCapture.Devices)
                yield return new CaptureCamera(capture, 2) { Color = Palette.Get(cidx++) };
        }

        private void AttachPerformanceBar()
        {
            var desireable = 14;
            var warning = 28;
            var unacceptable = 43;

            var height = 30;

            var text = new Text("", new Vector2(height, 2 * height), Color.White);
            var minq = new Quad(new Rectangle(0, 0, 1, height), Program.Renderer.LeaseWhiteTexel(), Color.Lime);
            var avgq = new Quad(new Rectangle(0, 0, 1, height / 3 * 2), Program.Renderer.LeaseWhiteTexel(), Color.Yellow);
            var maxq = new Quad(new Rectangle(0, 0, 1, height / 3), Program.Renderer.LeaseWhiteTexel(), Color.Red);

            Monitor.Scene.Add(maxq, avgq, minq, text);

            var whenPerfWindowSize = TimeSpan.FromSeconds(2.3);
            var whenPerfWindow = Program.WhenTick
                                        .Timestamp()
                                        .Scan(Enumerable.Empty<Timestamped<TimeSpan>>(),
                                                (acum, value) => from e in acum.StartWith(value)
                                                                 where e.Timestamp > DateTime.Now.Subtract(whenPerfWindowSize)
                                                                 select e)
                                        .Where(window => window != null && !window.IsEmpty())
                                        .Select(window => new
                                            {
                                                Min = window.Min(ts => ts.Value.TotalMilliseconds),
                                                Average = window.Average(ts => ts.Value.TotalMilliseconds),
                                                Max = window.Max(ts => ts.Value.TotalMilliseconds),
                                            });

            whenPerfWindow.Subscribe(window =>
                {
                    var min = (float)window.Min;
                    var avg = (float)window.Average;
                    var max = (float)window.Max;

                    var gradient = new Func<float, Color>(val =>
                    {
                        var lerp = MathHelper.Clamp(val, desireable, unacceptable);
                        return lerp < warning
                             ? Color.Lerp(Color.Lime, Color.Yellow, (lerp - desireable) / (warning - desireable))
                             : Color.Lerp(Color.Yellow, Color.Red, (lerp - warning) / (unacceptable - warning));
                    });

                    text.String = "{0:0.0} {1:0.0} {2:0.0}".FormatWith(min, avg, max);
                    text.FillColor = gradient(avg);
                    avgq.Color = gradient(avg);
                    minq.Color = gradient(min);
                    maxq.Color = gradient(max);
                    avgq.World = Matrix.CreateScale(avg * 10, 1, 1) * Matrix.CreateTranslation(height, height, 0);
                    minq.World = Matrix.CreateScale(min * 10, 1, 1) * Matrix.CreateTranslation(height, height, 0);
                    maxq.World = Matrix.CreateScale(max * 10, 1, 1) * Matrix.CreateTranslation(height, height, 0);

                });
        }

        private void AddVirtualCaptureCamera(string name, string uuid, Color color)
        {
            var size = new Size(1600, 1200);
            var principal = new PointF(392.713f * 2, 274.4965f * 2);
            var orbitCamera = new OrbitCamera("{0} Orbit".FormatWith(name), uuid, size, principal, size.Width, nearPlaneDistance: 2, farPlaneDistance: 40)
            {
                Color = Color.DarkCyan,
                Center = new Vector3(5, 4, 0),
                Distance = 30, 
                YawPitch = new Vector2(0.1f, 0.1f),
            };
            var virtualCapture = new VirtualCapture(name, uuid, size, fps: 10)
            { 
                Scene = new Scene(orbitCamera)
                { 
                    new Clear(Color.White),
                    Chessboard
                }
            };

            var virtualCaptureCamera = new CaptureCamera(virtualCapture, 0) { Color = color };
            
            WorldScene.Add(orbitCamera);
            CaptureCameras.Add(virtualCaptureCamera);
        }

        private IEnumerable<Vector3> PointsOnSphere(int n)
        {
            var inc = Math.PI * (3 - Math.Sqrt(5));
            var off = 2.0 / n;
            for (var k = 0.0; k < n; ++k)
            {
                var y = k * off - 1 + (off / 2);
                var r = Math.Sqrt(1 - y * y);
                var phi = k * inc;
                yield return new Vector3((float)(Math.Cos(phi) * r),
                                         (float)y,
                                         (float)(Math.Sin(phi) * r));
            }
        }

        private static LineSegment MakeColoredAxisOfLength(Vector3 axis, Color color, float length)
        {
            return new LineSegment(Vector3.Zero, length * axis, color, thickness: 0.1f);
        }


        public static IDisposable AttachInputToCamera(IObservable<Input> whenInput, Window window, OrbitCamera camera)
        {
            var whenFocusedInput = whenInput.Where(input => window.Focused);

            var whenLeftMouseDown = whenFocusedInput.Where(input => input.LeftMouseDown);
            var whenLeftMouseUp = whenFocusedInput.Where(input => input.LeftMouseUp);

            var whenFocusedDrag = from begin in whenLeftMouseDown
                                  let fro = new { YawPitch = camera.YawPitch, Center = camera.Center }
                                  from during in whenFocusedInput.TakeUntil(whenLeftMouseUp)
                                  select new
                                  {
                                      From = fro,
                                      To = during,
                                      Delta = during.MousePosition - begin.MousePosition
                                  };

            var halfOrbitPerPixel = MathHelper.Pi / window.Width * new Vector2(-1, -1);
            var meterPerPixel = 27.0f / window.Width;

            return new CompositeDisposable(

                // Scroll Wheel Camera Distance
                whenFocusedInput.Where(input => input.WheelDelta != 0).Subscribe(input => camera.Distance -= input.WheelDelta),

                // Shift Drag Camera Centroid Translate 
                whenFocusedDrag.Where(d => d.To.KeyPressed(Keys.LeftShift))
                               .Subscribe(drag => camera.Center = drag.From.Center
                                                                + drag.Delta.X * meterPerPixel * camera.World.Left
                                                                + drag.Delta.Y * meterPerPixel * camera.World.Up),

                // Normal Drag Camera Orbit Rotate 
                whenFocusedDrag.Where(d => !d.To.KeyPressed(Keys.LeftShift))
                               .Subscribe(drag => camera.YawPitch = drag.From.YawPitch + drag.Delta * halfOrbitPerPixel));
        }

        private int CaptureCameraSelectionIdx;
        private Picture2D CaptureCameraSelectionPictureGray;
        private Picture2D CaptureCameraSelectionPictureRgba;
        private Trigger CalibrateTrigger;
        private Trigger LocateTrigger;
        private Trigger ChessboardShapeTrigger;
        private Trigger CalibrateIndirectlyTrigger;
        private Trigger GrayScanTrigger;
    }
}
