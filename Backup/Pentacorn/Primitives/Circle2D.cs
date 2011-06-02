using Microsoft.Xna.Framework;

namespace Pentacorn.Graphics
{
    class Circle2D : IVisible
    {
        public Circle2D(float x, float y, float z, float innerRadius, float thickness, Color color)
            : this(new Vector3(x, y, z), innerRadius, thickness, color)
        {}

        public Circle2D(Vector3 position, float innerRadius, float thickness, Color color)
        {
            Position = position;
            InnerRadius = innerRadius;
            Thickness = thickness;
            Color = color;            
        }

        public void Render(Renderer renderer, IViewProject viewProject)
        {
            throw new System.NotImplementedException();
        }


        public float InnerRadius;
        public float Thickness;
        public Vector3 Position;
        public Color Color;
        public bool AlreadyInScreenSpace = false;
    }
}
