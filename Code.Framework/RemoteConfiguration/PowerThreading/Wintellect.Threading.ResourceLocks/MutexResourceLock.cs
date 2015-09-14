using System.Threading;

namespace Wintellect.Threading.ResourceLocks
{
    public sealed class MutexResourceLock : ResourceLock
    {
        private readonly Mutex m_lockObj;

        public MutexResourceLock()
            : this(false)
        {
        }

        public MutexResourceLock(bool initiallyOwned)
            : base(true)
        {
            this.m_lockObj = new Mutex(initiallyOwned);
        }

        protected override void OnWaitToWrite()
        {
            this.m_lockObj.WaitOne();
        }

        protected override void OnDoneWriting()
        {
            this.m_lockObj.ReleaseMutex();
        }
    }
}