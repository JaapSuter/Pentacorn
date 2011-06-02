using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Rectangle = System.Drawing.Rectangle;

namespace Pentacorn.Graphics
{
    sealed class Frame
    {
        public Size Size { get; private set; }
        public Rectangle Rectangle { get { return Size.ToRect(); } }
        public Renderer Renderer { get; private set; }
        public Task PresentAsync() { Submit(this); return PresentComplete.Task; }
                
        public void Add(object t)
        {
            if (t == null)
                return;

            var e = t as IEnumerable;
            if (e == null)
                Objects.Add(t);
            else
                foreach (var o in e)
                    Add(o);
        }

        public void Add(params object[] ts)
        {
            foreach (var t in ts)
                Add(t);
        }

        public void Add(string s)
        {
            if (s != null)
                Objects.Add(s);
        }

        public void Add(IEnumerable ts)
        {
            if (ts != null)
                foreach (var t in ts)
                    Add(t);
        }

        public Frame(Renderer renderer, Size size, Action<Frame> submit)
        {
            Renderer = renderer;
            Submit = submit;
            Size = size;
        }

        public void Presented()
        {
            PresentComplete.TrySetResult(default(Unit));
            // Don't care if the try fails, because it implies
            // it has already been set earlier.
        }

        private Action<Frame> Submit { get; set; }
        private TaskCompletionSource<Unit> PresentComplete = new TaskCompletionSource<Unit>();
        internal List<object> Objects = new List<Object>();
    }
}

