using System;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework.Input;
using Pentacorn.Graphics;

namespace Pentacorn
{
    class Toggle : IDisposable
    {
        public Toggle(Action on, Action off, Keys keys)
            : this(on, off, Program.WhenInput, keys) { }

        public Toggle(Action on, Action off, IObservable<Input> trigger, Keys keys)
            : this(on, off, trigger.Where(input => input.KeyDown(keys))) { }

        public Toggle(Action on, Action off, Keys keys, Window window)
            : this(on, off, Program.WhenInput, keys, window) { }

        public Toggle(Action on, Action off, IObservable<Input> trigger, Keys keys, Window window)
            : this(on, off, trigger.Where(input => window.Focused && input.KeyDown(keys))) { }

        public Toggle(Action on, Action off, IObservable<Input> trigger)
        {
            Disposable = trigger.Subscribe(input =>
                {
                    var prev = Latch;
                    while (prev != Interlocked.CompareExchange(ref Latch, prev ^ 1, prev))
                        prev = Latch;

                    if (0 == prev)
                        on();
                    else
                        off();
                });
        }

        public void Dispose()
        {
            Util.Dispose(ref Disposable);
        }

        private int Latch = 0;
        private IDisposable Disposable;
    }
}
