using System;
using System.Diagnostics;
using System.Threading;

namespace Wintellect.Threading
{
    public sealed class OperationTimer : IDisposable
    {
        private static int s_NumOperationTimersStarted;
        private Stopwatch m_sw;
        private string m_text;
        private int m_collectionCount;

        public OperationTimer()
            : this(string.Empty)
        {
        }

        public OperationTimer(string text)
        {
            if (Interlocked.Increment(ref OperationTimer.s_NumOperationTimersStarted) == 1)
            {
                OperationTimer.PrepareForOperation();
            }
            this.m_text = text;
            this.m_collectionCount = GC.CollectionCount(0);
            this.m_sw = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            Console.WriteLine("{0} (GCs={1,3}) {2}", this.m_sw.Elapsed, GC.CollectionCount(0) - this.m_collectionCount, this.m_text);
            Interlocked.Decrement(ref OperationTimer.s_NumOperationTimersStarted);
            this.m_sw = null;
        }

        public static void PrepareForOperation()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}