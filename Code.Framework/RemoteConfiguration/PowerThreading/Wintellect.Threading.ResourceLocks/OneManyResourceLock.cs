using System.Threading;

namespace Wintellect.Threading.ResourceLocks
{
    public sealed class OneManyResourceLock : ResourceLock
    {
        private const int c_lsFree = 0;
        private const int c_lsOwnedByWriter = 1;
        private const int c_lsOwnedByReaders = 2;
        private const int c_lsOwnedByReadersAndWriterWaiting = 3;
        private const int c_lsReservedForWriter = 4;
        private const int c_lsMask = 7;
        private const int c_1ReaderReading = 256;
        private const int c_ReadersReadingMask = 65280;
        private const int c_1ReaderWaiting = 65536;
        private const int c_ReadersWaitingMask = 16711680;
        private const int c_1WriterWaiting = 16777216;
        private const int c_WritersWaitingMask = -16777216;
        private int m_LockState;
        private Semaphore m_ReadersLock = new Semaphore(0, 2147483647);
        private Semaphore m_WritersLock = new Semaphore(0, 2147483647);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_WritersLock.Close();
                this.m_WritersLock = null;
                this.m_ReadersLock.Close();
                this.m_ReadersLock = null;
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

                    case 2:
                    case 3:
                        if (ResourceLock.IfThen(ref this.m_LockState, lockState, ((lockState & -8) | 3) + 16777216, out lockState))
                        {
                            this.m_WritersLock.WaitOne();
                            lockState = this.m_LockState;
                        }
                        break;

                    case 4:
                        flag = ResourceLock.IfThen(ref this.m_LockState, lockState, (lockState & -8) | 1, out lockState);
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
                InterlockedEx.MaskedExchange(ref this.m_LockState, 7, 4);
            }
            num = this.m_LockState;
            while ((num & 7) == 4)
            {
                if ((num & -16777216) == 0)
                {
                    break;
                }
                if (ResourceLock.IfThen(ref this.m_LockState, num, num - 16777216, out num))
                {
                    this.m_WritersLock.Release(1);
                    return;
                }
            }
            while (((num & 7) == 0 || (num & 7) == 2) && (num & 16711680) != 0)
            {
                if (ResourceLock.IfThen(ref this.m_LockState, num, num & -16711681, out num))
                {
                    int releaseCount = (num & 16711680) >> 16;
                    this.m_ReadersLock.Release(releaseCount);
                    return;
                }
            }
        }

        protected override void OnWaitToRead()
        {
            int lockState = this.m_LockState;
            bool flag = false;
            while (!flag)
            {
                switch (lockState & 7)
                {
                    case 0:
                        flag = ResourceLock.IfThen(ref this.m_LockState, lockState, 258, out lockState);
                        break;

                    case 1:
                    case 3:
                    case 4:
                        if (ResourceLock.IfThen(ref this.m_LockState, lockState, lockState + 65536, out lockState))
                        {
                            this.m_ReadersLock.WaitOne();
                            lockState = this.m_LockState;
                        }
                        break;

                    case 2:
                        flag = ResourceLock.IfThen(ref this.m_LockState, lockState, lockState + 256, out lockState);
                        break;
                }
            }
        }

        protected override void OnDoneReading()
        {
            int lockState = this.m_LockState;
            int num;
            do
            {
                num = lockState - 256;
                if ((num & 65280) == 0)
                {
                    if ((num & -16777216) == 0)
                    {
                        num = 0;
                    }
                    else
                    {
                        num = ((num & -8) | 4);
                    }
                }
            }
            while (!ResourceLock.IfThen(ref this.m_LockState, lockState, num, out lockState));
            if ((num & 7) == 2 || (num & 7) == 3 || (num & 7) == 0)
            {
                return;
            }
            lockState = this.m_LockState;
            while ((lockState & 7) == 4 && (lockState & -16777216) != 0)
            {
                if (ResourceLock.IfThen(ref this.m_LockState, lockState, lockState - 16777216, out lockState))
                {
                    this.m_WritersLock.Release(1);
                    return;
                }
            }
        }
    }
}