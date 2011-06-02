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
    class RealProjector : Projector
    {
        public override Window Window { get; set; }

        public RealProjector(string name, string uuid, Size imageSize, PointF principal, double focalLength, Screen screen)
            : base(name, uuid, imageSize, principal, focalLength)
        {
            Window = new Window("Pentacorn Projector")
            {
                LocatedOnScreen = screen,
                FullScreen = true,
            };

            Color = Color.DarkBlue;
            Window.Scene = new Scene(new ScreenCamera(Window)) { new Clear(Color.White), };
        }

        public RealProjector(string name, string uuid, Size imageSize, Screen screen)
            : this(name, uuid, imageSize, new PointF(imageSize.Width * 0.5f, imageSize.Height * 0.8f), GetDefaultFocalLength(imageSize), screen)
        {}

        public override Scene Scene { get { return Window.Scene; } set { Window.Scene = value; } }
    }
}
