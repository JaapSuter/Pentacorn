using System;
using System.Collections.Generic;
using System.Disposables;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Pentacorn.Captures;
using Pentacorn.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using PointF = System.Drawing.PointF;
using Size = System.Drawing.Size;
using Rectangle = System.Drawing.Rectangle;
using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using System.Threading;

namespace Pentacorn.Tasks
{
    abstract class WorkFlowTask
    {
        public Window Window { get; private set; }

        public WorkFlowTask(WorkFlowTask parent)
        {
            Window = parent.Window;
        }

        public WorkFlowTask(Window window)
        {
            Window = window;            
        }

        private async void Add(FadingTextField ftf)
        {
            await Program.SwitchToRender();
            await TaskEx.Delay(Util.Random.Next(10, 300));
            Window.Scene.Add(ftf);
        }

        protected void WriteLine(string fmt, params object[] args)
        {
            WriteLine(fmt.FormatWith(args));
        }

        protected void WriteLine(string s)
        {
            Console.WriteLine(s);

            var velocity = Util.Random.NextFloat(90, 120) * Vector2.Normalize(new Vector2(Util.Random.NextFloat(-0.6f, 0.6f), Util.Random.NextFloat(-0.4f, -2.3f)));
            var position = Window.ClientSize.CenterF().ToVector2();

            var last = Window.Scene.Where(v => v is FadingTextField).LastOrDefault() as FadingTextField;
            
            position.Y = last == null
                       ? position.Y
                       : Math.Max(position.Y, last.Position.Y + 100);

            Add(new FadingTextField(s, position, velocity, Window.ClientSize.Scale(1.4f).ToRect(-30, -30, AnchorPoints.TopLeft),
                async ftf =>
                {
                    await Program.SwitchToRender();
                    Window.Scene.Remove(ftf);
                }));
        }

        private class FadingTextField : IVisible
        {
            public Vector3 Position { get { return Text.Position; } }

            public FadingTextField(string s, Vector2 position, Vector2 velocity, Rectangle rect, Func<FadingTextField, Task> remove)
            {
                Text = new Text(s, position, Color.White);
                Velocity = new Vector3(velocity, 0);

                var every = TimeSpan.FromSeconds(1 / 30.0);
                Unsubscribe = Observable.Timer(TimeSpan.Zero, every)
                                        .TimeInterval()
                                        .Subscribe(t =>
                                        {
                                            Text.Position += Velocity * (float)t.Interval.TotalSeconds;
                                            if (!rect.Contains((int)Text.Position.X, (int)Text.Position.Y))
                                            {
                                                remove(this);
                                                if (Unsubscribe != null)
                                                    Unsubscribe.Dispose();
                                            }
                                        });
            }

            private Text Text;
            private Vector3 Velocity;
            private IDisposable Unsubscribe;
            
            public void Render(Renderer renderer, IViewProject viewProject)
            {
                renderer.Render(viewProject, Text);
            }
        }
    }
}
