using System;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework.Graphics;
using Capture = Pentacorn.Captures.Capture;
using Microsoft.Xna.Framework;
using System.IO;

namespace Pentacorn.Graphics
{
    class Renderer : IDisposable
    {
        public RendererImpl Impl { get { return RendererImpl; } }

        public Renderer()
        {
            var multiSampleCount = 0;
            var minWidth = 800;
            var minHeight = 1280;

            var hwnd = Winterop.CreateDummyWindowHandle();

            RendererImpl = new RendererImpl(hwnd, multiSampleCount, minWidth, minHeight);

            Renderables = new IRenderable[] {
                new RenderableClear(),
                new RenderableText(RendererImpl),
                new RenderableSphere(RendererImpl),
                new RenderableCircle2D(RendererImpl),
                new RenderableFrustum(RendererImpl),
                new RenderableQuad(RendererImpl),
                new RenderablePicture2d(RendererImpl),
                new RenderableGrid(RendererImpl),
                new RenderableChessboard(RendererImpl),                
                new RenderableCloud(),
                new RenderableMeshModel(RendererImpl),
                new RenderableLineSegment(RendererImpl),
                new RenderableGrayCodeSweep(RendererImpl),
            }.ToDictionary(r => r.AbleToRender);
        }

        public void Dispose()
        {
            RendererImpl.Dispose();
        }

        public void ToggleFullScreen()
        {
            // RendererImpl.Device.ToggleFullScreen
        }

        public void RenderInto(Scene scene, Window window)
        {
            var rect = window.ClientSize.LimitProportional(RendererImpl.PresentParams.BackBufferWidth,
                                                           RendererImpl.PresentParams.BackBufferHeight).ToXnaRect();
            if (rect.IsEmpty)
                return;

            var deviceResetError = RendererImpl.HandleDeviceReset(window.ClientSize);          

            RendererImpl.Device.Viewport = new Viewport(rect);

            scene.Render(this, scene.ViewProject);
        }

        public void RenderFinish(Scene scene, Window window)
        {
            var rect = window.ClientSize.LimitProportional(RendererImpl.PresentParams.BackBufferWidth,
                                                           RendererImpl.PresentParams.BackBufferHeight).ToXnaRect();
            if (rect.IsEmpty)
                return;

            RendererImpl.PostOps.Run(po => po());
            RendererImpl.PostOps.Clear();
            RendererImpl.Device.Textures[0] = null;

            try
            {
                RendererImpl.Device.Present(rect, null, window.Handle);
            }
            catch (DeviceLostException)
            {
                // Present might throw if the device became lost while we were
                // drawing. The lost device will be handled by the next BeginDraw,
                // so we just swallow the exception.
            }
        }

        public void RenderInto(Scene scene, Picture<Rgba, byte> picture)
        {
            var rect = picture.Size.ToXnaRect();
            if (rect.IsEmpty)
                return;

            RendererImpl.Device.SetRenderTarget(RendererImpl.RenderTarget2D);
            RendererImpl.Device.Viewport = new Viewport(rect);

            scene.Render(this, scene.ViewProject);
            RendererImpl.PostOps.Run(po => po());
            RendererImpl.PostOps.Clear();
            RendererImpl.Device.SetRenderTarget(null);
            RendererImpl.Device.SetVertexBuffer(null);
            RendererImpl.Device.Textures[0] = null;

            var bytesPerPixel = 4;
            var bytesPerPicture = rect.Width * rect.Height * bytesPerPixel;
            RendererImpl.RenderTarget2D.GetData(0, rect, picture.Bytes, 0, bytesPerPicture);
        }

        internal void Render(IViewProject viewProject, params IVisible[] visibles)
        {
            visibles.Run(v => Render(viewProject, v));
        }

        internal void Render(IViewProject viewProject, IEnumerable<IVisible> visibles)
        {
            visibles.Run(v => Render(viewProject, v));
        }

        internal void Render(IViewProject viewProject, IVisible visible)
        {
            IRenderable renderable;
            if (Renderables.TryGetValue(visible.GetType(), out renderable))
                renderable.Render(RendererImpl, viewProject, visible);
            else if (Renderables.TryGetValue(visible.GetType().BaseType, out renderable))
                renderable.Render(RendererImpl, viewProject, visible);
            else
                visible.Render(this, viewProject);
        }

        public Model LeaseModel(string name)
        {
            return RendererImpl.Loader.Load<Model>("Content/Models/" + name);
        }

        public Texture2D LeaseWhiteTexel()
        {
            return RendererImpl.WhiteTexel;
        }

        public Texture2D LeaseCoordinateSystem()
        {
            return RendererImpl.CoordinateSystemTexture2D;
        }

        public Texture2D LeaseFor(Capture capture, bool colorize = false)
        {
            if (colorize)
                return LeaseFor<Rgba, byte>(capture.Width, capture.Height);
            else
                return LeaseFor<Gray, byte>(capture.Width, capture.Height);
        }

        public Texture2D LeaseFor<TColor, TDepth>(Picture<TColor, TDepth> picture)
            where TColor : struct, IColor
            where TDepth : new()
        {
            return LeaseFor<TColor, TDepth>(picture.Width, picture.Height);
        }

        public Texture2D LeaseFor<TColor, TDepth>(int width, int height)
            where TColor : struct, IColor
            where TDepth : new()
        {
            var chan = default(TColor).Dimension;
            if (4 == chan)
                return new Texture2D(RendererImpl.Device, width, height, false, SurfaceFormat.Color);
            else if (1 == chan)
                return new Texture2D(RendererImpl.Device, width, height, false, SurfaceFormat.Alpha8);
            else
                throw new Exception("Unsupported channel count");
        }

        private RendererImpl RendererImpl;
        private IDictionary<Type, IRenderable> Renderables;
    }
}
