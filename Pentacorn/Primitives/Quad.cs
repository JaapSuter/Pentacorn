using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pentacorn.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pentacorn
{
    class Quad : IVisible
    {
        public Texture2D Texture2D { get; set; }
        public Matrix World { get; set; }
        public Matrix Homography { get; set; }
        public VertexPositionColorTexture[] Vertices { get; private set; }
        public Color Color { get; set; }

        public Quad(Vector3 c0, Vector3 c1, Vector3 c2, Vector3 c3, Texture2D texture2D, Color color)
        {
            World = Matrix.Identity;
            Homography = Matrix.Identity;
            Color = color;
            Texture2D = texture2D;
            Vertices = new[] {
                new VertexPositionColorTexture(c0, Color, new Vector2(0, 0)),
                new VertexPositionColorTexture(c1, Color, new Vector2(1, 0)),
                new VertexPositionColorTexture(c2, Color, new Vector2(0, 1)),
                new VertexPositionColorTexture(c3, Color, new Vector2(1, 1)),
            };
        }

        public void Reinitialize(Vector3 c0, Vector3 c1, Vector3 c2, Vector3 c3)
        {
            Vertices = new[] {
                new VertexPositionColorTexture(c0, Color, new Vector2(0, 0)),
                new VertexPositionColorTexture(c1, Color, new Vector2(1, 0)),
                new VertexPositionColorTexture(c2, Color, new Vector2(0, 1)),
                new VertexPositionColorTexture(c3, Color, new Vector2(1, 1)),
            };
        }

        public Quad(Texture2D texture2D, Color color, Matrix homography)
            : this(Vector3.Zero, new Vector3(texture2D.Width, 0, 0), new Vector3(0, texture2D.Height, 0), new Vector3(texture2D.Width, texture2D.Height, 0), texture2D, color)
        {
            Homography = homography;
        }

        public Quad(Size size, Texture2D texture2D)
            : this(size.ToQuadCorners(), texture2D, Color.White)
        { }

        public Quad(RectangleF rect, Texture2D texture2D)
            : this(rect.ToQuadCorners(), texture2D, Color.White)
        { }

        public Quad(Size size, Texture2D texture2D, float zBias)
            : this(size.ToQuadCorners(), texture2D, Color.White, zBias)
        { }

        public Quad(Rectangle rect, Texture2D texture2D, Color color)
            : this(rect.ToQuadCorners(), texture2D, color)
        { }
                
        public Quad(PointF c0, PointF c1, PointF c2, PointF c3, Texture2D texture2D, Color color, float zBias = 0)
            : this(c0.ToVector3(zBias), c1.ToVector3(zBias), c2.ToVector3(zBias), c3.ToVector3(zBias), texture2D, color)
        { }

        public Quad(PointF[] corners, Texture2D texture2D, Color color, float zBias = 0)
            : this(corners[0], corners[1], corners[2], corners[3], texture2D, color)
        { }

        public Quad(Vector3[] corners, Texture2D texture2D, Color color)
            : this(corners[0], corners[1], corners[2], corners[3], texture2D, color)
        { }

        public void Render(Renderer renderer, IViewProject viewProject)
        {
            throw new System.NotImplementedException();
        }

    }
}

