using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Graphics
{
    class Picture2D : IVisible
    {
        public Rectangle Rectangle { get; private set; }
        public Texture2D Texture2D { get; set; }
        public Color Color { get; set; }

        public Picture2D(Rectangle rect, Texture2D tex)
            : this(rect, tex, Color.White) { }

        public Picture2D(Texture2D tex)
            : this(new Rectangle(0, 0, tex.Width, tex.Height), tex, Color.White) { }

        public Picture2D(System.Drawing.Rectangle rect, Texture2D tex)
            : this(new Rectangle(rect.X, rect.Y, rect.Width, rect.Height), tex, Color.White) { }

        public Picture2D(Rectangle rect, Texture2D tex, Color color)
        {
            Rectangle = rect;
            Texture2D = tex;
            Color = color;
        }

        public void Render(Renderer renderer, IViewProject viewProject)
        {
            throw new System.NotImplementedException();
        }
    }
}
