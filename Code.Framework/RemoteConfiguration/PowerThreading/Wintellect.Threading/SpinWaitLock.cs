using System.Threading;

namespace Wintellect.Threading
{
    public struct SpinWaitLock
    {
        private const int c_lsFree = 0;
        private const int c_lsOwned = 1;
        private int m_LockState;

        public void Enter()
        {
            Thread.BeginCriticalRegion();
            while (Interlocked.Exchange(ref this.m_LockState, 1) != 0)
            {
                while (Thread.VolatileRead(ref this.m_LockState) == 1)
                {
                    ThreadUtil.StallThread();
                }
            }
        }

        public void Exit()
        {
            Interlocked.Exchange(ref this.m_LockState, 0);
            Thread.EndCriticalRegion();
        }
    }
}