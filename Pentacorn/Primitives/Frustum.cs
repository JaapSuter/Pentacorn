using Microsoft.Xna.Framework;
using Size = System.Drawing.Size;

namespace Pentacorn.Graphics
{
    class Frustum : IVisible
    {
        public BoundingFrustum BoundingFrustum { get; set; }
        public Matrix World { get; private set; }
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Color Color { get; set; }
        
        public Frustum(Size size, IViewProject viewProject)
        {
            Width = size.Width;
            Height = size.Height;
            Update(viewProject);
        }

        public void Update(IViewProject viewProject)
        {
            View = viewProject.View;
            Projection = viewProject.Projection;
            
            BoundingFrustum = new BoundingFrustum(View * Projection);
            World = Matrix.Invert(View);
        }

        public void Render(Renderer renderer, IViewProject viewProject)
        {
            throw new System.NotImplementedException();
        }

        public Vector3 IntersectWithPlane(float x, float y, Plane plane)
        {
            var ray = BoundingFrustum.ThroughPixel(World.Translation, x, y, Width, Height);
            var maybe = ray.Intersects(plane);
            return maybe.HasValue ? ray.Position + maybe.Value * ray.Direction : Vector3.Zero;
        }
    }
}
