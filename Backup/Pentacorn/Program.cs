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

namespace Pentacorn
{
    static class Program
    {
        public static void Run(string name, Func<Task> work)
        {
            Debug.Assert(Windows.IsEmpty());

            Monitor = new Window(name);
            Monitor.Load += (s, e) => Begin(work).ContinueWith(End);

            Application.Run(Monitor);
        }

        public static IObservable<Input> WhenInput { get; private set; }

        public static Window Monitor { get; private set; }
        public static Renderer Renderer { get; private set; }
        
        public static IEnumerable<Capture> Captures { get; private set; }

        public static IObservable<TimeSpan> WhenTick { get { return Ticker.AsObservable(); } }

        public static TimeSpan DT { get { return PreviousFrameDelta; } }

        internal static void Register(Window window)
        {
            Debug.Assert(!Windows.Contains(window));
            Windows.Add(window);            
        }

        private static Task Begin(Func<Task> work)
        {
            Debug.Assert(SynchronizationContext.Current is WindowsFormsSynchronizationContext);

            RenderContext = new ControlScheduler(Monitor);
            ComputeContext = Scheduler.TaskPool;            
            
            Renderer = new Renderer(Monitor.Handle);

            WhenInput = Pentacorn.Input.InitializeGlobal(Monitor.Handle, RenderContext);
            
            Captures = Enumerable.Concat(DirectShowCapture.Devices, CLEyeCapture.Devices).ToList();

            Stopwatch = Stopwatch.StartNew();
            
            Application.Idle += OnIdle;
            
            return work();
        }

        public static void OnClosing()
        {
            End(null);
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
        private static int FrameCount;
        private static long PreviousFrameTicks;
        private static TimeSpan PreviousFrameDelta;
        private static Stopwatch Stopwatch;
        private static Subject<TimeSpan> Ticker = new Subject<TimeSpan>();
    }
}
