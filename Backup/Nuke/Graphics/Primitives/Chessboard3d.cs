using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Vision.Graphics.Primitives
{
    class Chessboard3d : PrimitiveBase<VertexPosition>
    {
        public Chessboard3d(GraphicsDevice device, ContentManager content)
            : base()
        {
            AddIndex(CurrentVertex + 0);
            AddIndex(CurrentVertex + 1);
            AddIndex(CurrentVertex + 2);

            AddIndex(CurrentVertex + 0);
            AddIndex(CurrentVertex + 2);
            AddIndex(CurrentVertex + 3);

            var r = Projector.ScreenWidth / 2;
            var l = -r;
            var t = Projector.ScreenHeight / 2;
            var b = -t;

            AddVertex(new VertexPosition(Skreen.Left, Skreen.Bottom, 0));
            AddVertex(new VertexPosition(Skreen.Left, Skreen.Top, 0));
            AddVertex(new VertexPosition(Skreen.Right, Skreen.Top, 0));
            AddVertex(new VertexPosition(Skreen.Right, Skreen.Bottom, 0));

            FinishConstruction(device, content.Load<Effect>("Content/Shaders/Chessboard3d"));

            this.Effect.Parameters["WhiteColor"].SetValue(Color.White.ToVector4());
            this.Effect.Parameters["BlackColor"].SetValue(Color.Black.ToVector4());
            this.Effect.Parameters["Origin"].SetValue(Projector.Origin);
            this.Effect.Parameters["Size"].SetValue(Projector.Size);
            this.Effect.Parameters["Dim"].SetValue(new Vector2(Projector.M, Projector.N));
        }
    }
}
