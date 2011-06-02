using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pentacorn.Vision.Graphics;

namespace Pentacorn.Vision.Geometry
{
    class GeometricPrimitive<VertexType> : IDisposable where VertexType : struct
    {
        public Color Color { get; set; }

        public override void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {
            // Set BasicEffect parameters.
            basicEffect.World = this.World;
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.DiffuseColor = this.Color.ToVector3();
            basicEffect.Alpha = this.Color.A / 255.0f;

            GraphicsDevice device = basicEffect.GraphicsDevice;
            device.DepthStencilState = DepthStencilState.Default;

            if (this.Color.A < 255)
                device.BlendState = BlendState.AlphaBlend;
            else
                device.BlendState = BlendState.Opaque;

            Draw(basicEffect);
        }

        protected GeometricPrimitive()
        {
            this.Color = Color.LightPink;
        }

        protected void AddVertex(VertexType vertex)
        {
            vertices.Add(vertex);
        }

        protected void AddIndex(int index)
        {
            if (index > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("index");

            indices.Add((ushort)index);
        }

        protected int CurrentVertex
        {
            get { return vertices.Count; }
        }

        protected void InitializePrimitive(GraphicsDevice graphicsDevice, bool useLighting = true)
        {
            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexType), vertices.Count, BufferUsage.None);
            vertexBuffer.SetData(vertices.ToArray());

            indexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort), indices.Count, BufferUsage.None);
            indexBuffer.SetData(indices.ToArray());

            basicEffect = new BasicEffect(graphicsDevice);

            if (useLighting)
            {
                basicEffect.EnableDefaultLighting();
                basicEffect.PreferPerPixelLighting = true;
            }
            else
            {
                basicEffect.VertexColorEnabled = true;
                basicEffect.LightingEnabled = false;
            }
        }

        ~GeometricPrimitive()
        {
            Dispose(false);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (vertexBuffer != null)
                    vertexBuffer.Dispose();

                if (indexBuffer != null)
                    indexBuffer.Dispose();

                if (basicEffect != null)
                    basicEffect.Dispose();
            }
        }

        private void Draw(Effect effect)
        {
            GraphicsDevice graphicsDevice = effect.GraphicsDevice;

            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;

            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();

                int primitiveCount = indices.Count / 3;

                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                     vertices.Count, 0, primitiveCount);

            }
        }


        private List<VertexType> vertices = new List<VertexType>();
        private List<ushort> indices = new List<ushort>();

        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private BasicEffect basicEffect;
    }
}
