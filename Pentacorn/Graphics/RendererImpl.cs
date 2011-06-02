using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using GdiSize = System.Drawing.Size;
using GdiGraphics = System.Drawing.Graphics;
using GdiColor = System.Drawing.Color;
using GdiSolidBrush= System.Drawing.SolidBrush;
using GdiRectangle = System.Drawing.Rectangle;
using GdiFont = System.Drawing.Font;
using GdiStringAlignment = System.Drawing.StringAlignment;
using GdiStringFormat = System.Drawing.StringFormat;
using GdiSystemFonts = System.Drawing.SystemFonts;

namespace Pentacorn.Graphics
{
    class RendererImpl : IDisposable
    {
        public GraphicsAdapter Adapter { get; private set; }
        public GraphicsDevice Device { get; private set; }
        public PresentationParameters PresentParams { get; private set; }
        public List<Action> PostOps { get; private set; }

        public ContentManager Loader { get; private set; }

        public Texture2D HorizontalStripes { get; private set; }
        public Texture2D WhiteTexel { get; private set; }
        public Texture2D CoordinateSystemTexture2D { get; private set; }
        public RenderTarget2D RenderTarget2D { get; private set; }
        public RenderTarget2D RenderTarget2E { get; private set; }
        public RenderTarget2D RenderTarget2Z { get; private set; }
        public RenderTarget2D RenderTarget2W { get; private set; }

        public SpriteBatch SpriteBatch { get; private set; }
        public SpriteFont DebugFont { get; private set; }

        public RendererImpl(IntPtr hwnd, int multiSampleCount, int minWidth, int minHeight)
        {
            Adapter = GraphicsAdapter.DefaultAdapter;

            PresentParams = new PresentationParameters()
            {
                DeviceWindowHandle = hwnd,
                BackBufferWidth = Math.Max(minWidth, GraphicsAdapter.Adapters.Max(a => a.CurrentDisplayMode.Width)),
                BackBufferHeight = Math.Max(minHeight, GraphicsAdapter.Adapters.Max(a => a.CurrentDisplayMode.Height)),
                BackBufferFormat = Adapter.CurrentDisplayMode.Format,
                DepthStencilFormat = DepthFormat.Depth24Stencil8,                
                PresentationInterval = PresentInterval.Immediate,
                RenderTargetUsage = RenderTargetUsage.PreserveContents,
                MultiSampleCount = multiSampleCount,
                IsFullScreen = false,
            };

            Device = new GraphicsDevice(Adapter, GraphicsProfile.HiDef, PresentParams);

            DeviceService = new GraphicsDeviceService(Device);
            Loader = new ContentManager(DeviceService, Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

            SpriteBatch = new SpriteBatch(Device);
            DebugFont = Loader.Load<SpriteFont>("Content/Fonts/Debug");

            WhiteTexel = new Texture2D(Device, 1, 1);
            WhiteTexel.SetData(new Color[] { Color.White });

            if (Global.No)
            {
                CoordinateSystemTexture2D = Loader.Load<Texture2D>("Content/Textures/UV");

                var ptw = 800;
                var pth = 600;
                var ptd = from y in Enumerable.Range(0, pth)
                          from x in Enumerable.Range(0, ptw)
                          let yb = y == 0 || y == (pth - 1)
                          let xb = x == 0 || x == (ptw - 1)
                          let bw = ((x / 8) % 2) == 0 ? Color.Black : Color.White
                          select (xb || yb)
                               ? Color.Black.Alpha(0.1f)
                               : bw;

                HorizontalStripes = new Texture2D(Device, ptw, pth, false, SurfaceFormat.Color);
                HorizontalStripes.SetData(ptd.ToArray());

                RenderTarget2D = new RenderTarget2D(Device, PresentParams.BackBufferWidth, PresentParams.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            }

            PostOps = new List<Action>();
        }

        public void Dispose()
        {
            Loader.Dispose();
            DeviceService.Disposing();
            Device.Dispose();
        }

        public string HandleDeviceReset(GdiSize size)
        {
            var deviceNeedsReset = false;

            switch (Device.GraphicsDeviceStatus)
            {
                case GraphicsDeviceStatus.Normal:
                    var pp = Device.PresentationParameters;

                    deviceNeedsReset = (size.Width > pp.BackBufferWidth) ||
                                       (size.Height > pp.BackBufferHeight);
                    break;

                case GraphicsDeviceStatus.NotReset:
                    deviceNeedsReset = true;
                    break;

                case GraphicsDeviceStatus.Lost:
                default:
                    return Device.GraphicsDeviceStatus.ToString();
            }

            if (deviceNeedsReset)
                try
                {
                    ResetDevice(size.Width, size.Height);
                }
                catch (Exception e)
                {
                    return "Graphics device reset failed\n\n" + e;
                }

            return String.Empty;
        }

        private void ResetDevice(int width, int height)
        {
            DeviceService.Resetting();

            PresentParams.BackBufferWidth = Math.Max(PresentParams.BackBufferWidth, width);
            PresentParams.BackBufferHeight = Math.Max(PresentParams.BackBufferHeight, height);

            Device.Reset(PresentParams);

            DeviceService.Reset();
        }

        public static void PaintUsingSystemDrawing(GdiGraphics graphics, string text, GdiRectangle clientRectangle)
        {
            graphics.Clear(GdiColor.CornflowerBlue);

            using (var brush = new GdiSolidBrush(GdiColor.Black))
            using (var format = new GdiStringFormat())
            {
                format.Alignment = GdiStringAlignment.Center;
                format.LineAlignment = GdiStringAlignment.Center;

                graphics.DrawString(text, GdiSystemFonts.MessageBoxFont, brush, clientRectangle, format);
            }
        }

        private class GraphicsDeviceService : IServiceProvider, IGraphicsDeviceService
        {
            public GraphicsDeviceService(GraphicsDevice gd) { this.GraphicsDevice = gd; }
            public object GetService(Type serviceType) { return serviceType.IsAssignableFrom(this.GetType()) ? this : null; }
            public GraphicsDevice GraphicsDevice { get; set; }

            public void Created() { if (DeviceCreated != null) DeviceCreated(this, EventArgs.Empty); }
            public void Reset() { if (DeviceReset != null) DeviceReset(this, EventArgs.Empty); }
            public void Disposing() { if (DeviceDisposing != null) DeviceDisposing(this, EventArgs.Empty); }
            public void Resetting() { if (DeviceResetting != null) DeviceResetting(this, EventArgs.Empty); }

            public event EventHandler<EventArgs> DeviceCreated;
            public event EventHandler<EventArgs> DeviceDisposing;
            public event EventHandler<EventArgs> DeviceReset;
            public event EventHandler<EventArgs> DeviceResetting;
        }

        private GraphicsDeviceService DeviceService;
    }
}
