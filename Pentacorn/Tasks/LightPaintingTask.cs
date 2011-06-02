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
    class LightPaintingTask : WorkFlowTask
    {
        public LightPaintingTask(Window window, Projector projector, CaptureCamera captureCamera, Chessboard chessboard)
            : base(window)
        {
            Projector = projector;
            Chessboard = chessboard;
            CaptureCamera = captureCamera;
        }

        public async Task Run()
        {
            await new ChessboardShapeTask(Window, Projector, Chessboard).Run();

            var stop = false;
            using (Program.WhenInput.Where(input => input.KeyDown(Keys.Escape)).Take(1).Subscribe(input => stop = true))
            while (!stop)
            {
                var gray = await CaptureCamera.Capture.NextGray();
                var corners = await gray.FindChessboardCornersAsync(Chessboard.SaddleCount, true);
                
            }
        }

        private Chessboard Chessboard;
        private CaptureCamera CaptureCamera;
        private Projector Projector;
    }
}
