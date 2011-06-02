using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using Emgu.CV.Structure;
using System.Threading.Tasks;

namespace Pentacorn
{
    static class IObservableEx
    {
        public static async Task<T> TakeNext<T>(this IObservable<T> observable)
        {
            return (await observable.Take(1)).First();
        }

        public static IObservable<Unit> Select<T>(this IObservable<T> observable)
        {
            return observable.Select(t => default(Unit));
        }

        public static IObservable<Picture<Gray, byte>> AddRef(this IObservable<Picture<Gray, byte>> observable)
        {
            return Observable.CreateWithDisposable<Picture<Gray, byte>>(observer =>
                {
                    return observable.Subscribe(
                        p =>
                        {
                            if (p == null || p.IsDisposed)
                                return; // Todo, major hack;
                            p.AddRefs(1);
                            observer.OnNext(p);
                        },
                        e => observer.OnError(e),
                        () => observer.OnCompleted());
                });
        }

        public static IObservable<PointF[]> FindChessboardSaddles(this IObservable<Picture<Gray, byte>> observable, Chessboard chessboard)
        {
            int searching = 0;
            return Observable.CreateWithDisposable<PointF[]>(observer =>
            {
                return observable.Subscribe(
                    async p =>
                        {
                            if (0 == Interlocked.CompareExchange(ref searching, 1, 0))
                            {
                                p.Dispose();
                            }
                            else
                            {
                                var saddles = await p.FindChessboardCornersAsync(chessboard.SaddleCount);

                                // Todo, should use a proper using statement, seems to not work
                                // so great with await. Probably my fault rather than a CTP problem, but
                                // don't want to dig to deep right now.
                                p.Dispose();

                                if (saddles != null)
                                    observer.OnNext(saddles);

                                searching = 0;
                            }

                        },
                    e => observer.OnError(e),
                    () => observer.OnCompleted());
            });
        }
            
        public static IObservable<T> Using<T>(this IObservable<T> observable) where T : IDisposable
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
