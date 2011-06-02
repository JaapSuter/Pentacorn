using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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
                DepthStencilFormat = DepthFormat.Depth24,
                PresentationInterval = PresentInterval.Default,
                MultiSampleCount = multiSampleCount,
                IsFullScreen = false, 
            };
            
            Device = new GraphicsDevice(Adapter, GraphicsProfile.HiDef, PresentParams);
            Loader = new ContentManager(new GraphicsDeviceForContentManager(Device), Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

            SpriteBatch = new SpriteBatch(Device);
            DebugFont = Loader.Load<SpriteFont>("Content/Fonts/Debug");

            WhiteTexel = new Texture2D(Device, 1, 1);
            WhiteTexel.SetData(new Color[] { Color.White });

            CoordinateSystemTexture2D = Loader.Load<Texture2D>("Content/Textures/UV");

            var ptw = 1280;
            var pth = 800;
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

            RenderTarget2D = new RenderTarget2D(Device, PresentParams.BackBufferWidth, PresentParams.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            RenderTarget2E = new RenderTarget2D(Device, PresentParams.BackBufferWidth, PresentParams.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            PostOps = new List<Action>();
        }

        public void Dispose()
        {
            Loader.Dispose();
            Device.Dispose();
        }
        
        private class GraphicsDeviceForContentManager : IServiceProvider, IGraphicsDeviceService
        {
            public GraphicsDeviceForContentManager(GraphicsDevice gd) { this.GraphicsDevice = gd; }
            public object GetService(Type serviceType) { return serviceType.IsAssignableFrom(this.GetType()) ? this : null; }
            public GraphicsDevice GraphicsDevice { get; set; }

            private void UnusedFunctionToAvoidPragmaToDisableWarning67()
            {
                var neverCalled = new EventHandler<EventArgs>((s, e) => { });
                
                DeviceCreated += neverCalled;
                DeviceDisposing += neverCalled;
                DeviceReset += neverCalled;
                DeviceResetting += neverCalled;
                
                DeviceCreated(null, null);
                DeviceDisposing(null, null);
                DeviceReset(null, null);
                DeviceResetting(null, null);
            }

            public event EventHandler<EventArgs> DeviceCreated;
            public event EventHandler<EventArgs> DeviceDisposing;
            public event EventHandler<EventArgs> DeviceReset;
            public event EventHandler<EventArgs> DeviceResetting;
        }
    }
}
