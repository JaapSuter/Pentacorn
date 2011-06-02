using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Pentacorn.Vision.Graphics
{
    class Grid
    {
        public Grid(GraphicsDevice graphicsDevice, ContentManager content)
        {
            grid = content.Load<Model>("Content/Models/Grid");
            this.World = Matrix.CreateScale(0.1f);
        }

        public override void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {
            grid.Draw(this.World, view, projection);
        }

        private Model grid;
    }
}
