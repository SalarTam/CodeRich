using System;
using System.Threading;

namespace Wintellect.Threading.DoNotUse
{
    public sealed class OptexBadWithLockPassing : IDisposable
    {
        private int m_Waiters;
        private Semaphore m_WaiterLock = new Semaphore(0, 2147483647);

        public void Dispose()
        {
            this.m_WaiterLock.Close();
            this.m_WaiterLock = null;
        }

        public void Enter()
        {
            Thread.BeginCriticalRegion();
            if (Interlocked.Increment(ref this.m_Waiters) == 1)
            {
                return;
            }
            this.m_WaiterLock.WaitOne();
        }

        public void Exit()
        {
            if (Interlocked.Decrement(ref this.m_Waiters) > 0)
            {
                this.m_WaiterLock.Release(1);
            }
            Thread.EndCriticalRegion();
        }
    }
}