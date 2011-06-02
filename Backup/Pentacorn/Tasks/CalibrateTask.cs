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
    class CalibrateTask : WorkFlowTask
    {
        public class ImageAndWorldPoints
        {
            public PointF[] ImagePoints;
            public PointF[] WorldPoints;
        };

        public CalibrateTask(Window window, Camera camera, Chessboard chessboard)
            : base(window)
        {
            Camera = camera;
            Chessboard = chessboard;
        }

        public Task Run(CaptureCamera captureCamera)
        {
            return Run(() => GetPoints(captureCamera));
        }

        public async Task Run(Func<Task<ImageAndWorldPoints>> getPoints)
        {
            var imagePointsMinNecessary = 5;
            var imagePointsList = new List<PointF[]>(imagePointsMinNecessary * imagePointsMinNecessary);
            var worldPointsList = new List<PointF[]>(imagePointsMinNecessary * imagePointsMinNecessary);

            var whenTrigger = Program.WhenInput
                                     .Where(input => input.KeyDown(Keys.Space) || input.KeyDown(Keys.Enter) || input.KeyDown(Keys.Escape));

            for (; ; )
            {
                if (imagePointsList.Count < imagePointsMinNecessary)
                    WriteLine("Take {0} more chessboard pictures.", imagePointsMinNecessary - imagePointsList.Count);
                else
                {
                    WriteLine("Have {0} snapshots", imagePointsList.Count);
                    WriteLine("Press Enter to Calibrate");
                    WriteLine("Or Space to Take more Snapshots.");
                }

                var trigger = await whenTrigger.TakeNext();

                if (trigger.KeyDown(Keys.Escape))
                    return;

                if (imagePointsList.Count >= imagePointsMinNecessary)
                    if (trigger.KeyDown(Keys.Enter))
                    {
                        WriteLine("Calibrating Using {0} Images.", imagePointsList.Count);

                        await Camera.CalibrateAsync(worldPointsList, imagePointsList);

                        WriteLine("Calibration Complete");
                        WriteLine("Reprojection Error: {0:0.000}", Camera.Intrinsics.ReprojectionError);
                        WriteLine("Save? (Y)es/(N)o");

                        var save = await Program.WhenInput.Where(input => input.KeyDown(Keys.Y) || input.KeyDown(Keys.Escape) || input.KeyDown(Keys.N)).TakeNext();
                        if (save.KeyDown(Keys.Y))
                        {
                            Camera.Save();
                            WriteLine("Calibration Saved");
                        }

                        return;
                    }                

                if (trigger.KeyDown(Keys.Space))
                {
                    var imageAndWorldPoints = await getPoints();

                    if (imageAndWorldPoints != null)
                    {
                        WriteLine("Chessboard Found and Added");

                        Camera.Locate(imageAndWorldPoints.WorldPoints, imageAndWorldPoints.ImagePoints);

                        imagePointsList.Add(imageAndWorldPoints.ImagePoints);
                        worldPointsList.Add(imageAndWorldPoints.WorldPoints);
                    }
                    else
                        WriteLine("Could Not Find Chessboard");
                }
            }
        }

        private async Task<ImageAndWorldPoints> GetPoints(CaptureCamera captureCamera)
        {
            var imagePoints = await new ChessboardSearchTask(this, captureCamera, Chessboard).Run();
            if (imagePoints != null)
            {
                return new ImageAndWorldPoints()
                {
                    WorldPoints = Chessboard.Saddles,
                    ImagePoints = imagePoints,
                };
            }
            else WriteLine("Could Not Find Chessboard");

            return null;

        }

        private Camera Camera;
        private Chessboard Chessboard;
    }
}
