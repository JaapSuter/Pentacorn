using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Vision.Graphics.Primitives
{
    class Chessboard2d : IRenderable
    {
        public Chessboard2d(GraphicsDevice device, ContentManager content)
            : base()
        {
            sb = new SpriteBatch(device);            
            white = new Texture2D(device, 1, 1);
            white.SetData(new Color[] { Color.White });

            effect = content.Load<Effect>("Content/Shaders/Chessboard2d");            
        }

        public void Render(object obj, Matrix view, Matrix projection)
        {
            var chk = (Checkers)obj;

            effect.Parameters["WorldViewProj"].SetValue(effect.GraphicsDevice.Viewport.WorldViewProjectionForPixels2d());
            effect.Parameters["Board"].SetValue(new int[] { chk.Board.Left, chk.Board.Top, chk.Board.Width, chk.Board.Height });
            effect.Parameters["Square"].SetValue(new int[] { chk.Square.Width, chk.Square.Height });
            effect.Parameters["WhiteColor"].SetValue(chk.WhiteColor.ToVector4());
            effect.Parameters["BlackColor"].SetValue(chk.BlackColor.ToVector4());
            effect.Parameters["OtherColor"].SetValue(chk.OtherColor.ToVector4());
            
            sb.Begin(default(SpriteSortMode), BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, effect, Matrix.Identity);
            sb.Draw(white, effect.GraphicsDevice.Viewport.Bounds, Color.Red);
            sb.End();
        }

        SpriteBatch sb;
        Effect effect;
        Texture2D white;
    }
}
