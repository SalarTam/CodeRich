using System.Threading;
using Wintellect.Threading.ResourceLocks;

namespace Wintellect.Threading.DoNotUse
{
    public sealed class OneManyBadWithLockPassingResourceLock : ResourceLock
    {
        private SpinWaitLock m_FieldLock = default(SpinWaitLock);
        private readonly Semaphore m_ReadersLock = new Semaphore(0, 2147483647);
        private readonly Semaphore m_WritersLock = new Semaphore(0, 2147483647);
        private int m_NumWaitingReaders;
        private int m_NumWaitingWriters;
        private int m_NumActive;
        private int m_ReadersThatHadToWait;
        private int m_WritersThatHadToWait;

        ~OneManyBadWithLockPassingResourceLock()
        {
            this.Dispose(false);
        }

        public override string ToString()
        {
            return string.Format("ReadersThatHadToWait={2}, WritersThatHadToWait={3}", this.m_ReadersThatHadToWait, this.m_WritersThatHadToWait);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_NumWaitingReaders = (this.m_NumWaitingWriters = (this.m_NumActive = 0));
                this.m_ReadersLock.Close();
                this.m_WritersLock.Close();
            }
        }

        protected override void OnWaitToWrite()
        {
            this.m_FieldLock.Enter();
            bool flag = this.m_NumActive != 0;
            try
            {
                if (flag)
                {
                    this.m_NumWaitingWriters++;
                }
                else
                {
                    this.m_NumActive = -1;
                }
            }
            finally
            {
                this.m_FieldLock.Exit();
            }
            if (flag)
            {
                Interlocked.Increment(ref this.m_WritersThatHadToWait);
                this.m_WritersLock.WaitOne();
            }
        }

        protected override void OnDoneWriting()
        {
            Semaphore semaphore = null;
            int releaseCount = 1;
            this.m_FieldLock.Enter();
            try
            {
                if (this.m_NumActive > 0)
                {
                    this.m_NumActive--;
                }
                else
                {
                    this.m_NumActive = 0;
                }
                if (this.m_NumActive == 0)
                {
                    if (this.m_NumWaitingWriters > 0)
                    {
                        this.m_NumActive = -1;
                        this.m_NumWaitingWriters--;
                        semaphore = this.m_WritersLock;
                    }
                    else
                    {
                        if (this.m_NumWaitingReaders > 0)
                        {
                            this.m_NumActive = this.m_NumWaitingReaders;
                            this.m_NumWaitingReaders = 0;
                            semaphore = this.m_ReadersLock;
                            releaseCount = this.m_NumActive;
                        }
                    }
                }
            }
            finally
            {
                this.m_FieldLock.Exit();
            }
            if (semaphore != null)
            {
                semaphore.Release(releaseCount);
            }
            Thread.EndCriticalRegion();
        }

        protected override void OnWaitToRead()
        {
            Thread.BeginCriticalRegion();
            this.m_FieldLock.Enter();
            bool flag = this.m_NumWaitingWriters > 0 || this.m_NumActive < 0;
            try
            {
                if (flag)
                {
                    this.m_NumWaitingReaders++;
                }
                else
                {
                    this.m_NumActive++;
                }
            }
            finally
            {
                this.m_FieldLock.Exit();
            }
            if (flag)
            {
                Interlocked.Increment(ref this.m_ReadersThatHadToWait);
                this.m_ReadersLock.WaitOne();
            }
        }
    }
}