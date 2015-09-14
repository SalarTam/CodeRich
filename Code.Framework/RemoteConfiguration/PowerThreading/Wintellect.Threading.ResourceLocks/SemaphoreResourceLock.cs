using System.Threading;

namespace Wintellect.Threading.ResourceLocks
{
    public sealed class SemaphoreResourceLock : ResourceLock
    {
        private readonly Semaphore m_lockObj;

        public SemaphoreResourceLock()
            : base(true)
        {
            this.m_lockObj = new Semaphore(1, 1);
        }

        protected override void OnWaitToWrite()
        {
            this.m_lockObj.WaitOne();
        }

        protected override void OnDoneWriting()
        {
            this.m_lockObj.Release();
        }
    }
}