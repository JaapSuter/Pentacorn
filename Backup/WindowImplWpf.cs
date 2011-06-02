using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Screen = System.Windows.Forms.Screen;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Size = System.Drawing.Size;

namespace Pentacorn.Graphics
{
    class WindowImpl : System.Windows.Window
    {
        public IObservable<KeyEventArgs> KeyUpEvents { get; private set; }
        public IObservable<KeyEventArgs> KeyDownEvents { get; private set; }
        public IObservable<Vector2> MouseMoveEvents { get; private set; }
        public IObservable<Vector2> MouseDownEvents { get; private set; }
        public IObservable<Vector2> MouseUpEvents { get; private set; }
        
        public IntPtr Hwnd { get; private set; }

        public Size ClientSize { get { return new Size((int)Grid.ActualWidth, (int)Grid.ActualHeight); } }
        
        public Screen Fullscreen
        { 
            get { return MaybeFullscreen; }
            set
            {
                MaybeFullscreen = value;

                if (MaybeFullscreen == null)
                {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = WindowState.Normal;
                    Width = Screen.PrimaryScreen.Bounds.Height * 0.9;
                    Height = Screen.PrimaryScreen.Bounds.Width * 0.9;
                    Cursor = Cursors.Arrow;
                }
                else
                {
                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;
                    Height = MaybeFullscreen.Bounds.Height;
                    Width = MaybeFullscreen.Bounds.Width;
                    Left = MaybeFullscreen.Bounds.Left;
                    Top = MaybeFullscreen.Bounds.Top;

                    Cursor = Cursors.Cross;

                    Windows.RemoveBorder(Hwnd, MaybeFullscreen.Bounds);
                }                
            }
        }
        
        public WindowImpl(string title)
        {
            Background = Brushes.HotPink;
            UseLayoutRounding = true;
            Title = title;            

            Grid = new Grid()
            {
                UseLayoutRounding = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            
            this.AddChild(Grid);

            KeyDownEvents = Observable.FromEvent<KeyEventArgs>(this, "KeyDown").Select(e => e.EventArgs);
            KeyUpEvents = Observable.FromEvent<KeyEventArgs>(this, "KeyUp").Select(e => e.EventArgs);
            MouseMoveEvents = Observable.FromEvent<MouseEventArgs>(this, "MouseMove").Select(e => e.EventArgs.GetPosition(Grid).ToXna());
            MouseDownEvents = Observable.FromEvent<MouseButtonEventArgs>(this, "MouseDown").Select(e => e.EventArgs.GetPosition(Grid).ToXna());
            MouseUpEvents = Observable.FromEvent<MouseButtonEventArgs>(this, "MouseUp").Select(e => e.EventArgs.GetPosition(Grid).ToXna());

            KeyDownEvents.Where(e => e.Key == Key.F).Subscribe(ie => this.Fullscreen = this.Fullscreen == null ? Screen.PrimaryScreen : null);
            
            MaybeFullscreen = null;
            Hwnd = new WindowInteropHelper(this).EnsureHandle();            
            Show();
        }

        private Screen MaybeFullscreen;
        private Grid Grid;
    }
}
