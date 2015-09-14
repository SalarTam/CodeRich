using System.Threading;

namespace Wintellect.Threading.ResourceLocks
{
    public sealed class ReaderWriterResourceLock : ResourceLock
    {
        private readonly ReaderWriterLock m_lockObj = new ReaderWriterLock();

        protected override void OnWaitToWrite()
        {
            this.m_lockObj.AcquireWriterLock(-1);
        }

        protected override void OnDoneWriting()
        {
            this.m_lockObj.ReleaseWriterLock();
        }

        protected override void OnWaitToRead()
        {
            this.m_lockObj.AcquireReaderLock(-1);
        }

        protected override void OnDoneReading()
        {
            this.m_lockObj.ReleaseReaderLock();
        }
    }
}