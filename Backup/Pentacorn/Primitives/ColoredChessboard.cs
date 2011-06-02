using System.Disposables;
using Pentacorn.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Microsoft.Xna.Framework;

namespace Pentacorn
{
    class ColoredChessboard : IVisible
    {
        public Chessboard Chessboard { get; private set; }

        public Color BlackColor { get; set; }
        public Color WhiteColor { get; set; }
        public Matrix Homography { get; set; }

        public ColoredChessboard(Chessboard chessboard, Color blackColor, Color whiteColor)
        {
            Chessboard = chessboard;
            BlackColor = blackColor;
            WhiteColor = whiteColor;
            Homography = Matrix.Identity;
        }

        public ColoredChessboard(Chessboard chessboard, Color blackColor, Color whiteColor, Matrix homography)
        {
            Chessboard = chessboard;
            BlackColor = blackColor;
            WhiteColor = whiteColor;
            Homography = homography;
        }

        public void Render(Renderer renderer, IViewProject viewProject)
        {
            var blackRestore = Chessboard.BlackColor;
            var whiteRestore = Chessboard.WhiteColor;
            var homoRestore = Chessboard.Homography;
            using (Disposable.Create(() =>
            { 
                Chessboard.BlackColor = blackRestore;
                Chessboard.WhiteColor = whiteRestore;
                Chessboard.Homography = homoRestore;
            }))
            {
                Chessboard.BlackColor = BlackColor;
                Chessboard.WhiteColor = WhiteColor;
                Chessboard.Homography = Homography;
                renderer.Render(viewProject, Chessboard);
            }
        }

    }
}
