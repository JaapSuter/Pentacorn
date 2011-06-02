using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;

namespace Pentacorn.Graphics
{
    [System.ComponentModel.DesignerCategory("")]
    class Window : Form
    {
        public Scene Scene { get; set; }

        public new IObservable<KeyEventArgs> KeyUp { get; private set; }
        public new IObservable<KeyEventArgs> KeyDown { get; private set; }
        
        public IObservable<Unit> WhenClosing { get; private set; }

        public Window(string title)
        {
            Text = title;

            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.UserMouse, true);
            
            KeyDown = Observable.FromEvent<KeyEventArgs>(this, "KeyDown").Select(e => e.EventArgs);
            KeyUp = Observable.FromEvent<KeyEventArgs>(this, "KeyUp").Select(e => e.EventArgs);
            
            WhenClosing = Observable.FromEvent<FormClosingEventArgs>(this, "FormClosing").Select(e => default(Unit));

            KeyDown.Where(e => e.KeyCode == Keys.F).Subscribe(e => FullScreen = !FullScreen);
            
            Cursor = Cursors.Cross;
            StartPosition = FormStartPosition.Manual;
            Secondary = 0 < Application.OpenForms.Count;
            WindowRectangleBeforeFullscreen = ClientRectangle;

            Program.Register(this);            
        }

        public Screen LocatedOnScreen
        {
            get { return Screen.FromControl(this); }
            set { Location = Point.Round((value.WorkingArea.Location.ToVector2() + Location.ToVector2()).ToPointF()); }
        }

        public bool FullScreen
        {
            get { return WindowState == FormWindowState.Maximized && FormBorderStyle == FormBorderStyle.None; }                
            set
            {
                if (value == FullScreen)
                    return;

                if (value)
                {
                    WindowRectangleBeforeFullscreen = DesktopBounds;
                    FormBorderStyle = FormBorderStyle.None;
                    WindowState = FormWindowState.Maximized;
                    DesktopBounds = LocatedOnScreen.Bounds;
                }
                else
                {
                    DesktopBounds = WindowRectangleBeforeFullscreen;
                    FormBorderStyle = FormBorderStyle.Sizable;
                    WindowState = FormWindowState.Normal;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Scene == null)
                e.Graphics.Clear(Color.YellowGreen);
            else
                Program.Renderer.RenderInto(Scene, this);
        }

        protected override bool ShowWithoutActivation { get { return Secondary; } }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Do nothing, stops base class from painting the background, which would cause flickering.
        }

        protected override void OnActivated(EventArgs e)
        {
            Pentacorn.Input.SetFocusOn(Handle);
            base.OnActivated(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Program.OnClosing();
            base.OnClosing(e);
        }

        private bool Secondary;
        private Rectangle WindowRectangleBeforeFullscreen;
    }
}
