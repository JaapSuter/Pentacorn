using System;
using System.Collections.Generic;
using System.Concurrency;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pentacorn.Captures;
using Pentacorn.Graphics;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

using GdiRectangleF = System.Drawing.RectangleF;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;
using GdiGraphics = System.Drawing.Graphics;
using System.Runtime.InteropServices;
using Emgu.CV.Structure;
using System.Disposables;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace Pentacorn
{
    static class Program
    {
        public static int Run(string name, Func<Task> work)
        {
            Renderer = new Renderer();
            using (Disposable.Create(() => Renderer.Dispose()))
            {
                Monitor = new Window(name);
                Monitor.ClientSize = new Size(1280, 800);
                Monitor.Show();
                WhenInput = Pentacorn.Input.InitializeGlobal(Monitor.Handle);

                var cam = new ScreenCamera(Monitor);
                var clear = new Clear(Color.White);
                var text = new Text("", new Vector2(10, 10), Color.HotPink);
                var scene = new Scene(cam) { clear, text };

                var sw = Stopwatch.StartNew();
                int framePerSecondCount = 0;
                long secondCount = 0;
                long frameElapsedTicksPrev = 0;
                int fps = 0;
                float mspf = 0.0f;
                var swFreq = Stopwatch.Frequency;
                var msPerTick = 1000.0 / (double)Stopwatch.Frequency;
                var rmma = new RunningMinMaxAverage(numSamples: 120);
                var exitCode = 0;

                FrameworkDispatcher.Update();

                for (var frameCount = 0; secondCount < 80; ++frameCount)
                {
                    Winterop.Message message;
                    if (Winterop.PeekMessage(out message, IntPtr.Zero, 0, 0, Winterop.PM_REMOVE | Winterop.PM_NOYIELD))
                    {
                        if (message.Msg == Winterop.WM_QUIT)
                        {
                            exitCode = message.WParam.ToInt32();
                            Winterop.PostQuitMessage(exitCode);
                            break;
                        }
                        else
                        {
                            Winterop.TranslateMessage(ref message);
                            Winterop.DispatchMessage(ref message);
                        }
                    }
                    else
                    {
                        ++framePerSecondCount;
                        long frameElapsedTicksCurr = sw.ElapsedTicks;
                        long frameElapsedTicks = frameElapsedTicksCurr - frameElapsedTicksPrev;
                        frameElapsedTicksPrev = frameElapsedTicksCurr;

                        rmma.Add(frameElapsedTicks);

                        var framePerSecondCurrent = frameElapsedTicksCurr / swFreq; // Intentionally using an integer divide, because we want floor semantics.
                        if (framePerSecondCurrent > secondCount)
                        {
                            fps = framePerSecondCount;
                            mspf = 1000.0f / fps;

                            secondCount = framePerSecondCurrent;
                            framePerSecondCount = 0;
                        }
                        
                        var sec = (float)sw.Elapsed.TotalSeconds;
                                               
                        text.String = String.Format("{0:0.00} fps", fps);

                        Pentacorn.Input.UpdateGlobal();
                        Renderer.RenderInto(scene, Monitor);

                        Renderer.RenderFinish(scene, Monitor);
                        if (!Monitor.Visible)
                        {
                            Monitor.Show();
                            Monitor.FullScreen = true;
                        }
                        
                        FrameworkDispatcher.Update();
                    }
                }

                Winterop.PostQuitMessage(exitCode);
                return exitCode;
            }
        }

        public static IObservable<Input> WhenInput { get; private set; }

        public static Window Monitor { get; private set; }
        public static Renderer Renderer { get; private set; }

        public static IEnumerable<Capture> Captures { get; private set; }

        public static IObservable<TimeSpan> WhenTick { get { return Ticker.AsObservable(); } }

        public static TimeSpan DT { get { return PreviousFrameDelta; } }
        public static int FrameCount { get; private set; }

        internal static void Register(Window window)
        {
            Debug.Assert(!Windows.Contains(window));
            Windows.Add(window);
        }

        private static Task Begin(Func<Task> work)
        {
            // Debug.Assert(SynchronizationContext.Current is WindowsFormsSynchronizationContext);
            // RenderContext = new ControlScheduler(Monitor);
            // ComputeContext = Scheduler.TaskPool;

            Renderer = new Renderer();

            WhenInput = Pentacorn.Input.InitializeGlobal(Monitor.Handle);

            Captures = Enumerable.Concat(DirectShowCapture.Devices, CLEyeCapture.Devices).ToList();

            Stopwatch = Stopwatch.StartNew();

            Application.Idle += OnIdle;

            return work();
        }

        public static void OnClosing()
        {
            // End(null);
            Winterop.PostQuitMessage(0);
        }

        public static Task Tick()
        {
            EnsureRendering();
            var tcs = new TaskCompletionSource<Unit>();
            AwaitingTick.Add(tcs);
            return tcs.Task;
        }

        private static void OnIdle(object s, EventArgs e)
        {
            EnsureRendering();

            while (Winterop.NoPendingWindowMessages())
            {
                var currentFrameTicks = Stopwatch.ElapsedTicks;
                double deltaFrameTicks = currentFrameTicks - PreviousFrameTicks;
                PreviousFrameDelta = TimeSpan.FromSeconds(deltaFrameTicks / Stopwatch.Frequency);
                PreviousFrameTicks = currentFrameTicks;

                Pentacorn.Input.UpdateGlobal();
                foreach (var w in Windows)
                    w.Invalidate();

                ++FrameCount;

                Ticker.OnNext(PreviousFrameDelta);

                // Todo, this has a race condition if people were
                // allowed to await ticks on a different thread than ours.
                var toSignal = AwaitingTick.ToArray();
                AwaitingTick.Clear();
                foreach (var t in toSignal)
                    t.SetResult(default(Unit));
            }
        }

        private static async void End(Task work)
        {
            await SwitchToRender();

            foreach (var capture in Captures)
                capture.Close();

            foreach (var window in Windows)
                window.Close();
        }

        public static ITask SwitchToRender() { return RenderContext.SwitchTo(); }
        public static ITask SwitchToCompute() { return ComputeContext.SwitchTo(); }

        public static string CurrentContext { get { return !Monitor.InvokeRequired ? "RenderContext" : "ComputeContext"; } }

        public static void EnsureComputing() { Debug.Assert(Monitor.InvokeRequired, "Not within expected computing threading context."); }
        public static void EnsureRendering() { Debug.Assert(!Monitor.InvokeRequired, "Not within expected computing rendering context."); }

        public static IScheduler RenderContext { get; private set; }
        public static IScheduler ComputeContext { get; private set; }

        private static ISet<Window> Windows = new HashSet<Window>();

        private static List<TaskCompletionSource<Unit>> AwaitingTick = new List<TaskCompletionSource<Unit>>();
        private static long PreviousFrameTicks;
        private static TimeSpan PreviousFrameDelta;
        private static Stopwatch Stopwatch;
        private static Subject<TimeSpan> Ticker = new Subject<TimeSpan>();
    }    
}
