using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Microsoft.Xna.Framework.Input;
using Pentacorn;

namespace Pentacorn.Graphics
{
    [System.ComponentModel.DesignerCategory("")] 
    class WindowImpl : Form
    {
        public WindowImpl(string title, Renderer renderer, Screen fullScreen)
            : this(title, renderer)
        {
            if (fullScreen == null)
                return;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            WindowState = fullScreen == null ? FormWindowState.Normal : FormWindowState.Maximized;            
            Bounds = fullScreen.Bounds;

            AddMostRecentState();
            CreateControl();
        }

        public WindowImpl(string title, Renderer renderer)
        {
            Renderer = renderer;
            Text = title;
            BackColor = System.Drawing.Color.HotPink;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;
            ClientSize = Screen.PrimaryScreen.Bounds.Size.Scale(0.5);
        }

        public Frame Begin()
        {
            if (this.IsDisposed)
                return null;

            return null; // new Frame(this, StatePipe.Take());
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Prep = new State(Prep.Bounds, Microsoft.Xna.Framework.Input.Mouse.GetState(), Microsoft.Xna.Framework.Input.Keyboard.GetState());
            
            base.OnMouseMove(e);
        }

        public void Submit(Frame frame)
        {
            FramePipe.Add(frame);
        }

        public void Process()
        {
            if (FramePipe.Count > 0)
                Invalidate();
        }
                
        protected override void OnPaint(PaintEventArgs a)
        {
            var frame = FramePipe.Take();

            if (frame.State.Bounds.Width == 0 || frame.State.Bounds.Height == 0)
                return;

            try { Renderer.Render(frame.State, frame, Handle); }
            catch (Exception e)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                Renderer.SetExceptionWhileRendering(e);
                throw;
            }

            AddMostRecentState();
        }

        private void AddMostRecentState()
        {
            if (StatePipe.IsEmpty())
                StatePipe.Add(new State(Bounds.ToXna(), Mouse.GetState(), Keyboard.GetState()));
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Do nothing, ensures GDI doesn't paint the form background.
        }

        private Renderer Renderer;
        private BlockingCollection<State> StatePipe = new BlockingCollection<State>(new ConcurrentQueue<State>(), 2);
        private BlockingCollection<Frame> FramePipe = new BlockingCollection<Frame>(new ConcurrentQueue<Frame>(), 1);
        private AutoResetEvent MorePlease = new AutoResetEvent(true);
    }
}
