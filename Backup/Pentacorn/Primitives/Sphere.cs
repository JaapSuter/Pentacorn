using Microsoft.Xna.Framework;

namespace Pentacorn.Graphics
{
    class Sphere : IVisible
    {
        public Sphere(Vector3 position, float radius, Color color)
        {
            Position = position;
            Radius = radius;
            Color = color;            
        }

        public void Render(Renderer renderer, IViewProject viewProject)
        {
            throw new System.NotImplementedException();
        }

        public float Radius;
        public Vector3 Position;
        public Color Color;
    }
}
