using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Pentacorn.Vision.Graphics.Primitives;

namespace Pentacorn.Vision
{
    class Program
    {
        private static void Main(string[] args)
        {
            Interop.SetLibraryPaths(Global.LibDir);
            ThreadPool.SetMaxThreads(5, 5);
            Run();
        }

        private static void Run()
        {
            var m = new Window("Pentacorn.Vision.Primary", Screen.PrimaryScreen, maximize: false);
            var p = new Window("Pentacorn.Vision.Secondary", Screen.AllScreens.Where(s => s != Screen.PrimaryScreen).First());

            var c0 = p.MouseClickEvents;
            var c1 = p.MouseClickEvents.Skip(1);
            var c2 = p.MouseClickEvents.Skip(2);
            var c3 = p.MouseClickEvents.Skip(3);
            var clicks = Observable.Zip(Observable.Zip(c0, c1, (l, r) => Tuple.Create(l, r)),
                                        Observable.Zip(c2, c3, (l, r) => Tuple.Create(l, r)),
                                        (l, r) => Tuple.Create(l.Item1, l.Item2, r.Item1, r.Item2));

            p.MouseMoveEvents.Where(e => p.Bounds.Contains(new System.Drawing.Point(e.X, e.Y)));

            clicks.Subscribe((quad) =>
            {
                ChessboardPicked.P0 = new Vector3(quad.Item1.X, quad.Item1.Y, 0);
                ChessboardPicked.P1 = new Vector3(quad.Item2.X, quad.Item2.Y, 0);
                ChessboardPicked.P2 = new Vector3(quad.Item3.X, quad.Item3.Y, 0);
                ChessboardPicked.P3 = new Vector3(quad.Item4.X, quad.Item4.Y, 0);
            });
        }
    }
}
