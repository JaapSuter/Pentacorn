using System;

namespace Pentacorn.Graphics
{
    abstract class Renderable<T> : IRenderable
    {
        public abstract void Render(RendererImpl rendererImpl, IViewProject viewProject, T t);

        public Type AbleToRender { get { return typeof(T); } }
        public void Render(RendererImpl rendererImpl, IViewProject viewProject, object obj)
        {
            Program.EnsureRendering();
            Render(rendererImpl, viewProject, (T)obj);
        }
    }
}
