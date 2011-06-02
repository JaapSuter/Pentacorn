using System.Drawing;
using System.Linq;
using Emgu.CV;
using Microsoft.Xna.Framework;
using Pentacorn.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using System;

namespace Pentacorn
{
    class Chessboard : IVisible
    {
        public Size TileCount { get; private set; }
        public Size SaddleCount { get; private set; }
        public Size VertexCount { get; private set; }

        public Vector3[] Vertices { get; private set; }
        public PointF[] Saddles { get; private set; }

        public Color BlackColor { get; set; }
        public Color WhiteColor { get; set; }

        public Vector3[] VertexQuadCorners { get { return Vertices.TakeQuadCorners(VertexCount); } }
        public PointF[] SaddleQuadCorners { get { return Saddles.TakeQuadCorners(SaddleCount); } }
        
        public Vector3 VertexCenter { get { return (Vertices.First() + Vertices.Last()) / 2; } }

        public Matrix Homography { get; set; }

        public PointF[] GetDefaultImageQuadCorners(Size imageSize)
        {
            var tileSize = (int)Math.Min(imageSize.Width / (TileCount.Width + 1),
                                         imageSize.Height / (TileCount.Height + 1));
            var boardSize = new Size(TileCount.Width * tileSize, TileCount.Height * tileSize);
            var boardRect = boardSize.AnchorWithin(imageSize.ToRect(), AnchorPoints.Center);

            return new[]
                { 
                    new PointF(boardRect.Left, boardRect.Top),
                    new PointF(boardRect.Right, boardRect.Top),
                    new PointF(boardRect.Left, boardRect.Bottom),
                    new PointF(boardRect.Right, boardRect.Bottom),
                };
        }
        
        public HomographyMatrix GetHomographyTo(PointF[] quadCorners)
        {
            return CameraCalibration.GetPerspectiveTransform(VertexQuadCorners.ToArrayOf(v => v.ToPointF()), quadCorners);
        }

        public HomographyMatrix GetHomographyFrom(PointF[] quadCorners)
        {
            return CameraCalibration.GetPerspectiveTransform(quadCorners, VertexQuadCorners.ToArrayOf(v => v.ToPointF()));
        }

        public Chessboard(int M, int N)
        {
            TileCount = new Size(M, N);
            SaddleCount = new Size(M - 1, N - 1);
            VertexCount = new Size(M + 1, N + 1);

            BlackColor = Color.Black;
            WhiteColor = Color.White;

            Vertices = new Vector3[VertexCount.Area()];
            Saddles = new PointF[SaddleCount.Area()];

            Homography = Matrix.Identity;

            var vdx = 0;
            var eps = 0;
            for (int n = 0; n < VertexCount.Height; ++n)
                for (int m = 0; m < VertexCount.Width; ++m)
                    Vertices[vdx++] = new Vector3(m - 1, VertexCount.Height - 2 - n, eps);


            var sdx = 0;
            for (int n = 0; n < SaddleCount.Height; ++n)
                for (int m = 0; m < SaddleCount.Width; ++m)
                    Saddles[sdx++] = new PointF(m, SaddleCount.Height - 1 - n);
        }

        public void Render(Renderer renderer, IViewProject viewProject)
        {
            throw new System.NotImplementedException();
        }

        public static int ActiveSearches = 0;
    }
}
