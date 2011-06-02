using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pentacorn;

namespace Pentacorn.Graphics
{
    class RenderableFrustumOld : Renderable<Frustum>
    {
        public RenderableFrustumOld(RendererImpl rendererImpl)
        {
            Effect = new BasicEffect(rendererImpl.Device);
            Effect.VertexColorEnabled = true;
            Effect.LightingEnabled = false;

            var vis = new int[]
            { 
                0, 1,
                1, 2,
                2, 3,
                3, 0,
                0, 4,
                1, 5,
                2, 6,
                3, 7,
                4, 5,
                5, 6,
                6, 7,
                7, 4
            };

            Indices = vis;
            VertexArray = new VertexPositionColor[8];
            VertexBuffer = new VertexBuffer(rendererImpl.Device, VertexPositionColor.VertexDeclaration, VertexArray.Length, BufferUsage.None);
            IndexBuffer = new IndexBuffer(rendererImpl.Device, typeof(int), vis.Length, BufferUsage.None);
            IndexBuffer.SetData(vis);

            Rc2D = new RenderableCircle2D(rendererImpl);
            Rl2D = new RenderableLineSegment(rendererImpl);
        }

        public override void Render(RendererImpl rendererImpl, Matrix viewProj, Frustum frustum)
        {
            var boundingFrustum = frustum.BoundingFrustum;
            var corners = boundingFrustum.GetCorners();

            for (int i = 0; i < corners.Length; ++i)
                VertexArray[i] = new VertexPositionColor(corners[i], frustum.Color);
            
            VertexBuffer.SetData(VertexArray);

            Effect.Parameters["WorldViewProj"].SetValue(viewProj);
            Effect.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Effect.CurrentTechnique.Passes[0].Apply();

            rendererImpl.Device.SetVertexBuffer(VertexBuffer);
            rendererImpl.Device.Indices = IndexBuffer;
            rendererImpl.Device.DrawUserIndexedPrimitives(PrimitiveType.LineList, VertexArray, 0, 8, Indices, 0, Indices.Length / 2);

            Rc2D.Render(rendererImpl, viewProj, new Circle2D(corners[4], 10, 4, Palette.Get(0)));
            Rc2D.Render(rendererImpl, viewProj, new Circle2D(corners[5], 8, 4, Palette.Get(1)));
            Rc2D.Render(rendererImpl, viewProj, new Circle2D(corners[7], 6, 4, Palette.Get(2)));

            var plane = new Plane(Vector3.UnitZ, 0);

            var fourCorners = new [] {
                IntersectWithPlane(0, 0, rendererImpl, plane, frustum),
                IntersectWithPlane(frustum.Width, 0, rendererImpl, plane, frustum),
                IntersectWithPlane(frustum.Width, frustum.Height, rendererImpl, plane, frustum),                
                IntersectWithPlane(0, frustum.Height, rendererImpl, plane, frustum),  };

            Rc2D.Render(rendererImpl, viewProj, new Circle2D(fourCorners[0], 14, 4, Palette.Get(0)));
            Rc2D.Render(rendererImpl, viewProj, new Circle2D(fourCorners[1], 14, 4, Palette.Get(1)));
            Rc2D.Render(rendererImpl, viewProj, new Circle2D(fourCorners[3], 14, 4, Palette.Get(2)));

            Rl2D.Render(rendererImpl, viewProj, new LineSegment(fourCorners[0], fourCorners[1], Palette.Get(0), 0.1f));
            Rl2D.Render(rendererImpl, viewProj, new LineSegment(fourCorners[1], fourCorners[2], Palette.Get(1), 0.1f));
            Rl2D.Render(rendererImpl, viewProj, new LineSegment(fourCorners[2], fourCorners[3], Palette.Get(2), 0.1f));
            Rl2D.Render(rendererImpl, viewProj, new LineSegment(fourCorners[3], fourCorners[0], Palette.Get(3), 0.1f));
        }

        private Vector3 IntersectWithPlane(float x, float y, RendererImpl rendererImpl, Plane plane, Frustum frustum)
        {
            var ray = frustum.BoundingFrustum.ThroughPixel(frustum.World.Translation, x, y, frustum.Width, frustum.Height);
            var maybe = ray.Intersects(plane);
            return maybe.HasValue ? ray.Position + maybe.Value * ray.Direction : Vector3.Zero;            
        }

        private int[] Indices;
        private VertexBuffer VertexBuffer;
        private IndexBuffer IndexBuffer;
        private VertexPositionColor[] VertexArray;
        private BasicEffect Effect;
        private RenderableCircle2D Rc2D;
        private RenderableLineSegment Rl2D;
    }
}
