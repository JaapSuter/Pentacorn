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
using System.IO;

namespace Pentacorn.Tasks
{
    class ChessboardShapeTask : WorkFlowTask
    {
        public ChessboardShapeTask(Window window, Projector projector, Chessboard chessboard)
            : base(window)
        {
            Projector = projector;
            Chessboard = chessboard;
            ColoredChessboard = new ColoredChessboard(Chessboard, Color.Black, Color.White);
            LoadOrDefault(projector.Window.ClientSize);
        }

        public async Task Run()
        {            
            WriteLine("Shape Projector Chessboard By Dragging Corners");

            var shapingEnd = Program.WhenInput.Where(i => i.LeftMouseUp);
            var shapingBegin = Program.WhenInput.Where(i => i.LeftMouseDown && Projector.Window.Focused);

            var closeEnoughForCornerShape = float.MaxValue;
            var closeEnoughForSnap = 37;
            var shaping = from begin in shapingBegin
                          let fro = begin.MousePosition
                          let tup = DistanceToClosestCorner(ProjectorQuadCorners, fro)
                          where tup.Item1 <= closeEnoughForCornerShape
                          let idx = tup.Item2
                          from input in Program.WhenInput.TakeUntil(shapingEnd)
                          let to = input.MousePosition
                          let minPixelDist = 3
                          where Vector2.Distance(fro, to) > minPixelDist
                          select new { Idx = idx, Position = input.MousePosition, Snap = input.KeyPressed(Keys.LeftShift), };

            using (shaping.Subscribe(s =>
            {
                ProjectorQuadCorners[s.Idx] = s.Position.ToPointF();

                if (s.Snap)
                    Snap(ProjectorQuadCorners, s.Idx, snapWithin: closeEnoughForSnap);

                ColoredChessboard.WhiteColor = Color.Orange;
                ColoredChessboard.BlackColor = Color.CornflowerBlue;

                WorldToProjectorHom = Chessboard.GetHomographyTo(ProjectorQuadCorners);
                ColoredChessboard.Homography = WorldToProjectorHom.ToMatrixFromHomogeneous3x3();

                LocateProjector();
            }))
            using (shapingEnd.Subscribe(e =>
            {
                ColoredChessboard.WhiteColor = Color.White;
                ColoredChessboard.BlackColor = Color.Black;
            }))
            using (Disposable.Create(() => Projector.Scene.Remove(ColoredChessboard)))            
            {
                Projector.Scene.Add(ColoredChessboard);

                await Program.WhenInput.Where(input => input.KeyDown(Keys.Enter)).TakeNext();
            }

            Save(Chessboard, ProjectorQuadCorners);
            LocateProjector();
        }

        private void LocateProjector()
        {
            var imagePoints = Chessboard.Saddles.ToArray();
            WorldToProjectorHom.ProjectPoints(imagePoints);
            Projector.Locate(Chessboard.Saddles, imagePoints);
        }

        private static void Snap(PointF[] projectorQuadCorners, int idx, float snapWithin)
        {
            int x = 0;
            int y = 0;
            if (idx == 0) { x = 2; y = 1; }
            if (idx == 1) { x = 3; y = 0; }
            if (idx == 2) { x = 0; y = 3; }
            if (idx == 3) { x = 1; y = 2; }

            var sx = projectorQuadCorners[x].X;
            var sy = projectorQuadCorners[y].Y;

            if (Math.Abs(sx - projectorQuadCorners[idx].X) < snapWithin)
                projectorQuadCorners[idx].X = sx;
            if (Math.Abs(sy - projectorQuadCorners[idx].Y) < snapWithin)
                projectorQuadCorners[idx].Y = sy;
        }

        private Size Load()
        {
            var df = new DictionaryFile(File.ReadAllLines(FourCornersPath));

            ProjectorQuadCorners = new[] 
            {
                new PointF(df.Get<float>("corner0.X"), df.Get<float>("corner0.Y")),
                new PointF(df.Get<float>("corner1.X"), df.Get<float>("corner1.Y")),
                new PointF(df.Get<float>("corner2.X"), df.Get<float>("corner2.Y")),
                new PointF(df.Get<float>("corner3.X"), df.Get<float>("corner3.Y")),
            };

            return new Size(df.Get<int>("tileCount.Width"), df.Get<int>("tileCount.Height"));
        }

        private void LoadOrDefault(Size imageSize)
        {
            var tileCountFromLoad = File.Exists(FourCornersPath)
                                  ? Load()
                                  : Size.Empty;

            if (Chessboard.TileCount != tileCountFromLoad)
                ProjectorQuadCorners = Chessboard.GetDefaultImageQuadCorners(imageSize);

            WorldToProjectorHom = Chessboard.GetHomographyTo(ProjectorQuadCorners);
            ColoredChessboard.Homography = WorldToProjectorHom.ToMatrixFromHomogeneous3x3();
        }

        private static void Save(Chessboard chessboard, PointF[] projectorQuadCorners)
        {
            var df = new DictionaryFile();

            df.Set("tileCount.Width", chessboard.TileCount.Width);
            df.Set("tileCount.Height", chessboard.TileCount.Height);

            df.Set("corner0.X", projectorQuadCorners[0].X);
            df.Set("corner0.Y", projectorQuadCorners[0].Y);
            df.Set("corner1.X", projectorQuadCorners[1].X);
            df.Set("corner1.Y", projectorQuadCorners[1].Y);
            df.Set("corner2.X", projectorQuadCorners[2].X);
            df.Set("corner2.Y", projectorQuadCorners[2].Y);
            df.Set("corner3.X", projectorQuadCorners[3].X);
            df.Set("corner3.Y", projectorQuadCorners[3].Y);

            using (var sw = File.CreateText(FourCornersPath))
                df.Save(sw);
        }

        private static Tuple<float, int> DistanceToClosestCorner(PointF[] projectorQuadCorners, Vector2 to)
        {
            var ordered = from idx in Enumerable.Range(0, projectorQuadCorners.Length)
                          let dist = Vector2.Distance(projectorQuadCorners[idx].ToVector2(), to)
                          orderby dist
                          select Tuple.Create(dist, idx);

            return ordered.First();
        }

        private readonly static string FourCornersPath = Path.Combine(Global.DatDir, "FourCorners.txt");
        private HomographyMatrix WorldToProjectorHom;
        private Chessboard Chessboard;
        private ColoredChessboard ColoredChessboard;
        private PointF[] ProjectorQuadCorners;
        private Projector Projector;
    }
}
