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
    class NullProjector : Projector, IVisible
    {
        public NullProjector(Window window)
            : base("Null", "N/A", new Size(256, 256), new PointF(128, 128), 128)
        {
            Color = Color.Pink;
            Scene = new Scene(ViewProject.Identity);
            Window = window;
        }

        public override Scene Scene { get; set; }
        public override Window Window { get; set; }
    }
}
