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
    class CalibrateIndirectlyTask : WorkFlowTask
    {
        public CalibrateIndirectlyTask(Window window, IEnumerable<CaptureCamera> captureCameras, Projector projector, Chessboard chessboard)
            : base(window)
        {
            CaptureCameras = captureCameras;
            Chessboard = chessboard;
            Projector = projector;

            ProjectorQuadCorners = Chessboard.GetDefaultImageQuadCorners(projector.Window.ClientSize);
            WorldToProjectorHom = Chessboard.GetHomographyTo(ProjectorQuadCorners);
            ColoredChessboard = new ColoredChessboard(Chessboard, Color.Black, Color.White, WorldToProjectorHom.ToMatrixFromHomogeneous3x3());

            Circles = Chessboard.Saddles.Select(s => new Circle2D(s.ToVector3(z: 0.01f), 10, 4, Color.Crimson) { AlreadyInScreenSpace = false } ).ToList();
        }

        public async Task Run(Scene worldScene)
        {
            using (Disposable.Create(() => worldScene.Remove(ColoredChessboard)))
            using (Disposable.Create(() => Circles.Run(circle => worldScene.Remove(circle))))
            {
                Projector.Scene.Add(ColoredChessboard);
                worldScene.Add(Circles);

                var imagePoints = Chessboard.Saddles;
                WorldToProjectorHom.ProjectPoints(imagePoints);

                await new CalibrateTask(Window, Projector, Chessboard).Run(async () => 
                {
                    var captureCameraPoints = await new ChessboardSearchTask(Window, CaptureCameras.First(), Chessboard).Run();
                    if (captureCameraPoints == null)
                        return null;

                    var worldPoints = CaptureCameras.First().ToWorld(captureCameraPoints);

                    for (var i = 0; i < worldPoints.Length; ++i)
                        Circles[i].Position = worldPoints[i].ToVector3(z: 0.01f);

                    return new CalibrateTask.ImageAndWorldPoints()
                    {
                        ImagePoints = imagePoints,
                        WorldPoints = worldPoints,
                    };
                });

                var whenTrigger = Program.WhenInput
                                         .Where(input => input.KeyDown(Keys.Space) || input.KeyDown(Keys.Enter) || input.KeyDown(Keys.Escape));
            }
        }

        private Projector Projector;
        private IEnumerable<CaptureCamera> CaptureCameras;
        private Chessboard Chessboard;

        private HomographyMatrix WorldToProjectorHom;
        private ColoredChessboard ColoredChessboard;
        private PointF[] ProjectorQuadCorners;
        
        private List<Circle2D> Circles;
    }
}
