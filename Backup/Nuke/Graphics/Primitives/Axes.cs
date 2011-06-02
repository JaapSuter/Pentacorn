using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Pentacorn.Vision.Graphics.Primitives
{
    class Axes : PrimitiveBase<VertexPositionColor>
    {
        public Axes(GraphicsDevice device, float size = 0.2f)
        {
            AddIndex(0);
            AddIndex(1);
            AddIndex(2);
            
            AddIndex(0);
            AddIndex(2);
            AddIndex(1);

            AddIndex(0);
            AddIndex(3);
            AddIndex(4);
            AddIndex(0);
            AddIndex(4);
            AddIndex(3);

            AddIndex(0);
            AddIndex(5);
            AddIndex(6);
            AddIndex(0);
            AddIndex(6);
            AddIndex(5);

            const float sm = 0.2f;

            AddVertex(new VertexPositionColor(Vector3.Zero, Color.White));
            AddVertex(new VertexPositionColor(size * Vector3.UnitX, Color.Red));
            AddVertex(new VertexPositionColor(size * Vector3.UnitX + sm * size * Vector3.UnitY, Color.Red));
            AddVertex(new VertexPositionColor(size * Vector3.UnitY, Color.Green));
            AddVertex(new VertexPositionColor(size * Vector3.UnitY + sm * size * Vector3.UnitZ, Color.Green));
            AddVertex(new VertexPositionColor(size * Vector3.UnitZ, Color.Blue));
            AddVertex(new VertexPositionColor(size * Vector3.UnitZ + sm * size * Vector3.UnitX, Color.Blue));

            FinishConstruction(device, new BasicEffect(device)
            {
                LightingEnabled = false,
                FogEnabled = false,
                TextureEnabled = false,
                VertexColorEnabled = true,                 
            });
        }
    }
}
