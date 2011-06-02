using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Windows = System.Windows;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Pentacorn.Vision.Cameras;

namespace Pentacorn.Vision.Backup
{
    abstract class Window : Windows.Window
    {
        public static T Open<T>(bool isPrimary = true) where T : Window, new()
        {
            var exists = new TaskCompletionSource<T>();
            var thread = new Thread(_ =>
            {
                exists.SetResult(new T());
                Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.IsBackground = !isPrimary;
            return exists.Task.Result;
        }

        public IntPtr Hwnd { get { return hwnd; } }
        
        public Window()
        {
            this.Title = this.GetType().Namespace + "." + this.GetType().Name;

            hwnd = new WindowInteropHelper(this).EnsureHandle();
            keyEvents = Observable.FromEvent<KeyEventArgs>(this, "KeyUp").Select(e => e.EventArgs);
            mouseEvents = Observable.FromEvent<MouseEventArgs>(this, "MouseMove").Select(e => e.EventArgs);
        }

        public IObservable<MouseEventArgs> MouseEvents { get { return mouseEvents; } }

        public Task<Key> NextKeyUp()
        {
            var t = new TaskCompletionSource<Key>();
            keyEvents.Take(1).Subscribe(e => t.SetResult(e.Key));
            return t.Task;
        }

        public void WaitForClose()
        {
            if (thread.IsAlive)
                thread.Join();            
        }

        protected override void OnClosed(EventArgs e)
        {
            this.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
            base.OnClosed(e);
        }

        private IObservable<KeyEventArgs> keyEvents;
        private IObservable<MouseEventArgs> mouseEvents;
        private Thread thread = Thread.CurrentThread;
        private IntPtr hwnd;
    }
}
