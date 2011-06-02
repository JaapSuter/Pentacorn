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
    class LocateTask : WorkFlowTask
    {
        public LocateTask(WorkFlowTask parent, CaptureCamera captureCamera, Chessboard chessboard)
            : base(parent)
        {
            CaptureCamera = captureCamera;
            Chessboard = chessboard;
            WriteLine("Taking Snapshot");
        }

        public LocateTask(Window window, CaptureCamera captureCamera, Chessboard chessboard)
            : base(window)
        {            
            CaptureCamera = captureCamera;
            Chessboard = chessboard;
            WriteLine("Taking Snapshot");
        }

        public async Task Run()
        {
            var task = new ChessboardSearchTask(this, CaptureCamera, Chessboard);
            var imagePoints = await task.Run();

            if (imagePoints == null)
            {
                WriteLine("Unable To Locate Camera");

            }
            else
            {
                await Program.SwitchToCompute();
                CaptureCamera.Locate(Chessboard.Saddles, imagePoints);
                WriteLine("Camera Located Ok");
            }
        }

        private CaptureCamera CaptureCamera;
        private Chessboard Chessboard;
    }
}
