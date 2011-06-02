using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Graphics
{
    class RenderableSphere : RenderablePrimitiveBase<VertexPositionNormal, Sphere>
    {
        public RenderableSphere(RendererImpl rendererImpl)
            : this(rendererImpl, 1, 4)
        {
        }

        private RenderableSphere(RendererImpl rendererImpl, float diameter, int tessellation)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            int verticalSegments = tessellation;
            int horizontalSegments = tessellation * 2;

            float radius = diameter / 2;

            // Start with a single vertex at the bottom of the sphere.
            AddVertex(new VertexPositionNormal(Vector3.Down * radius, Vector3.Down));

            // Create rings of vertices at progressively higher latitudes.
            for (int i = 0; i < verticalSegments - 1; i++)
            {
                float latitude = ((i + 1) * MathHelper.Pi /
                                            verticalSegments) - MathHelper.PiOver2;

                float dy = (float)Math.Sin(latitude);
                float dxz = (float)Math.Cos(latitude);

                // Create a single ring of vertices at this latitude.
                for (int j = 0; j < horizontalSegments; j++)
                {
                    float longitude = j * MathHelper.TwoPi / horizontalSegments;

                    float dx = (float)Math.Cos(longitude) * dxz;
                    float dz = (float)Math.Sin(longitude) * dxz;

                    Vector3 normal = new Vector3(dx, dy, dz);

                    AddVertex(new VertexPositionNormal(normal * radius, normal));
                }
            }

            // Finish with a single vertex at the top of the sphere.
            AddVertex(new VertexPositionNormal(Vector3.Up * radius, Vector3.Up));

            // Create a fan connecting the bottom vertex to the bottom latitude ring.
            for (int i = 0; i < horizontalSegments; i++)
            {
                AddIndex(0);
                AddIndex(1 + (i + 1) % horizontalSegments);
                AddIndex(1 + i);
            }

            // Fill the sphere body with triangles joining each pair of latitude rings.
            for (int i = 0; i < verticalSegments - 2; i++)
            {
                for (int j = 0; j < horizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % horizontalSegments;

                    AddIndex(1 + i * horizontalSegments + j);
                    AddIndex(1 + i * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + j);

                    AddIndex(1 + i * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + j);
                }
            }

            // Create a fan connecting the top vertex to the top latitude ring.
            for (int i = 0; i < horizontalSegments; i++)
            {
                AddIndex(CurrentVertex - 1);
                AddIndex(CurrentVertex - 2 - (i + 1) % horizontalSegments);
                AddIndex(CurrentVertex - 2 - i);
            }

            BasicEffect = new BasicEffect(rendererImpl.Device)
            {
                VertexColorEnabled = false,
                LightingEnabled = true,
                TextureEnabled = false,
            };

            BasicEffect.EnableDefaultLighting();
            FinishConstruction(rendererImpl.Device);
        }

        public override void Render(RendererImpl rendererImpl, IViewProject viewProject, Sphere sphere)
        {
            BasicEffect.World = Matrix.CreateScale(sphere.Radius) * Matrix.CreateTranslation(sphere.Position);
            BasicEffect.View = viewProject.View;
            BasicEffect.Projection = viewProject.Projection;
            BasicEffect.DiffuseColor = sphere.Color.ToVector3();
            BasicEffect.Alpha = sphere.Color.A / 255.0f;
            
            base.Render(rendererImpl, BasicEffect);
        }

        private BasicEffect BasicEffect;
    }
}
