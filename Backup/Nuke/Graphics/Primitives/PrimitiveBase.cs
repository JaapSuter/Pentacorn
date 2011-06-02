using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Vision.Graphics.Primitives
{
    class PrimitiveBase<VertexType> : IRenderable where VertexType : struct
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

        protected void FinishConstruction(GraphicsDevice device, Effect effect)
        {
            this.Effect = effect;

            if (vertexBuffer != null)
                vertexBuffer.Dispose();

            vertexBuffer = new VertexBuffer(device, typeof(VertexType), vertices.Count, BufferUsage.None);
            vertexBuffer.SetData(vertices.ToArray());

            indexBuffer = new IndexBuffer(device, typeof(uint), indices.Count, BufferUsage.None);
            indexBuffer.SetData(indices.ToArray());
        }

        protected Effect Effect { get; private set; }

        public virtual void Render(object obj, Matrix view, Matrix projection)
        {
            var world = (Matrix)obj;
            this.Effect.Parameters["WorldViewProj"].SetValue(world * view * projection);
            Render();
        }

        private void Render(BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null)
        {
            GraphicsDevice device = this.Effect.GraphicsDevice;

            if (blendState != null) device.BlendState = blendState;
            if (samplerState != null) device.SamplerStates[0] = samplerState;
            if (depthStencilState != null) device.DepthStencilState = depthStencilState;
            if (rasterizerState != null) device.RasterizerState = rasterizerState;
                
            device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;

            foreach (var effectPass in this.Effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                var batch = 900000;
                var total = indices.Count / 3;
                var offset = 0;
                for (int remaining = total; remaining > 0; )
                {
                    var actual = Math.Min(batch, remaining);
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, actual * 3, offset * 3, actual);
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
