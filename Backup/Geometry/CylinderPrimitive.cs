using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



namespace Pentacorn.Vision.Geometry
{
    /// <summary>
    /// Geometric primitive class for drawing cylinders.
    /// </summary>
    class CylinderPrimitive : GeometricPrimitive<VertexPositionNormal>
    {
        /// <summary>
        /// Constructs a new cylinder primitive, using default settings.
        /// </summary>
        public CylinderPrimitive(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, 1, 1, 32)
        {
        }


        /// <summary>
        /// Constructs a new cylinder primitive,
        /// with the specified size and tessellation level.
        /// </summary>
        public CylinderPrimitive(GraphicsDevice graphicsDevice,
                                 float height, float diameter, int tessellation)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            height /= 2;

            float radius = diameter / 2;

            // Create a ring of triangles around the outside of the cylinder.
            for (int i = 0; i < tessellation; i++)
            {
                Vector3 normal = GetCircleVector(i, tessellation);

                AddVertex(new VertexPositionNormal(normal * radius + Vector3.Up * height, normal));
                AddVertex(new VertexPositionNormal(normal * radius + Vector3.Down * height, normal));

                AddIndex(i * 2);
                AddIndex(i * 2 + 1);
                AddIndex((i * 2 + 2) % (tessellation * 2));

                AddIndex(i * 2 + 1);
                AddIndex((i * 2 + 3) % (tessellation * 2));
                AddIndex((i * 2 + 2) % (tessellation * 2));
            }

            // Create flat triangle fan caps to seal the top and bottom.
            CreateCap(tessellation, height, radius, Vector3.Up);
            CreateCap(tessellation, height, radius, Vector3.Down);

            InitializePrimitive(graphicsDevice);
        }


        /// <summary>
        /// Helper method creates a triangle fan to close the ends of the cylinder.
        /// </summary>
        void CreateCap(int tessellation, float height, float radius, Vector3 normal)
        {
            // Create cap indices.
            for (int i = 0; i < tessellation - 2; i++)
            {
                if (normal.Y > 0)
                {
                    AddIndex(CurrentVertex);
                    AddIndex(CurrentVertex + (i + 1) % tessellation);
                    AddIndex(CurrentVertex + (i + 2) % tessellation);
                }
                else
                {
                    AddIndex(CurrentVertex);
                    AddIndex(CurrentVertex + (i + 2) % tessellation);
                    AddIndex(CurrentVertex + (i + 1) % tessellation);
                }
            }

            // Create cap vertices.
            for (int i = 0; i < tessellation; i++)
            {
                Vector3 position = GetCircleVector(i, tessellation) * radius +
                                   normal * height;

                AddVertex(new VertexPositionNormal(position, normal));
            }
        }


        /// <summary>
        /// Helper method computes a point on a circle.
        /// </summary>
        static Vector3 GetCircleVector(int i, int tessellation)
        {
            float angle = i * MathHelper.TwoPi / tessellation;

            float dx = (float)Math.Cos(angle);
            float dz = (float)Math.Sin(angle);

            return new Vector3(dx, 0, dz);
        }
    }
}
