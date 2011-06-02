using Microsoft.Xna.Framework;

namespace Pentacorn.Graphics
{
    class LineSegment : IVisible
    {
        public Vector3 From { get; set; }
        public Vector3 To { get; set; }

        public Color Color { get; set; }

        public Matrix Homography { get; set; }
        public Vector3 Dir { get { return Vector3.Normalize(To - From); } }        

        public float Thickness { get; set; }

        public LineSegment(Vector3 from, Vector3 to, Color color, float thickness = 0.01f)
        {
            From = from;
            To = to;
            Color = color;
            Thickness = thickness;
            Homography = Matrix.Identity;
        }

        public void Render(Renderer renderer, IViewProject viewProject)
        {
            throw new System.NotImplementedException();
        }

    }
}
