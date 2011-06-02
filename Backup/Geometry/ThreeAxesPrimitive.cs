using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pentacorn.Vision.Graphics;

namespace Pentacorn.Vision.Geometry
{
    class ThreeAxesPrimitive : IDisposable
    {
        public ThreeAxesPrimitive(GraphicsDevice graphicsDevice, float length = 1.0f, float thickness = 0.03f, int tesselation = 8)
        {
            this.length = length;
            x = new CylinderPrimitive(graphicsDevice, length, thickness, tesselation);
            y = new CylinderPrimitive(graphicsDevice, length, thickness, tesselation);
            z = new CylinderPrimitive(graphicsDevice, length, thickness, tesselation);
            center = new CubePrimitive(graphicsDevice, 2 * thickness);

            x.Color = Color.Red;
            y.Color = Color.Green;
            z.Color = Color.Blue;
            center.Color = Color.White;
        }

        public override void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {
            // Cylinder primitive goes up the Y-axis.
            Matrix moveUpY = Matrix.CreateTranslation(0, length / 2, 0);
            Matrix rotateX = Matrix.CreateRotationZ(-MathHelper.PiOver2);
            Matrix rotateZ = Matrix.CreateRotationX(MathHelper.PiOver2);
            x.World = moveUpY * rotateX * this.World;
            y.World = moveUpY * this.World;
            z.World = moveUpY * rotateZ * this.World;
            center.World = this.World;

            x.Draw(gameTime, view, projection);
            y.Draw(gameTime, view, projection);
            z.Draw(gameTime, view, projection);
            center.Draw(gameTime, view, projection);
        }

        ~ThreeAxesPrimitive() { Dispose(false); }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (x != null)
                    x.Dispose();
                if (y != null)
                    y.Dispose();
                if (z != null)
                    z.Dispose();
            }
        }

        private float length;
        private CylinderPrimitive x, y, z;
        private CubePrimitive center;
    }
}
