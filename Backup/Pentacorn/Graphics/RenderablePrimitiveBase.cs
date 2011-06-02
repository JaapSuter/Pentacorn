using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Graphics
{
    abstract class RenderablePrimitiveBase<VertexType, Primitive> : Renderable<Primitive>
        where VertexType : struct
    {
        protected int CurrentVertex { get { return vertices.Count; } }
        
        protected void AddVertex(VertexType vertex)
        {
            vertices.Add(vertex);
        }

        protected void AddIndex(int index)
        {
            indices.Add((uint)index);
        }

        protected void FinishConstruction(GraphicsDevice device)
        {
            if (vertexBuffer != null)
                vertexBuffer.Dispose();

            vertexBuffer = new VertexBuffer(device, typeof(VertexType), vertices.Count, BufferUsage.None);
            vertexBuffer.SetData(vertices.ToArray());

            indexBuffer = new IndexBuffer(device, typeof(uint), indices.Count, BufferUsage.None);
            indexBuffer.SetData(indices.ToArray());
        }

        protected void Render(RendererImpl rendererImpl, Effect effect)
        {
            rendererImpl.Device.SetVertexBuffer(vertexBuffer);
            rendererImpl.Device.Indices = indexBuffer;

            foreach (var effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                var batch = 900000;
                var total = indices.Count / 3;
                var offset = 0;
                for (int remaining = total; remaining > 0; )
                {
                    var actual = Math.Min(batch, remaining);
                    rendererImpl.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, actual * 3, offset * 3, actual);
                    remaining -= actual;
                    offset += actual;
                }
            }
        }

        private List<VertexType> vertices = new List<VertexType>();
        private List<uint> indices = new List<uint>();
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
    }
}
