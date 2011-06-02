using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Vision.Graphics.Primitives
{
    class ChessboardPicked : IRenderable
    {
        public static Vector3 P0 = new Vector3(40, 40, 0);
        public static Vector3 P1 = new Vector3(400, 20, 0);
        public static Vector3 P2 = new Vector3(400, 570, 0);
        public static Vector3 P3 = new Vector3(40, 490, 0);

        public const int M = 11;
        public const int N = 9;
        
        public class Context { };

        public ChessboardPicked(GraphicsDevice device, ContentManager content)
            : base()
        {
            sb = new SpriteBatch(device);            
            white = new Texture2D(device, 1, 1);
            white.SetData(new Color[] { Color.White });

            effect = new BasicEffect(device);
            effect.VertexColorEnabled = true;
            effect.TextureEnabled = false;
            vb = new DynamicVertexBuffer(device, VertexPositionColor.VertexDeclaration, va.Length, BufferUsage.WriteOnly);
        }

        public void Render(object obj, Matrix view, Matrix projection)
        {
            var ctx = (Context)obj;

            var idx = 0;

            float sn0 = 0.0f;                
            for (int n = 1; n <= N; ++n)
            {
                float sn1 = (float)n / (float)N;

                var l0 = Vector3.Lerp(P0, P1, sn0);
                var l1 = Vector3.Lerp(P0, P1, sn1);
                var r0 = Vector3.Lerp(P3, P2, sn0);
                var r1 = Vector3.Lerp(P3, P2, sn1);

                float sm0 = 0.0f;
                for (int m = 1; m <= M; ++m)
                {
                    float sm1 = (float)m / (float)M;
                    var p00 = Vector3.Lerp(l0, r0, sm0);
                    var p01 = Vector3.Lerp(l0, r0, sm1);
                    var p10 = Vector3.Lerp(l1, r1, sm0);
                    var p11 = Vector3.Lerp(l1, r1, sm1);

                    var color = (((m ^ n) & 1)) != 0 ? Color.White : Color.Black;

                    va[idx++] = new VertexPositionColor(p00, color);
                    va[idx++] = new VertexPositionColor(p01, color);
                    va[idx++] = new VertexPositionColor(p11, color);
                    va[idx++] = new VertexPositionColor(p00, color);
                    va[idx++] = new VertexPositionColor(p11, color);
                    va[idx++] = new VertexPositionColor(p10, color);

                    sm0 = sm1;
                }

                sn0 = sn1;
            }

            effect.GraphicsDevice.SetVertexBuffer(null);
            vb.SetData(va);
            effect.Projection = effect.GraphicsDevice.Viewport.WorldViewProjectionForPixels2d();
            effect.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            effect.GraphicsDevice.SetVertexBuffer(vb);
            effect.CurrentTechnique.Passes[0].Apply();
            effect.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, M * N * 2);
        }

        private DynamicVertexBuffer vb;
        private VertexPositionColor[] va = new VertexPositionColor[6 * M * N];
        private SpriteBatch sb;
        private BasicEffect effect;
        private Texture2D white;
    }
}
