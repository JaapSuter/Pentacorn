using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;
using Pentacorn.Graphics;
using Capture = Pentacorn.Captures.Capture;
using PointF = System.Drawing.PointF;
using Size = System.Drawing.Size;
using Color = Microsoft.Xna.Framework.Color;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Pentacorn;

namespace Pentacorn.Tasks
{
    class LocationTask
    {
        public async Task<PinHole> Run(Services services, IEnumerable<Intrinsics> intrinsics)
        {
            var monitor = services.Monitor;
            var projector = services.Projectors.First();

            monitor.FullScreen = false;
            projector.FullScreen = true;

            monitor.Show();
            projector.Show();
            
            Vector3[] projectorCorners;
            var chessboard = SculptChessboardTask.Load(out projectorCorners, projector.ClientSize);                    

            var confirmTrigger = from key in projector.KeyDown
                                 where key.KeyCode == Keys.Space
                                 select default(Unit);

            var closeTrigger = from close in monitor.FormClosingObservable
                               select close;

            bool done = false;
            var doneTrigger = Observable.Amb(confirmTrigger, closeTrigger);

            var pinHoles = services.Captures.Select((capture, idx) => new PinHole(capture, intrinsics.ToArray()[idx], services.Renderer)).ToArray();

            var capture1 = services.Captures.First();
            var texture2D = services.Renderer.LeaseFor(capture1);
            
            PointF[] corners = new PointF[chessboard.SaddleCount.Area()];                
            
            using (doneTrigger.Subscribe(_ => done = true))
            using (capture1.Subscribe(picture => {
                using (picture)
                {
                    CvInvoke.cvDrawChessboardCorners(picture.Bgra.Ptr, chessboard.SaddleCount, corners, corners.Length, 1);
                    texture2D.SetData(picture.Bytes);
                }
            }))
            {
                var homography = chessboard.GetVertexHomographyTo(projectorCorners);
                var camera = new ScreenCamera2D(homography,
                        Matrix.CreateOrthographicOffCenter(0, projector.ClientSize.Width, projector.ClientSize.Height, 0, 0, 1));

                var orbitCamera = monitor.CreateOrbitCamera(50, monitor.Size.Ratio(), null);
                var picture2D = new Picture2D(services.Captures.First().Size.Scale(0.5f).AnchorWithin(monitor.ClientRectangle, AnchorPoints.BottomRight), texture2D) { Color = Color.White.Scale(0.9f), };
                
                foreach (var pinHole in pinHoles)
                    pinHole.LocateUsing(chessboard, corners);

                while (!done)
                {
                    var pf = projector.Begin(services.Renderer);
                    pf.Add(Color.White);
                    pf.Add(camera);
                    pf.Add(chessboard);
                    await pf.PresentAsync();

                    var mf = monitor.Begin(services.Renderer);
                    mf.Add(Color.DarkGray);
                    mf.Add(chessboard);
                    
                    foreach (var pinHole in pinHoles)
                    {
                        var crns = pinHole.BoundingFrustum.GetCorners().Skip(4).Take(4).ToArray();
                        mf.Add(new Quad(crns[2], crns[3], crns[1], crns[0], picture2D.Texture2D, Color.White.Scale(0.8f)));
                        mf.Add(pinHole.BoundingFrustum);

                        if (corners != null)
                        {
                            var position = pinHole.World.Translation;
                            mf.Add(new LineSegment(position, Vector3.Zero, 0.03f) { Color = Color.Fuchsia, });
                                
                            for (int i = 0; i < corners.Length; ++i)                                
                            {
                                var corner = corners[i];
                                var ray = pinHole.BoundingFrustum.ThroughPixel(position, corner.X, corner.Y, pinHole.ImageWidth, pinHole.ImageHeight);
                                var lineSegment = new LineSegment(ray.Position, ray.Position + 5.4f * ray.Direction, 0.005f) { Color = Palette.Get(i), };
                                mf.Add(lineSegment);
                            }
                        }
                    }

                    await mf.PresentAsync();
                }
            }

            return null;
        }
    }
}
