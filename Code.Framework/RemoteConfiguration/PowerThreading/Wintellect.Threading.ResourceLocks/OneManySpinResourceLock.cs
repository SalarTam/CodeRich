using System.Threading;

namespace Wintellect.Threading.ResourceLocks
{
    public sealed class OneManySpinResourceLock : ResourceLock
    {
        private const int c_lsFree = 0;
        private const int c_lsOwnedByWriter = 1;
        private const int c_1WritersPending = 256;
        private const int c_WritersPendingMask = 65280;
        private const int c_1ReadersReading = 65536;
        private const int c_ReadersReadingMask = 16711680;
        private int m_LockState;

        public OneManySpinResourceLock()
            : base(false)
        {
        }

        protected override void OnWaitToWrite()
        {
            Interlocked.Add(ref this.m_LockState, 256);
            InterlockedEx.MaskedOr(ref this.m_LockState, 1, 65280);
        }

        protected override void OnDoneWriting()
        {
            Interlocked.Add(ref this.m_LockState, -257);
        }

        protected override void OnWaitToRead()
        {
            InterlockedEx.MaskedAdd(ref this.m_LockState, 65536, 16711680);
        }

        protected override void OnDoneReading()
        {
            Interlocked.Add(ref this.m_LockState, -65536);
        }
    }
}