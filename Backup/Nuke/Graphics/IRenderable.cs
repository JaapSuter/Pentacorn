using Microsoft.Xna.Framework;

namespace Pentacorn.Vision.Graphics
{
    interface IRenderable
    {
        void Render(object obj, Matrix view, Matrix projection);
    }
}
