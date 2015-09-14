using System.Threading;

namespace Wintellect.Threading.ResourceLocks
{
    public class OptexResourceLock : ResourceLock
    {
        private const int c_lsFree = 0;
        private const int c_lsOwned = 1;
        private const int c_1Waiter = 2;
        private int m_LockState;
        private Semaphore m_WaiterLock = new Semaphore(0, 2147483647);

        public OptexResourceLock()
            : base(true)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_WaiterLock.Close();
                this.m_WaiterLock = null;
            }
        }

        protected override void OnWaitToWrite()
        {
            while (true)
            {
                int num = InterlockedEx.Or(ref this.m_LockState, 1);
                if ((num & 1) == 0)
                {
                    break;
                }
                if (ResourceLock.IfThen(ref this.m_LockState, num, num + 2))
                {
                    this.m_WaiterLock.WaitOne();
                }
            }
        }

        protected override void OnDoneWriting()
        {
            int num = InterlockedEx.And(ref this.m_LockState, -2);
            if (num == 1)
            {
                return;
            }
            num &= -2;
            if (ResourceLock.IfThen(ref this.m_LockState, num, num - 2))
            {
                this.m_WaiterLock.Release(1);
            }
        }
    }
}