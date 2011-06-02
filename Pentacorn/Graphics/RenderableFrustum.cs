using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Graphics
{
    class RenderableFrustum : Renderable<Frustum>
    {
        public RenderableFrustum(RendererImpl rendererImpl)
        {
            Effect = new BasicEffect(rendererImpl.Device);
            Effect.VertexColorEnabled = false;
            Effect.LightingEnabled = false;
            Effect.TextureEnabled = false;

            Indices = new int[]
            { 
                0, 1, 1, 2, 2, 3, 3, 0,
                0, 4, 1, 5, 2, 6, 3, 7,
                4, 5, 5, 6, 6, 7, 7, 4
            };

            VertexArray = new []
            {                
                new VertexPosition(new Vector3(-1, -1, 0)),
                new VertexPosition(new Vector3( 1, -1, 0)),
                new VertexPosition(new Vector3( 1,  1, 0)),
                new VertexPosition(new Vector3(-1,  1, 0)),
                                                         
                new VertexPosition(new Vector3(-1, -1, 1)),
                new VertexPosition(new Vector3( 1, -1, 1)),
                new VertexPosition(new Vector3( 1,  1, 1)),
                new VertexPosition(new Vector3(-1,  1, 1)),
            };

            VertexBuffer = new VertexBuffer(rendererImpl.Device, VertexPosition.VertexDeclaration, VertexArray.Length, BufferUsage.None);
            VertexBuffer.SetData(VertexArray);

            IndexBuffer = new IndexBuffer(rendererImpl.Device, typeof(int), Indices.Length, BufferUsage.None);
            IndexBuffer.SetData(Indices);

            Rc2D = new RenderableCircle2D(rendererImpl);
            Rl2D = new RenderableLineSegment(rendererImpl);
        }

        public override void Render(RendererImpl rendererImpl, IViewProject viewProject, Frustum frustum)
        {
            var boundingFrustum = frustum.BoundingFrustum;
            var corners = boundingFrustum.GetCorners();
            var projToWorld = Matrix.Invert(frustum.View * frustum.Projection);

            Effect.DiffuseColor = frustum.Color.ToVector3();
            Effect.Parameters["WorldViewProj"].SetValue(projToWorld * viewProject.View * viewProject.Projection);
            Effect.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Effect.CurrentTechnique.Passes[0].Apply();

            rendererImpl.Device.SetVertexBuffer(VertexBuffer);
            rendererImpl.Device.Indices = IndexBuffer;
            rendererImpl.Device.DrawUserIndexedPrimitives(PrimitiveType.LineList, VertexArray, 0, 8, Indices, 0, Indices.Length / 2);

            var plane = new Plane(Vector3.UnitZ, 0);

            var fourCorners = new [] {
                frustum.IntersectWithPlane(0, 0, plane),
                frustum.IntersectWithPlane(frustum.Width, 0, plane),
                frustum.IntersectWithPlane(frustum.Width, frustum.Height, plane),                
                frustum.IntersectWithPlane(0, frustum.Height, plane),  };

            var shots = new [] 
            {
                boundingFrustum.ThroughPixel(frustum.World.Translation, 0, 0, 100, 100),
                boundingFrustum.ThroughPixel(frustum.World.Translation, 100, 0, 100, 100),
                boundingFrustum.ThroughPixel(frustum.World.Translation, 0, 100, 100, 100),
                boundingFrustum.ThroughPixel(frustum.World.Translation, 100, 100, 100, 100),
                boundingFrustum.ThroughPixel(frustum.World.Translation, 25, 50, 100, 100),
                boundingFrustum.ThroughPixel(frustum.World.Translation, 50, 50, 100, 100),
                boundingFrustum.ThroughPixel(frustum.World.Translation, 75, 50, 100, 100),
            };

            shots.Select((s, i) => { Rl2D.Render(rendererImpl, viewProject, new LineSegment(s.Position, s.Position + s.Direction * 100, Palette.Get(i))); return 42; }).Run();

            Rc2D.Render(rendererImpl, viewProject, new Circle2D(fourCorners[0], 14, 4, Palette.Get(0)));
            Rc2D.Render(rendererImpl, viewProject, new Circle2D(fourCorners[1], 14, 4, Palette.Get(1)));
            Rc2D.Render(rendererImpl, viewProject, new Circle2D(fourCorners[3], 14, 4, Palette.Get(2)));
            
            Rl2D.Render(rendererImpl, viewProject, new LineSegment(fourCorners[0], fourCorners[1], frustum.Color, 0.1f));
            Rl2D.Render(rendererImpl, viewProject, new LineSegment(fourCorners[1], fourCorners[2], frustum.Color, 0.1f));
            Rl2D.Render(rendererImpl, viewProject, new LineSegment(fourCorners[2], fourCorners[3], frustum.Color, 0.1f));
            Rl2D.Render(rendererImpl, viewProject, new LineSegment(fourCorners[3], fourCorners[0], frustum.Color, 0.1f));

            // Rl2D.Render(rendererImpl, viewProject, new LineSegment(corners[0], corners[4], frustum.Color, 0.2f));
            // Rl2D.Render(rendererImpl, viewProject, new LineSegment(corners[1], corners[5], frustum.Color, 0.2f));
            // Rl2D.Render(rendererImpl, viewProject, new LineSegment(corners[2], corners[6], frustum.Color, 0.2f));
            // Rl2D.Render(rendererImpl, viewProject, new LineSegment(corners[3], corners[7], frustum.Color, 0.2f));
        }

        private int[] Indices;
        private VertexBuffer VertexBuffer;
        private IndexBuffer IndexBuffer;
        private VertexPosition[] VertexArray;
        private BasicEffect Effect;
        private RenderableCircle2D Rc2D;
        private RenderableLineSegment Rl2D;
    }
}
