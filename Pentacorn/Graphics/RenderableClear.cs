
using Microsoft.Xna.Framework.Graphics;
namespace Pentacorn.Graphics
{
    class RenderableClear : Renderable<Clear>
    {
        public override void Render(RendererImpl rendererImpl, IViewProject viewProject, Clear clear)
        {
            rendererImpl.Device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, clear.Color, 1.0f, 0);
        }
    }
}
