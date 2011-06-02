using System;
using System.Concurrency;

namespace Pentacorn
{
    public static class SchedulerEx
    {
        public static ITask SwitchTo(this IScheduler scheduler)
        {
            return new AnonymousTask(() => new ScheduledAwaiter(callback => scheduler.Schedule(callback)));
        }

        public static ITask SwitchTo(this IScheduler scheduler, TimeSpan interval)
        {
            return new AnonymousTask(() => new ScheduledAwaiter(callback => scheduler.Schedule(callback, interval)));
        }

        private class AnonymousTask : ITask
        {
            private readonly Func<IAwaiter> _getAwaiter;

            public AnonymousTask(Func<IAwaiter> getAwaiter)
            {
                _getAwaiter = getAwaiter;
            }

            public IAwaiter GetAwaiter()
            {
                return _getAwaiter();
            }
        }

        private class ScheduledAwaiter : IAwaiter
        {
            private readonly Action<Action> _scheduleCallback;

            internal ScheduledAwaiter(Action<Action> scheduleCallback)
            {
                _scheduleCallback = scheduleCallback;
            }

            public bool BeginAwait(Action callback)
            {
                _scheduleCallback(callback);
                return true;
            }

            public void EndAwait()
            {
            }
        }
    }

    public interface ITask
    {
        IAwaiter GetAwaiter();
    }

    public interface IAwaiter
    {
        bool BeginAwait(Action callback);
        void EndAwait();
    }
}
