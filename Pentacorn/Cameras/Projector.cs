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
    abstract class Projector : Camera, IVisible
    {
        public Projector(string name, string uuid, Size imageSize, PointF principal, double focalLength)
            : base(name, uuid, imageSize, principal, focalLength, DefaultNearPlaneDistance, DefaultFarPlaneDistance, 0)
        {
            Color = Color.DarkBlue;            
        }

        public abstract Window Window { get; set; }
        public abstract Scene Scene { get; set; }
    }
}
