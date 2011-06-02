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

namespace Pentacorn.Tasks
{
    class ChessboardSearchTask : WorkFlowTask
    {
        public ChessboardSearchTask(WorkFlowTask parent, CaptureCamera captureCamera, Chessboard chessboard)
            : base(parent)
        {
            CaptureCamera = captureCamera;
            Chessboard = chessboard;
            WriteLine("Taking Snapshot");
        }

        public ChessboardSearchTask(Window window, CaptureCamera captureCamera, Chessboard chessboard)
            : base(window)
        {
            CaptureCamera = captureCamera;
            Chessboard = chessboard;
            WriteLine("Taking Snapshot");
        }

        public async Task<PointF[]> Run()
        {
            using (var picture = await CaptureCamera.Capture.NextGray())
            {
                WriteLine("Searching For Chessboard");
                var imagePoints = await picture.FindChessboardCornersAsync(Chessboard.SaddleCount);

                if (imagePoints == null)
                    WriteLine("Could Not Find Chessboard");
                else
                    WriteLine("Chessboard Found");

                return imagePoints;
            }
        }

        private CaptureCamera CaptureCamera;
        private Chessboard Chessboard;
    }
}
