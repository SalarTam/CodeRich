using System;
using System.Threading;

namespace Wintellect.Threading.ResourceLocks.Diagnostics
{
    public sealed class ThreadSafeCheckerResourceLock : DiagnosticResourceLock
    {
        private int m_LockState;

        public ThreadSafeCheckerResourceLock(ResourceLock resLock)
            : base(resLock, resLock.IsMutualExclusive)
        {
        }

        public ThreadSafeCheckerResourceLock(IResourceLock resLock)
            : base(resLock, false)
        {
        }

        public ThreadSafeCheckerResourceLock(IResourceLock resLock, bool isMutualExclusive)
            : base(resLock, isMutualExclusive)
        {
        }

        protected override void Dispose(bool disposing)
        {
            this.VerifyNoReaders("Lock held by readers while being disposed");
            this.VerifyNoWriters("Lock held by a writer while being disposed");
            base.Dispose(disposing);
        }

        protected override void OnWaitToRead()
        {
            if (base.IsMutualExclusive)
            {
                base.WaitToWrite();
                return;
            }
            base.InnerLock.WaitToRead();
            this.VerifyNoWriters("Reading while already writing!");
            Interlocked.Increment(ref this.m_LockState);
        }

        protected override void OnDoneReading()
        {
            this.VerifySomeReaders("Done reading while not reading!");
            this.VerifyNoWriters("Done reading while already writing!");
            Interlocked.Decrement(ref this.m_LockState);
            base.InnerLock.DoneReading();
        }

        protected override void OnWaitToWrite()
        {
            if (base.IsMutualExclusive)
            {
                base.WaitToWrite();
                return;
            }
            base.InnerLock.WaitToWrite();
            this.VerifyNoWriters("Writing while already writing!");
            this.VerifyNoReaders("Writing while already reading!");
            InterlockedEx.BitTestAndSet(ref this.m_LockState, 31);
        }

        protected override void OnDoneWriting()
        {
            this.VerifyOneWriter("Done writing while not writing!");
            this.VerifyNoReaders("Done writing while already reading!");
            InterlockedEx.BitTestAndReset(ref this.m_LockState, 31);
            base.InnerLock.DoneWriting();
        }

        private void VerifyNoWriters(string message)
        {
            if (((long)this.m_LockState & 0x80000000) != 0L)
            {
                this.ThrowException(message);
            }
        }

        private void VerifyOneWriter(string message)
        {
            if (((long)this.m_LockState & 0x80000000) == 0L)
            {
                this.ThrowException(message);
            }
        }

        private void VerifyNoReaders(string message)
        {
            if ((this.m_LockState & 2147483647) != 0)
            {
                this.ThrowException(message);
            }
        }

        private void VerifySomeReaders(string message)
        {
            if ((this.m_LockState & 2147483647) == 0)
            {
                this.ThrowException(message);
            }
        }

        private void ThrowException(string message)
        {
            throw new InvalidOperationException(message);
        }
    }
}