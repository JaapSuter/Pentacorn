using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Pentacorn.Graphics;

namespace Pentacorn
{
    class Trigger : IDisposable
    {
        public Trigger(Func<Task> action, Keys keys)
            : this(action, Program.WhenInput, keys) { }

        public Trigger(Func<Task> action, IObservable<Input> trigger, Keys keys)
            : this(action, trigger.Where(input => input.KeyDown(keys))) { }

        public Trigger(Func<Task> action, Keys keys, Window window)
            : this(action, Program.WhenInput, keys, window) { }

        public Trigger(Func<Task> action, IObservable<Input> trigger, Keys keys, Window window)
            : this(action, trigger.Where(input => window.Focused && input.KeyDown(keys))) { }

        public Trigger(Func<Task> action, IObservable<Input> trigger)
        {
            Disposable = trigger.Subscribe(async input =>
                {
                    var prev = Interlocked.CompareExchange(ref Latch, 1, 0);
                    if (0 != prev)
                        return;

                    await action();
                    Latch = 0;
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
