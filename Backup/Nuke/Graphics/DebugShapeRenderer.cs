using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Vision.Graphics
{
    class DebugShapeRenderer : IRenderable
    {
        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            if (graphics != null)
                throw new InvalidOperationException("Initialize can only be called once.");

            graphics = graphicsDevice;

            effect = new BasicEffect(graphicsDevice);
            effect.VertexColorEnabled = true;
            effect.TextureEnabled = false;
            effect.DiffuseColor = Vector3.One;
            effect.World = Matrix.Identity;

            InitializeSphere();
        }
        
        public static void AddLine(Vector3 a, Vector3 b, Color color)
        {
            // Get a DebugShape we can use to draw the line
            DebugShape shape = GetShapeForLines(1);

            // Add the two vertices to the shape
            shape.Vertices[0] = new VertexPositionColor(a, color);
            shape.Vertices[1] = new VertexPositionColor(b, color);
        }

        public static void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            // Get a DebugShape we can use to draw the triangle
            DebugShape shape = GetShapeForLines(3);

            // Add the vertices to the shape
            shape.Vertices[0] = new VertexPositionColor(a, color);
            shape.Vertices[1] = new VertexPositionColor(b, color);
            shape.Vertices[2] = new VertexPositionColor(b, color);
            shape.Vertices[3] = new VertexPositionColor(c, color);
            shape.Vertices[4] = new VertexPositionColor(c, color);
            shape.Vertices[5] = new VertexPositionColor(a, color);
        }

        public static void AddBoundingFrustum(BoundingFrustum frustum, Color color)
        {
            // Get a DebugShape we can use to draw the frustum
            DebugShape shape = GetShapeForLines(12);

            // Get the corners of the frustum
            frustum.GetCorners(corners);

            // Fill in the vertices for the bottom of the frustum
            shape.Vertices[0] = new VertexPositionColor(corners[0], color);
            shape.Vertices[1] = new VertexPositionColor(corners[1], color);
            shape.Vertices[2] = new VertexPositionColor(corners[1], color);
            shape.Vertices[3] = new VertexPositionColor(corners[2], color);
            shape.Vertices[4] = new VertexPositionColor(corners[2], color);
            shape.Vertices[5] = new VertexPositionColor(corners[3], color);
            shape.Vertices[6] = new VertexPositionColor(corners[3], color);
            shape.Vertices[7] = new VertexPositionColor(corners[0], color);

            // Fill in the vertices for the top of the frustum
            shape.Vertices[8] = new VertexPositionColor(corners[4], color);
            shape.Vertices[9] = new VertexPositionColor(corners[5], color);
            shape.Vertices[10] = new VertexPositionColor(corners[5], color);
            shape.Vertices[11] = new VertexPositionColor(corners[6], color);
            shape.Vertices[12] = new VertexPositionColor(corners[6], color);
            shape.Vertices[13] = new VertexPositionColor(corners[7], color);
            shape.Vertices[14] = new VertexPositionColor(corners[7], color);
            shape.Vertices[15] = new VertexPositionColor(corners[4], color);

            // Fill in the vertices for the vertical sides of the frustum
            shape.Vertices[16] = new VertexPositionColor(corners[0], color);
            shape.Vertices[17] = new VertexPositionColor(corners[4], color);
            shape.Vertices[18] = new VertexPositionColor(corners[1], color);
            shape.Vertices[19] = new VertexPositionColor(corners[5], color);
            shape.Vertices[20] = new VertexPositionColor(corners[2], color);
            shape.Vertices[21] = new VertexPositionColor(corners[6], color);
            shape.Vertices[22] = new VertexPositionColor(corners[3], color);
            shape.Vertices[23] = new VertexPositionColor(corners[7], color);
        }

        public static void AddBoundingBox(BoundingBox box, Color color)
        {
            // Get a DebugShape we can use to draw the box
            DebugShape shape = GetShapeForLines(12);

            // Get the corners of the box
            box.GetCorners(corners);

            // Fill in the vertices for the bottom of the box
            shape.Vertices[0] = new VertexPositionColor(corners[0], color);
            shape.Vertices[1] = new VertexPositionColor(corners[1], color);
            shape.Vertices[2] = new VertexPositionColor(corners[1], color);
            shape.Vertices[3] = new VertexPositionColor(corners[2], color);
            shape.Vertices[4] = new VertexPositionColor(corners[2], color);
            shape.Vertices[5] = new VertexPositionColor(corners[3], color);
            shape.Vertices[6] = new VertexPositionColor(corners[3], color);
            shape.Vertices[7] = new VertexPositionColor(corners[0], color);

            // Fill in the vertices for the top of the box
            shape.Vertices[8] = new VertexPositionColor(corners[4], color);
            shape.Vertices[9] = new VertexPositionColor(corners[5], color);
            shape.Vertices[10] = new VertexPositionColor(corners[5], color);
            shape.Vertices[11] = new VertexPositionColor(corners[6], color);
            shape.Vertices[12] = new VertexPositionColor(corners[6], color);
            shape.Vertices[13] = new VertexPositionColor(corners[7], color);
            shape.Vertices[14] = new VertexPositionColor(corners[7], color);
            shape.Vertices[15] = new VertexPositionColor(corners[4], color);

            // Fill in the vertices for the vertical sides of the box
            shape.Vertices[16] = new VertexPositionColor(corners[0], color);
            shape.Vertices[17] = new VertexPositionColor(corners[4], color);
            shape.Vertices[18] = new VertexPositionColor(corners[1], color);
            shape.Vertices[19] = new VertexPositionColor(corners[5], color);
            shape.Vertices[20] = new VertexPositionColor(corners[2], color);
            shape.Vertices[21] = new VertexPositionColor(corners[6], color);
            shape.Vertices[22] = new VertexPositionColor(corners[3], color);
            shape.Vertices[23] = new VertexPositionColor(corners[7], color);
        }

        public static void AddBoundingSphere(BoundingSphere sphere, Color color)
        {
            // Get a DebugShape we can use to draw the sphere
            DebugShape shape = GetShapeForLines(sphereLineCount);

            // Iterate our unit sphere vertices
            for (int i = 0; i < unitSphere.Length; i++)
            {
                // Compute the vertex position by transforming the point by the radius and center of the sphere
                Vector3 vertPos = unitSphere[i] * sphere.Radius + sphere.Center;

                // Add the vertex to the shape
                shape.Vertices[i] = new VertexPositionColor(vertPos, color);
            }
        }

        public virtual void Render(object obj, Matrix view, Matrix projection)        
        {
            // Update our effect with the matrices.
            effect.View = view;
            effect.Projection = projection;

            // Calculate the total number of vertices we're going to be rendering.
            int vertexCount = 0;
            foreach (var shape in activeShapes)
                vertexCount += shape.LineCount * 2;

            // If we have some vertices to draw
            if (vertexCount > 0)
            {
                // Make sure our array is large enough
                if (verts.Length < vertexCount)
                {
                    // If we have to resize, we make our array twice as large as necessary so
                    // we hopefully won't have to resize it for a while.
                    verts = new VertexPositionColor[vertexCount * 2];
                }

                // Now go through the shapes again to move the vertices to our array and
                // add up the number of lines to draw.
                int lineCount = 0;
                int vertIndex = 0;
                foreach (DebugShape shape in activeShapes)
                {
                    lineCount += shape.LineCount;
                    int shapeVerts = shape.LineCount * 2;
                    for (int i = 0; i < shapeVerts; i++)
                        verts[vertIndex++] = shape.Vertices[i];
                }

                // Start our effect to begin rendering.
                effect.CurrentTechnique.Passes[0].Apply();

                // We draw in a loop because the Reach profile only supports 65,535 primitives. While it's
                // not incredibly likely, if a game tries to render more than 65,535 lines we don't want to
                // crash. We handle this by doing a loop and drawing as many lines as we can at a time, capped
                // at our limit. We then move ahead in our vertex array and draw the next set of lines.
                int vertexOffset = 0;
                while (lineCount > 0)
                {
                    // Figure out how many lines we're going to draw
                    int linesToDraw = Math.Min(lineCount, 65535);

                    // Draw the lines
                    graphics.DrawUserPrimitives(PrimitiveType.LineList, verts, vertexOffset, linesToDraw);

                    // Move our vertex offset ahead based on the lines we drew
                    vertexOffset += linesToDraw * 2;

                    // Remove these lines from our total line count
                    lineCount -= linesToDraw;
                }
            }

            // Go through our active shapes and retire any shapes that have expired to the
            // cache list. 
            bool resort = false;
            for (int i = activeShapes.Count - 1; i >= 0; i--)
            {
                DebugShape s = activeShapes[i];
                cachedShapes.Add(s);
                activeShapes.RemoveAt(i);
                resort = true;
            }

            // If we move any shapes around, we need to resort the cached list
            // to ensure that the smallest shapes are first in the list.
            if (resort)
                cachedShapes.Sort(CachedShapesSort);
        }
        
        private static void InitializeSphere()
        {
            // We need two vertices per line, so we can allocate our vertices
            unitSphere = new Vector3[sphereLineCount * 2];

            // Compute our step around each circle
            float step = MathHelper.TwoPi / sphereResolution;

            // Used to track the index into our vertex array
            int index = 0;

            // Create the loop on the XY plane first
            for (float a = 0f; a < MathHelper.TwoPi; a += step)
            {
                unitSphere[index++] = new Vector3((float)Math.Cos(a), (float)Math.Sin(a), 0f);
                unitSphere[index++] = new Vector3((float)Math.Cos(a + step), (float)Math.Sin(a + step), 0f);
            }

            // Next on the XZ plane
            for (float a = 0f; a < MathHelper.TwoPi; a += step)
            {
                unitSphere[index++] = new Vector3((float)Math.Cos(a), 0f, (float)Math.Sin(a));
                unitSphere[index++] = new Vector3((float)Math.Cos(a + step), 0f, (float)Math.Sin(a + step));
            }

            // Finally on the YZ plane
            for (float a = 0f; a < MathHelper.TwoPi; a += step)
            {
                unitSphere[index++] = new Vector3(0f, (float)Math.Cos(a), (float)Math.Sin(a));
                unitSphere[index++] = new Vector3(0f, (float)Math.Cos(a + step), (float)Math.Sin(a + step));
            }
        }

        private static int CachedShapesSort(DebugShape s1, DebugShape s2)
        {
            return s1.Vertices.Length.CompareTo(s2.Vertices.Length);
        }

        private static DebugShape GetShapeForLines(int lineCount)
        {
            DebugShape shape = null;

            // We go through our cached list trying to find a shape that contains
            // a large enough array to hold our desired line count. If we find such
            // a shape, we move it from our cached list to our active list and break
            // out of the loop.
            int vertCount = lineCount * 2;
            for (int i = 0; i < cachedShapes.Count; i++)
            {
                if (cachedShapes[i].Vertices.Length >= vertCount)
                {
                    shape = cachedShapes[i];
                    cachedShapes.RemoveAt(i);
                    activeShapes.Add(shape);
                    break;
                }
            }

            // If we didn't find a shape in our cache, we create a new shape and add it
            // to the active list.
            if (shape == null)
            {
                shape = new DebugShape { Vertices = new VertexPositionColor[vertCount] };
                activeShapes.Add(shape);
            }

            // Set the line count and lifetime of the shape based on our parameters.
            shape.LineCount = lineCount;

            return shape;
        }

        private class DebugShape
        {
            public VertexPositionColor[] Vertices;
            public int LineCount;
        }

        private static readonly List<DebugShape> cachedShapes = new List<DebugShape>();
        private static readonly List<DebugShape> activeShapes = new List<DebugShape>();
        private static VertexPositionColor[] verts = new VertexPositionColor[64];
        private static GraphicsDevice graphics;
        private static BasicEffect effect;
        private static Vector3[] corners = new Vector3[8];
        private const int sphereResolution = 30;
        private const int sphereLineCount = (sphereResolution + 1) * 3;
        private static Vector3[] unitSphere;
    }
}
