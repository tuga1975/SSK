using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trinity.Device
{
    internal static class Repeat
    {
        private static TimeSpan _pollInterval = TimeSpan.FromSeconds(10);

        public static Task Interval(
            Action action,
            CancellationToken token)
        {
            // We don't use Observable.Interval:
            // If we block, the values start bunching up behind each other.
            return Task.Factory.StartNew(
                () =>
                {
                    for (; ; )
                    {
                        if (token.WaitCancellationRequested(_pollInterval))
                            break;

                        action();
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }

    static class CancellationTokenExtensions
    {
        public static bool WaitCancellationRequested(
            this CancellationToken token,
            TimeSpan timeout)
        {
            return token.WaitHandle.WaitOne(timeout);
        }
    }
}
