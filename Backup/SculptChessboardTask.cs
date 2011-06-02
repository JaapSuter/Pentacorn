using System;
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

namespace Pentacorn.Tasks
{
    class SculptChessboardTask
    {
        public async Task<Chessboard> Run(Services services)
        {
            var monitor = services.Monitor;
            var projector = services.Projectors.First();

            monitor.FullScreen = false;
            projector.FullScreen = true;

            monitor.Show();
            projector.Show();

            Vector3[] projectorCorners;
            var chessboard = LoadOrDefault(out projectorCorners, projector.ClientSize);

            var shapingChessboard = from idx in projector.MouseDown.Select(e => ClosestCorner(projectorCorners, e))
                                    from pos in projector.MouseMove.TakeUntil(projector.MouseUp)
                                    select new { Idx = idx, Position = pos, };
            
            var done = false;
            var confirmTrigger = from key in projector.KeyDown.Merge(monitor.KeyDown)
                                 where key.KeyCode == Keys.Space
                                 select default(Unit);

            var closeTrigger = from close in monitor.FormClosingObservable
                               select close;

            var doneTrigger = Observable.Amb(confirmTrigger, closeTrigger);

            var homography = chessboard.GetVertexHomographyTo(projectorCorners);
            
            using (shapingChessboard.Subscribe(s =>
            {
                projectorCorners[s.Idx] = new Vector3(s.Position, 0);
                homography = chessboard.GetVertexHomographyTo(projectorCorners);
            }))
            using (doneTrigger.Subscribe(_ => done = true))
            {
                while (!done)
                {
                    var pf = projector.Begin(services.Renderer);
                    
                    var camera = new ScreenCamera2D(homography, 
                        Matrix.CreateOrthographicOffCenter(0, pf.Size.Width, pf.Size.Height, 0, 0, 1));                    
                    pf.Add(Color.Orange);
                    pf.Add(camera);
                    pf.Add(chessboard);
                    await pf.PresentAsync();
                }
            }

            Save(chessboard, projectorCorners);

            return null;
        }
        
        public static Chessboard Load(out Vector3[] fourImageCorners, Size projectorResolution)
        {
            var df = new DictionaryFile(File.ReadAllLines(FourCornersPath));

            fourImageCorners = new[] 
            {
                new Vector3(df.Get<float>("imageCorner0.X"), df.Get<float>("imageCorner0.Y"), 0),
                new Vector3(df.Get<float>("imageCorner1.X"), df.Get<float>("imageCorner1.Y"), 0),
                new Vector3(df.Get<float>("imageCorner2.X"), df.Get<float>("imageCorner2.Y"), 0),
                new Vector3(df.Get<float>("imageCorner3.X"), df.Get<float>("imageCorner3.Y"), 0),
            };

            var tileCount = new Size(df.Get<int>("tileCount.Width"), df.Get<int>("tileCount.Height"));
                
            return new Chessboard(tileCount);
        }

        private static Chessboard LoadOrDefault(out Vector3[] fourImageCorners, Size frameSize)
        {
            if (File.Exists(FourCornersPath))
                return Load(out fourImageCorners, frameSize);

            var tileCount = new Size(4, 5);

            var marginPercentage = 0.10f;
            var margin = marginPercentage * Math.Min(frameSize.Width, frameSize.Height);
            var unMargin = Math.Min(frameSize.Width, frameSize.Height) - 2 * margin;
            var marginHor = (margin / Math.Max(tileCount.Width, tileCount.Height)) * tileCount.Width;
            var marginVer = (margin / Math.Max(tileCount.Width, tileCount.Height)) * tileCount.Height;

            fourImageCorners = new[]
            {
                new Vector3(frameSize.Width - marginHor, frameSize.Height - marginVer, 0),
                new Vector3(marginHor, frameSize.Height - marginVer, 0),
                new Vector3(marginHor, marginVer, 0),
                new Vector3(frameSize.Width - marginHor, marginVer, 0),
            };

            return new Chessboard(tileCount);
        }

        private void Save(Chessboard chessboard, Vector3[] fourImageCorners)
        {
            var df = new DictionaryFile();

            df.Set("tileCount.Width", chessboard.TileCount.Width);
            df.Set("tileCount.Height", chessboard.TileCount.Height);

            df.Set("imageCorner0.X", fourImageCorners[0].X);
            df.Set("imageCorner0.Y", fourImageCorners[0].Y);
            df.Set("imageCorner1.X", fourImageCorners[1].X);
            df.Set("imageCorner1.Y", fourImageCorners[1].Y);
            df.Set("imageCorner2.X", fourImageCorners[2].X);
            df.Set("imageCorner2.Y", fourImageCorners[2].Y);
            df.Set("imageCorner3.X", fourImageCorners[3].X);
            df.Set("imageCorner3.Y", fourImageCorners[3].Y);

            using (var sw = File.CreateText(FourCornersPath))
                df.Save(sw);
        }

        private readonly static string FourCornersPath = Path.Combine(Global.DatDir, "FourCorners.txt");

        public static int ClosestCorner(Vector3[] fourImageCorners, Vector2 to)
        {
            var ordered = from idx in Enumerable.Range(0, fourImageCorners.Length)
                          let dist = Vector2.Distance(fourImageCorners[idx].ToVector2(), to)
                          orderby dist 
                          select idx;

            return ordered.First();
        }
    }
}
