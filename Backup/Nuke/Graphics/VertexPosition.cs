using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Vision.Graphics
{
    public struct VertexPosition : IVertexType
    {
        public Vector3 Position;

        public VertexPosition(float x, float y, float z)
        {
            Position = new Vector3(x, y, z);
        }

        public VertexPosition(Vector3 position)
        {
            Position = position;
        }

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0));

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexPosition.VertexDeclaration; }
        }
    }
}
