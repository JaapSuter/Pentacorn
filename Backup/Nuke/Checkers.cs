using System;
using System.Drawing;
using System.Linq;
using Emgu.CV.Structure;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rect = Microsoft.Xna.Framework.Rectangle;

namespace Pentacorn.Vision
{
    class Checkers
    {
        public Size Squares { get { return new Size(M, N); } }
        public Size Saddles { get { return new Size(M - 1, N - 1); } }
        public Rect Board { get; private set; }
        public Size Square { get; private set; }
        
        public Color WhiteColor { get; set; }
        public Color BlackColor { get; set; }
        public Color OtherColor { get; set; }
        
        public Checkers(int m, int n, Point origin, Size square)
        {
            if ((m & 1) == (n & 1))
                throw new Exception("Checker board must have even/odd M and N or vice versa.");

            WhiteColor = Color.White;
            BlackColor = Color.Black;
            OtherColor = Color.White;

            M = m;
            N = n;
            Square = square;
            Board = new Rect(origin.X, origin.Y, M * square.Width, N * square.Height);            
        }

        public PointF[][] GetImagePointsCopy(int num)
        {
            PointF[][] ops = new PointF[num][];

            var m =  Saddles.Width;
            var n =  Saddles.Height;

            ops[0] = new PointF[m * n];
            for (int y = 0; y < n; ++y)
                for (int x = 0; x < m; ++x)
                    ops[0][y * m + x] = new PointF(Board.Left + (x + 1) * Square.Width, Board.Top + (y + 1) * Square.Height);

            for (int i = 1; i < num; ++i)
                ops[i] = ops[0].ToArray();

            return ops;
        }

        public MCvPoint3D32f[] GetWorldPointsCopy()
        {
            var m = Saddles.Width;
            var n = Saddles.Height;

            var ops = new MCvPoint3D32f[m * n];
            for (int y = 0; y < n; ++y)
                for (int x = 0; x < m; ++x)
                    ops[y * m + x] = new MCvPoint3D32f(Board.Left + (x + 1) * Square.Width, Board.Top + (y + 1) * Square.Height, 0);

            return ops;
        }

        private int M;
        private int N;                
    }
}
