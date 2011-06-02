using Microsoft.Xna.Framework;

namespace Pentacorn.Graphics
{
    class Grid : IVisible
    {
        public Grid(Matrix world, Color fillColor, Color lineColor, float lineEvery = 1.0f, float lineThickness = 0.1f)
        {
            World = world;
            FillColor = fillColor;
            LineColor = lineColor;
            LineEvery = lineEvery;
            LineThickness = lineThickness;
        }

        public void Render(Renderer renderer, IViewProject viewProject)
        {
            throw new System.NotImplementedException();
        }


        public Matrix World;
        public Color FillColor;
        public Color LineColor;
        public float LineEvery;
        public float LineThickness;
    }
}
