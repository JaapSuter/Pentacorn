using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Pentacorn.Vision.Graphics;
using Primitives = Pentacorn.Vision.Graphics.Primitives;

namespace Pentacorn.Vision
{
    class Universe
    {
        public TimeSpan Age { get { return sw.Elapsed; } }
        public Input Input { get { return input; } }

        Universe()
        {
            thread = new Thread(() => Runner());
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
        }

        public void Start()
        {
            thread.Start();
        }

        public T Add<T>(T obj)
        {
            stuff.Add(obj);
            return obj;
        }

        private void See(Frame frame)
        {
            frame.Add<Primitives.Grid>(Matrix.Identity);
            frame.Add<Primitives.Axes>(Matrix.Identity);
            frame.Add<Primitives.Chessboard2d>(Matrix.Identity);
        }

        public void Close()
        {
            close = true;
            if (thread.IsAlive)
                thread.Join();
        }

        private void Runner()
        {
            while (!close)
            {
            }
        }

        public void Tick()
        {
            var curr = sw.Elapsed;
            var dt = curr - prev;
            prev = curr;

            foreach (var u in stuff.OfType<IUpdateable>())
                u.Update(dt);

            
            Statistics.Update("Universe.Tick");
        }
        
        private ConcurrentBag<Object> stuff = new ConcurrentBag<Object>();
        private Input input = new Input();
        private Stopwatch sw = Stopwatch.StartNew();
        private TimeSpan prev = TimeSpan.Zero;            
        private Thread thread;
        private volatile bool close;
    }
}
