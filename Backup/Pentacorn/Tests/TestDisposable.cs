using System;
using System.Linq;
using System.Threading;

namespace Pentacorn.Tasks
{
    class TestDisposable : IDisposable
    {
        private static int Salive = 0;
        private static int Sid = -1;
        public int Id = Interlocked.Increment(ref Sid);
        public int Count;
        public TestDisposable()
        {
            Write("Ctor");
            Interlocked.Increment(ref Salive);
            Count = 1;
        }

        public TestDisposable AddRefs(int n)
        {
            Write("AddRefs");
            Interlocked.Add(ref Count, n);
            return this;
        }

        public void Write(string s)
        {
            Console.WriteLine("[{0}] {1}, {2}: {3} ({4} alive)", Program.CurrentContext, Count.ToString().PadLeft(4 * Count), Id, s, Salive);
        }

        public void Dispose()
        {
            Write("Dispose");
            if (0 == Interlocked.Decrement(ref Count))
            {
                Interlocked.Decrement(ref Salive);
                Write("Killed");
            }
        }
    }

    static class TestDisposableEx
    {
        public static IObservable<TestDisposable> AddRef(this IObservable<TestDisposable> observable)
        {
            return Observable.CreateWithDisposable<TestDisposable>(observer =>
            {
                return observable.Subscribe(
                    p => { p.AddRefs(1); observer.OnNext(p); },
                    e => observer.OnError(e),
                    () => observer.OnCompleted());
            });
        }

        public static IObservable<TestDisposable> Using(this IObservable<TestDisposable> observable)
        {
            return observable.SelectMany(d =>
            {
                return Observable.Using(() => d, e =>
                {

                    return Observable.Return(e);
                });
            });
        }
    }
}
