using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Pentacorn.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using PointF = System.Drawing.PointF;
using Size = System.Drawing.Size;

namespace Pentacorn.Tasks
{
    class VirtualProjector : Projector, IVisible
    {
        public VirtualProjector(string name, string uuid, Size imageSize, PointF principal, double focalLength, Screen screen, Chessboard chessboard, CaptureCamera captureCamera)
            : base(name, uuid, imageSize, principal, focalLength, screen, chessboard, captureCamera)
        {
            OrbitCamera = new OrbitCamera("{0} Orbit".FormatWith(name), "N/A", imageSize, principal, focalLength,
                Intrinsics.NearPlaneDistance, Intrinsics.FarPlaneDistance) { Color = Color.DarkCyan.Alpha(0.7f), };
            Color = Color.DarkKhaki.Alpha(0.6f);
            
            ProgramTask.AttachInputToCamera(Program.WhenInput.Where(input => !input.KeyPressed(Keys.C)), Window, OrbitCamera);

            Program.WhenInput.Where(input => input.KeyDown(Keys.P)).Subscribe(input =>
            {
                var frustum = new Frustum(OrbitCamera, Intrinsics.ImageSize);

                var plane = new Plane(Vector3.UnitZ, 0);

                var p0 = frustum.IntersectWithPlane(ProjectorQuadCorners[0].X, ProjectorQuadCorners[0].Y, plane).ToPointF();
                var p1 = frustum.IntersectWithPlane(ProjectorQuadCorners[1].X, ProjectorQuadCorners[1].Y, plane).ToPointF();
                var p2 = frustum.IntersectWithPlane(ProjectorQuadCorners[2].X, ProjectorQuadCorners[2].Y, plane).ToPointF();
                var p3 = frustum.IntersectWithPlane(ProjectorQuadCorners[3].X, ProjectorQuadCorners[3].Y, plane).ToPointF();

                var quadCorners = new[] { p0, p1, p2, p3, };

                // Chessboard.HomographTo(quadCorners);
            });
        }
        
        public override void Render(Renderer renderer, IViewProject viewProject)
        {
            OrbitCamera.Render(renderer, viewProject);
            base.Render(renderer, viewProject);
        }

        public OrbitCamera OrbitCamera;
    }
}
