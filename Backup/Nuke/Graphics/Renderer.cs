using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Vision.Graphics
{
    class Renderer
    {
        public Renderer(IntPtr hwnd)
        {
            width = GraphicsAdapter.Adapters.Max(a => a.CurrentDisplayMode.Width);
            height = GraphicsAdapter.Adapters.Max(a => a.CurrentDisplayMode.Height);

            var adapter = GraphicsAdapter.DefaultAdapter;

            parameters = new PresentationParameters()
            {
                DeviceWindowHandle = hwnd,
                BackBufferWidth = width,
                BackBufferHeight = height,
                BackBufferFormat = adapter.CurrentDisplayMode.Format,
                DepthStencilFormat = DepthFormat.Depth24,
                PresentationInterval = PresentInterval.Immediate,
                MultiSampleCount = 8,
                IsFullScreen = false,
            };

            device = new GraphicsDevice(adapter, GraphicsProfile.HiDef, parameters);
            device.DepthStencilState = DepthStencilState.Default;
            
            DebugShapeRenderer.Initialize(device);

            content = new ContentManager(new GraphicsDeviceForContentManager(device), Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

            spriteBatch = new SpriteBatch(device);
            spriteFont = content.Load<SpriteFont>("Content/Fonts/Util");

            primitives.Add(typeof(Primitives.Axes), new Primitives.Axes(device));
            primitives.Add(typeof(Primitives.Grid), new Primitives.Grid(device, content));
            primitives.Add(typeof(Primitives.Chessboard3d), new Primitives.Chessboard3d(device, content));
            primitives.Add(typeof(Primitives.Chessboard2d), new Primitives.Chessboard2d(device, content));
            primitives.Add(typeof(Primitives.ChessboardPicked), new Primitives.ChessboardPicked(device, content));            
            primitives.Add(typeof(Primitives.Snapshot), new Primitives.Snapshot(device));
            primitives.Add(typeof(Primitives.Circle2d), new Primitives.Circle2d(device, content));
            primitives.Add(typeof(Primitives.GrayCodeSweep), new Primitives.GrayCodeSweep(device));
            primitives.Add(typeof(Primitives.Clear), new Primitives.Clear(device));
            primitives.Add(typeof(DebugShapeRenderer), new DebugShapeRenderer());
        }

        public void Begin(int width, int height)
        {
            device.Viewport = new Viewport(0, 0, width, height);
        }
            
        public void End(IntPtr hwnd)
        {
            device.Present(device.Viewport.Bounds, null, hwnd);
        }

        public void Render<T>(object obj)
        {
            primitives[typeof(T)].Render(obj, Matrix.Identity, Matrix.Identity);
        }

        public void Render(string fmt, params object[] args)
        {
            var txt = String.Format(fmt, args);
            var position = new Vector2(10, 10);
            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, txt, position, Color.White);
            spriteBatch.End();
        }

        public void Render(Window window, Frame frame)
        {   
            foreach (var cmd in frame.Commands)
            {
                
            };

            // Statistics.Draw(spriteBatch, spriteFont);
        }

        private int width;
        private int height;
        private GraphicsDevice device;
        private ContentManager content;
        private PresentationParameters parameters;

        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        
        public IDictionary<Type, IRenderable> primitives = new Dictionary<Type, IRenderable>();

        private class GraphicsDeviceForContentManager : IServiceProvider, IGraphicsDeviceService
        {
            public GraphicsDeviceForContentManager(GraphicsDevice gd) { this.GraphicsDevice = gd; }
            public object GetService(Type serviceType) { return serviceType.IsAssignableFrom(this.GetType()) ? this : null; }
            public GraphicsDevice GraphicsDevice { get; set; }

#pragma warning disable 67
            public event EventHandler<EventArgs> DeviceCreated;
            public event EventHandler<EventArgs> DeviceDisposing;
            public event EventHandler<EventArgs> DeviceReset;
            public event EventHandler<EventArgs> DeviceResetting;
        }
    }
}
