using System.Threading;

namespace Wintellect.Threading.ResourceLocks
{
    public sealed class OneResourceLock : ResourceLock
    {
        private const int c_lsFree = 0;
        private const int c_lsOwnedByWriter = 1;
        private const int c_lsMask = 7;
        private const int c_1WriterWaiting = 16777216;
        private const int c_WritersWaitingMask = -16777216;
        private int m_LockState;
        private Semaphore m_WritersLock = new Semaphore(0, 2147483647);

        public OneResourceLock()
            : base(true)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_WritersLock.Close();
                this.m_WritersLock = null;
            }
        }

        protected override void OnWaitToWrite()
        {
            int lockState = this.m_LockState;
            bool flag = false;
            while (!flag)
            {
                switch (lockState & 7)
                {
                    case 0:
                        flag = ResourceLock.IfThen(ref this.m_LockState, lockState, lockState | 1, out lockState);
                        break;

                    case 1:
                        if (ResourceLock.IfThen(ref this.m_LockState, lockState, lockState + 16777216, out lockState))
                        {
                            this.m_WritersLock.WaitOne();
                            lockState = this.m_LockState;
                        }
                        break;
                }
            }
        }

        protected override void OnDoneWriting()
        {
            int num = this.m_LockState;
            if ((num & -16777216) == 0)
            {
                num = InterlockedEx.MaskedExchange(ref this.m_LockState, 7, 0);
                if (num == 1)
                {
                    return;
                }
            }
            else
            {
                InterlockedEx.MaskedExchange(ref this.m_LockState, 7, 0);
            }
            num = this.m_LockState;
            while ((num & 7) == 0 && (num & -16777216) != 0)
            {
                if (ResourceLock.IfThen(ref this.m_LockState, num, num - 16777216, out num))
                {
                    this.m_WritersLock.Release(1);
                    return;
                }
            }
        }
    }
}