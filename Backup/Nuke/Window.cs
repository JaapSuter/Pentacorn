using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pentacorn.Vision.Graphics;
using Pentacorn.Vision.Graphics.Primitives;

namespace Pentacorn.Vision
{
    sealed class Window
    {
        public bool IsDisposed { get { return Form.IsDisposed; } }
        public Size Size { get { return Form.ClientSize; } }
        public Rectangle Bounds { get { return Form.ClientRectangle; } }
        public IObservable<MouseEventArgs> MouseClickEvents { get; private set; }
        public IObservable<MouseEventArgs> MouseMoveEvents { get; private set; }

        public static Renderer UnsafeRenderer { get { return Renderer; } }

        public Window(string title, Screen screen, bool maximize = true)
        {
            var ctored = new TaskCompletionSource<Form>();
            var thread = new Thread(_ =>
            {
                Form = new FormImpl(title, screen, maximize);

                MouseClickEvents = Observable.FromEvent<MouseEventArgs>(Form, "MouseClick").Select(e => e.EventArgs);
                MouseMoveEvents = Observable.FromEvent<MouseEventArgs>(Form, "MouseMove").Select(e => e.EventArgs);

                if (Renderer == null)
                    lock (Sync)
                        if (Renderer == null)
                            Renderer = new Renderer(Form.Handle);

                ctored.SetResult(Form);
                Application.Run(Form);
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = title;
            thread.IsBackground = false;
            thread.Start();
            
            ctored.Task.Wait();
        }

        [System.ComponentModel.DesignerCategory("")]
        private class FormImpl : Form
        {
            public FormImpl(string title, Screen screen, bool maximize)
            {
                Text = title;
                Bounds = screen.Bounds;                
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.Manual;

                if (!maximize)
                {
                    Width /= 2; Height /= 2;
                    Left = Width / 2; Top = Width / 2;
                    WindowState = FormWindowState.Normal;
                    FormBorderStyle = FormBorderStyle.Fixed3D;
                }

                Application.Idle += delegate { Invalidate(); };
            }

            private static Captures.Capture cap = Captures.DirectShowCapture.Devices.First();
            private Picture pic = new Picture(cap.Width, cap.Height);

            protected override void OnPaint(PaintEventArgs e)
            {
                lock (Window.Renderer)
                {
                    Window.Renderer.Begin(Width, Height);
                    Window.Renderer.Render<Graphics.Primitives.Clear>(Microsoft.Xna.Framework.Color.DarkGray);
                    if (this.Width == 800)
                        Window.Renderer.Render<Graphics.Primitives.ChessboardPicked>(new ChessboardPicked.Context() { });
                    else
                    {
                        var poc = cap.TryDequeue();
                        if (poc != null)
                        {
                            if (pic != null)
                                pic.Release();
                            pic = poc;
                        }

                        Window.UnsafeRenderer.Render<Snapshot>(new Snapshot.Context() { Picture = pic, Rect = new Microsoft.Xna.Framework.Rectangle(10, 10, cap.Width / 3, cap.Height / 3), });
                    }
                    Window.Renderer.End(Handle);
                }
                    
                base.OnPaint(e);
            }
            
            protected override void OnPaintBackground(PaintEventArgs e)
            {
                // Avoid default in base class being called.
            }
        }

        static Window()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }

        private static object Sync = new object();
        private static Renderer Renderer;
        
        private Form Form;        
    }
}
