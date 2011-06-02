using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Vision.Geometry
{
    class ChessboardPrimitive : GeometricPrimitive<VertexPositionColor>
    {
        public ChessboardPrimitive(GraphicsDevice graphicsDevice, int width, int height, float size, Color black, Color white)
        {
            this.width = width;
            this.height = height;
            this.size = size;

            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                {
                    AddIndex(CurrentVertex + 0);
                    AddIndex(CurrentVertex + 2);
                    AddIndex(CurrentVertex + 1);

                    AddIndex(CurrentVertex + 0);
                    AddIndex(CurrentVertex + 3);
                    AddIndex(CurrentVertex + 2);

                    AddIndex(CurrentVertex + 0);
                    AddIndex(CurrentVertex + 1);
                    AddIndex(CurrentVertex + 2);

                    AddIndex(CurrentVertex + 0);
                    AddIndex(CurrentVertex + 2);
                    AddIndex(CurrentVertex + 3);

                    Color color = (0 == ((x ^ y) & 1)) ? black : white;

                    AddVertex(new VertexPositionColor(new Vector3(x * size, y * size, 0), color));
                    AddVertex(new VertexPositionColor(new Vector3(x * size + size, y * size, 0), color));
                    AddVertex(new VertexPositionColor(new Vector3(x * size + size, y * size + size, 0), color));
                    AddVertex(new VertexPositionColor(new Vector3(x * size, y * size + size, 0), color));
                }

            InitializePrimitive(graphicsDevice, useLighting: false);
        }

        private int width;
        private int height;
        private float size;
    }
}
