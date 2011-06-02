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
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Tasks
{
    class GrayScanTask : WorkFlowTask
    {
        public GrayScanTask(Window window, CaptureCamera captureCamera, Projector projector)
            : base(window)
        {
            CaptureCamera = captureCamera;
            Projector = projector;
            Projector.Scene = new Scene(new ScreenCamera(Projector.Window));
        }
        
        public async Task Run(Scene worldScene)
        {
            WriteLine("Scanning...");

            var scene = Projector.Scene;
            var size = Projector.Intrinsics.ImageSize;

            WriteLine("Black, White, Color, and Mask...");

            var clearBlack = new Clear(Color.Black);
            await GetBlackWhiteAndMask(scene);

            WriteLine("Horizontal...");

            var horSgc = new SafeGrayCode(size.Width);
            var horGcs = new GrayCodeSweep(horSgc, size.Height, GrayCodeSweep.Direction.Horizontal);            
            scene.Clear();
            scene.Add(horGcs);
            var horAssembly = await SweepGrayCode(horGcs);

            int[] verAssembly = null;
            
            WriteLine("Vertical...");

            var verSgc = new SafeGrayCode(size.Height);
            var verGcs = new GrayCodeSweep(verSgc, size.Width, GrayCodeSweep.Direction.Vertical);
            scene.Clear();
            scene.Add(verGcs);
            verAssembly = await SweepGrayCode(verGcs);

            scene.Clear();
            scene.Add(new Clear(Color.White));

            WriteLine("Making...");

            await Make(horAssembly, verAssembly, worldScene);

            WriteLine("Done...");
        }

        private async Task<int[]> SweepGrayCode(GrayCodeSweep gcs)
        {
            var sweeps = new List<Picture<Gray, byte>>(gcs.Sgc.NumBits);
            using (Disposable.Create(() => sweeps.Run(s => Util.Dispose(ref s))))
            {
                for (int b = 0; b < gcs.Sgc.NumBits; ++b)
                {
                    gcs.Bit = b;
                    using (var positive = await RenderThenCapture("Positive {0} [{1}/{2}]".FormatWith(gcs.Dir, gcs.Bit, gcs.Sgc.NumBits)))
                    {
                        gcs.Invert();
                        using (var negative = await RenderThenCapture("Negative {0} [{1}/{2}]".FormatWith(gcs.Dir, gcs.Bit, gcs.Sgc.NumBits)))
                        {
                            gcs.Invert();

                            var gray = Binarize(positive, negative);
                            sweeps.Add(gray);
                        }
                    }
                }

                return await Assemble(sweeps, gcs.Sgc);
            }
        }

        private async Task GetBlackWhiteAndMask(Scene scene)
        {
            Util.Dispose(ref Black);
            Util.Dispose(ref White);
            Util.Dispose(ref Colored);
            Util.Dispose(ref Mask);

            var clearBlack = new Clear(Color.Black);
            var clearWhite = new Clear(Color.White);
            var clearLightish = new Clear(Color.WhiteSmoke);

            scene.Clear();
            scene.Add(clearBlack);
            Black = await RenderThenCapture("Black");

            scene.Clear();
            scene.Add(clearWhite);
            White = await RenderThenCapture("White");

            scene.Clear();
            scene.Add(clearLightish);
            Colored = await RenderThenCaptureColored("Colored");

            var maskThreshold = new Gray(75);
            var maskOn = new Gray(255);
            Mask = new Picture<Gray, byte>(White.Width, White.Height);
            (White.Emgu - Black.Emgu).ThresholdBinary(maskThreshold, maskOn).CopyTo(Mask.Emgu);

            Black.Emgu.Save(Global.TmpFileName("black", "png"));
            White.Emgu.Save(Global.TmpFileName("white", "png"));
            Mask.Emgu.Save(Global.TmpFileName("mask", "png"));
            Colored.Emgu.Convert<Bgr, byte>().Save(Global.TmpFileName("colored", "png"));
        }

        private async Task<int[]> Assemble(IList<Picture<Gray, byte>> grays, SafeGrayCode sgc)
        {
            await Program.SwitchToCompute();

            var width = Black.Width;
            var height = Black.Height;
            var accum = new int[height * width];

            using (var asmbl = new Picture<Gray, byte>(width, height))
            using (var clred = new Picture<Bgra, byte>(width, height))
            {
                for (int g = 0; g < sgc.NumBits; ++g)
                {
                    for (int b = 0; b < accum.Length; ++b)
                    {
                        var v = grays[g].Bytes[b] != 0
                              ? (1 << g)
                              : 0;

                        accum[b] = accum[b] | v;
                    }
                }

                for (int b = 0; b < accum.Length; ++b)
                {
                    accum[b] = sgc.ToBinary(accum[b]);
                    asmbl.Bytes[b] = (byte)((accum[b] * 255) / sgc.Count);
                }

                WriteLine("Accumulated Min and Max is {0} {1}", accum.Min(), accum.Max());
                
                asmbl.Emgu.Save(Global.TmpFileName("assembled", "png"));

                return accum;
            }
        }

        private async Task Make(int[] horAccum, int[] verAccum, Scene scene)
        {
            await Program.SwitchToCompute();

            var projectorSize = Projector.Intrinsics.ImageSize;
            var captureCameraSize = CaptureCamera.Intrinsics.ImageSize;

            var horSweptPlanes = Enumerable.Range(0, projectorSize.Width)
                                           .Select(x => Projector.Frustum.BoundingFrustum.SweepVerticalThroughX(Projector.World.Translation, x, projectorSize.Width))
                                           .ToArray();

            var verSweptPlanes = Enumerable.Range(0, projectorSize.Height)
                                           .Select(y => Projector.Frustum.BoundingFrustum.SweepHorizontalThroughY(Projector.World.Translation, y, projectorSize.Height))
                                           .ToArray();

            var zPlane = new Plane(Vector3.UnitZ, 0);
            var vertices = new VertexPositionNormalTexture[captureCameraSize.Area()];
            for (int y = 0; y < captureCameraSize.Height; ++y)
                for (int x = 0; x < captureCameraSize.Width; ++x)
                {
                    var pixel = y * captureCameraSize.Width + x;

                    var ray = CaptureCamera.Frustum.BoundingFrustum.ThroughPixel(CaptureCamera.World.Translation, x, y, captureCameraSize.Width, captureCameraSize.Height);
                        
                    Vector3? horPos = null;
                    Vector3? verPos = null;

                    var hor = horAccum[pixel];
                    if (hor >= 0 && hor < horSweptPlanes.Length)
                    {
                        var swep = horSweptPlanes[hor];
                        var intersect = ray.Intersects(swep);
                        if (intersect.HasValue && intersect.Value > 0.0f)
                        {
                            var position = ray.Position + intersect.Value * ray.Direction;
                            if (position.Z > -0.6f && position.Z < 70.0f)
                            {
                                horPos = position;
                            }
                        }
                    }

                    var ver = verAccum[pixel];
                    if (ver >= 0 && ver < verSweptPlanes.Length)
                    {
                        var swep = verSweptPlanes[ver];                        
                        var intersect = ray.Intersects(swep);
                        if (intersect.HasValue && intersect.Value > 0.0f)
                        {
                            var position = ray.Position + intersect.Value * ray.Direction;
                            if (position.Z > -0.6f && position.Z < 70.0f)
                            {
                                verPos = position;
                            }
                        }
                    }
                    
                    var failed = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                    var fusedPos3 = horPos.HasValue ? horPos.Value : failed;
                    var fusedPos = verPos.HasValue ? verPos.Value : failed;
                    var fusedPos2 = horPos.HasValue
                                 ? (verPos.HasValue
                                    ? (horPos.Value + verPos.Value) / 2.0f
                                    : horPos.Value)
                                 : verPos.HasValue
                                 ? verPos.Value
                                 : failed;

                    // if (fusedPos3 != failed)
                    // if ((ver >= 0 && ver < verSweptPlanes.Length)
                    // || (hor >= 0 && hor < horSweptPlanes.Length))
                    if (verPos.HasValue)
                    {
                        fusedPos = verPos.Value;// new Vector3(x / 100.0f, (captureCameraSize.Height - y) / 100.0f, 0.1f);
                        vertices[pixel] = new VertexPositionNormalTexture(fusedPos, Vector3.UnitZ, new Vector2(x / (float)captureCameraSize.Width, y / (float)captureCameraSize.Height));
                    }
                }

            CalculateNormals(vertices, captureCameraSize.Width, captureCameraSize.Height);

            await Program.SwitchToRender();

            var texture2D = Program.Renderer.LeaseFor(Colored);
            texture2D.SetData(Colored.Bytes);
                
            // Program.Monitor.Scene.Remove(Program.Monitor.Scene.First(v => v is Picture2D));
            // Program.Monitor.Scene.Remove(Program.Monitor.Scene.First(v => v is Clear));
            
            scene.Add(new Clear(Color.DarkGray));
            for (int i = 0; i < horSweptPlanes.Length; i += 230)
                scene.Add(Projector.Frustum.BoundingFrustum.QuadVerticalThroughX(Projector.World.Translation, i, 1280));
            for (int i = 0; i < verSweptPlanes.Length; i += 174)
                scene.Add(Projector.Frustum.BoundingFrustum.QuadHorizontalThroughY(Projector.World.Translation, i, 800));
            
            
            scene.Add(new Cloud(Color.White, CaptureCamera.Intrinsics.ImageSize) { Points = vertices, Texture2D = texture2D });
        }

        public static void CalculateNormals(VertexPositionNormalTexture[] pts, int width, int height)
        {
            var nms = new Vector3[width * height];

            for (int y = 1; y < height; ++y)
                for (int x = 1; x < width; ++x)
                {
                    nms[y * width + x] = Vector3.Normalize(
                                                        Vector3.Cross(pts[(y - 1) * width + x].Position - pts[y * width + x].Position,
                                                                      pts[y * width + (x - 1)].Position) - pts[y * width + x].Position);
                }

            for (int y = 1; y < (height - 1); ++y)
                for (int x = 1; x < (width - 1); ++x)
                {
                    var a = nms[(y - 1) * width + (x - 1)];
                    var b = nms[(y - 0) * width + (x - 1)] * 2;
                    var c = nms[(y + 1) * width + (x - 1)];

                    var d = nms[(y - 1) * width + (x - 0)] * 2;
                    var e = nms[(y - 0) * width + (x - 0)] * 4;
                    var f = nms[(y + 1) * width + (x - 0)] * 2;

                    var g = nms[(y - 1) * width + (x + 1)];
                    var k = nms[(y - 0) * width + (x + 1)] * 2;
                    var i = nms[(y + 1) * width + (x + 1)];

                    var n = Vector3.Normalize((a + b + c + d + e + f + g + k + i) / 16.0f);
                    pts[y * width + x].Normal = n;
                }
        }
        
        private Picture<Gray, byte> Binarize(Picture<Gray, byte> positive, Picture<Gray, byte> negative)
        {
            var binary = new Picture<Gray, byte>(positive.Width, positive.Height);
            positive.Emgu.Cmp(negative.Emgu, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_GE).And(Mask.Emgu).CopyTo(binary.Emgu);
            binary.Emgu.Save(Global.TmpFileName("bin", "png"));
            return binary;
        }

        private async Task<Picture<Gray, byte>> RenderThenCapture(string text)
        {
            var numPreTicks = 2;
            var numFlushTicks = 3;
            var numPostTicks = 2;
            var numPictureFlushes = 3;
            
            for (var i = 0; i < numPreTicks; ++i) await Program.Tick();
            
            for (var i = 0; i < numPictureFlushes; ++i)
                using (var flush = await CaptureCamera.Capture.NextGray()) { } // Do nothing, just flushing any outdated captures...

            for (var i = 0; i < numFlushTicks; ++i) await Program.Tick();

            var picture = await CaptureCamera.Capture.NextGray();
                
            for (var i = 0; i < numPostTicks; ++i) await Program.Tick();

            return picture;
        }

        private async Task<Picture<Rgba, byte>> RenderThenCaptureColored(string text)
        {
            WriteLine(text);

            var numPreTicks = 3;
            var numFlushTicks = 2;
            var numPostTicks = 1;
            var numPictureFlushes = 2;

            for (var i = 0; i < numPreTicks; ++i) await Program.Tick();

            for (var i = 0; i < numPictureFlushes; ++i)
                using (var flush = await CaptureCamera.Capture.NextRgba()) { } // Do nothing, just flushing any outdated captures...

            for (var i = 0; i < numFlushTicks; ++i) await Program.Tick();

            var picture = await CaptureCamera.Capture.NextRgba();

            for (var i = 0; i < numPostTicks; ++i) await Program.Tick();

            return picture;
        }

        private Picture<Rgba, byte> Colored;
        private Picture<Gray, byte> White;
        private Picture<Gray, byte> Black;
        private Picture<Gray, byte> Mask;
        
        private CaptureCamera CaptureCamera;
        private Projector Projector;
    }
}
