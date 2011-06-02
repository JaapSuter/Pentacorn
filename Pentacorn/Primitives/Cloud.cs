using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Size = System.Drawing.Size;

namespace Pentacorn.Graphics
{
    class Cloud : IVisible
    {
        public Cloud(Color color, Size size)
        {
            Size = size;
            Color = color;            
        }

        public void Render(Renderer renderer, IViewProject viewProject)
        {
            throw new System.NotImplementedException();
        }

        public Size Size;
        public VertexPositionNormalTexture[] Points;
        public Texture2D Texture2D;
        public Color Color;
    }
}
