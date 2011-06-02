using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;
using Pentacorn.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using PointF = System.Drawing.PointF;
using Size = System.Drawing.Size;
using Emgu.CV;

namespace Pentacorn.Tasks
{
    class VirtualProjector : Projector, IVisible
    {
        public VirtualProjector(Window window, OrbitCamera orbitCamera)
            : base("Null", "N/A", orbitCamera.Intrinsics.ImageSize, orbitCamera.Intrinsics.PrincipalPoint, orbitCamera.Intrinsics.FocalLength)
        {
            Color = Color.Yellow;
            Scene = new Scene(ViewProject.Identity);
            Window = window;

            View = orbitCamera.View;
            World = orbitCamera.World;

            OrbitCamera = orbitCamera;
        }

        public override void Render(Renderer renderer, IViewProject viewProject)
        {
            View = OrbitCamera.View;
            World = OrbitCamera.World;
            base.Render(renderer, viewProject);
        }
        
        public override Scene Scene { get; set; }
        public override Window Window { get; set; }

        private OrbitCamera OrbitCamera;
    }
}
