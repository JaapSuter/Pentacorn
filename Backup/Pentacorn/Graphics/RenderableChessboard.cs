using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Graphics
{
    class RenderableChessboard : Renderable<Chessboard>
    {
        public RenderableChessboard(RendererImpl rendererImpl)        
        {
            BasicEffect = new BasicEffect(rendererImpl.Device)
            {
                VertexColorEnabled = false,
                TextureEnabled = false,
            };
        }

        public override void Render(RendererImpl rendererImpl, IViewProject viewProject, Chessboard chessboard)
        {
            Buffers buffers;
            if (!CachedBuffers.TryGetValue(chessboard, out buffers))            
                CachedBuffers[chessboard] = buffers = new Buffers(rendererImpl, chessboard);

            BasicEffect.World = chessboard.Homography;
            BasicEffect.View = viewProject.View;
            BasicEffect.Projection = viewProject.Projection;
            
            rendererImpl.Device.RasterizerState = RasterizerState.CullNone;
            rendererImpl.Device.DepthStencilState = DepthStencilState.Default;
            rendererImpl.Device.SetVertexBuffer(buffers.VertexBuffer);
            rendererImpl.Device.Indices = buffers.IndexBuffer;

            var numTriangles = 2 * chessboard.TileCount.Area();
            var numIndicesPerColor = 3 * numTriangles;

            BasicEffect.Alpha = chessboard.BlackColor.A / 255.0f;
            BasicEffect.DiffuseColor = chessboard.BlackColor.ToVector3();
            BasicEffect.CurrentTechnique.Passes[0].Apply();
            rendererImpl.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numIndicesPerColor, 0, numTriangles);

            BasicEffect.Alpha = chessboard.WhiteColor.A / 255.0f;
            BasicEffect.DiffuseColor = chessboard.WhiteColor.ToVector3();
            BasicEffect.CurrentTechnique.Passes[0].Apply();
            rendererImpl.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numIndicesPerColor, numIndicesPerColor, numTriangles);
        }

        private struct Buffers
        {
            public Buffers(RendererImpl rendererImpl, Chessboard chessboard)
            {
                var numTriangles = 2 * chessboard.TileCount.Area();
                var numIndicesPerColor = 3 * numTriangles;
                var idxs = new short[2 * numIndicesPerColor];
                var verts = chessboard.Vertices.ToArrayOf(v => new VertexPosition(v));

                IndexBuffer = new IndexBuffer(rendererImpl.Device, IndexElementSize.SixteenBits, idxs.Length, BufferUsage.None);
                VertexBuffer = new VertexBuffer(rendererImpl.Device, VertexPosition.VertexDeclaration, verts.Length, BufferUsage.None);

                var idxBlack = 0;
                var idxWhite = numIndicesPerColor;
                for (var n = 0; n < chessboard.TileCount.Height; ++n)
                    for (var m = 0; m < chessboard.TileCount.Width; ++m)
                    {
                        var bw = (n ^ m) % 2 == 0;
                        var offset = bw ? idxBlack : idxWhite;

                        idxs[offset++] = (short)(n * chessboard.VertexCount.Width + m);
                        idxs[offset++] = (short)(n * chessboard.VertexCount.Width + m + 1);
                        idxs[offset++] = (short)(n * chessboard.VertexCount.Width + m + chessboard.VertexCount.Width);

                        idxs[offset++] = (short)(n * chessboard.VertexCount.Width + m + chessboard.VertexCount.Width);
                        idxs[offset++] = (short)(n * chessboard.VertexCount.Width + m + 1);
                        idxs[offset++] = (short)(n * chessboard.VertexCount.Width + m + chessboard.VertexCount.Width + 1);
                        
                        if (bw)                            
                            idxBlack = offset;
                        else
                            idxWhite = offset;
                    }

                IndexBuffer.SetData(idxs);
                VertexBuffer.SetData(verts);            
            }

            public VertexBuffer VertexBuffer;
            public IndexBuffer IndexBuffer;
        
        }

        private Dictionary<Chessboard, Buffers> CachedBuffers = new Dictionary<Chessboard, Buffers>();
        private BasicEffect BasicEffect;
    }
}
