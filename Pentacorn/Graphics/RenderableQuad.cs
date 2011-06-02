using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Graphics
{
    class RenderableQuad : Renderable<Quad>
    {
        public RenderableQuad(RendererImpl rendererImpl)
        {
            RgbaEffect = rendererImpl.Loader.Load<Effect>("Content/Shaders/RgbaTexture");
            GrayEffect = rendererImpl.Loader.Load<Effect>("Content/Shaders/GrayTexture");

            var vis = new ushort[]
            { 
                0, 1, 2,
                2, 1, 3,
            };

            VertexBuffer = new VertexBuffer(rendererImpl.Device, VertexPositionColorTexture.VertexDeclaration, 4, BufferUsage.None);
            IndexBuffer = new IndexBuffer(rendererImpl.Device, typeof(ushort), vis.Length, BufferUsage.None);
            IndexBuffer.SetData(vis);
        }

        public override void Render(RendererImpl rendererImpl, IViewProject viewProject, Quad quad)
        {
            VertexBuffer.SetData(quad.Vertices);

            var effect = quad.Texture2D.Format == SurfaceFormat.Alpha8 ? GrayEffect : RgbaEffect;

            effect.Parameters["WorldViewProj"].SetValue(quad.Homography * quad.World * viewProject.View * viewProject.Projection);
            effect.Parameters["Color"].SetValue(quad.Color.ToVector4());
            effect.GraphicsDevice.BlendState = BlendState.Opaque;
            effect.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            effect.CurrentTechnique.Passes[0].Apply();

            rendererImpl.Device.DepthStencilState = new DepthStencilState() { DepthBufferFunction = CompareFunction.LessEqual }; //DepthStencilState.Default;
            rendererImpl.Device.SetVertexBuffer(VertexBuffer);
            rendererImpl.Device.Textures[0] = quad.Texture2D;
            rendererImpl.Device.Indices = IndexBuffer;
            rendererImpl.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            rendererImpl.Device.SetVertexBuffer(null);
        }

        private VertexBuffer VertexBuffer;
        private IndexBuffer IndexBuffer;
        private Effect RgbaEffect;
        private Effect GrayEffect;
    }
}
