using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Pentacorn.Vision.Graphics.Primitives
{
    class Clear : IRenderable
    {
        public Clear(GraphicsDevice device)
        {
            Device = device;
        }

        public void Render(object obj, Matrix view, Matrix projection)
        {
            Color c = (Color)obj;
            Device.Clear(c);
        }

        private GraphicsDevice Device;
    }
}
