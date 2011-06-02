using Microsoft.Xna.Framework;

namespace Pentacorn.Graphics
{
    class Clear : IVisible
    {
        public Clear(Color color)
        {
            Color = color;
        }

        public void Render(Renderer renderer, IViewProject viewProject)
        {
            throw new System.NotImplementedException();
        }


        public Color Color;
    }
}
