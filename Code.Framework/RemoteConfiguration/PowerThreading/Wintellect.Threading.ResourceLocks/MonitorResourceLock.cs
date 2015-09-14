using System.Threading;

namespace Wintellect.Threading.ResourceLocks
{
    public sealed class MonitorResourceLock : ResourceLock
    {
        private readonly object m_lock;

        public MonitorResourceLock()
            : base(true)
        {
            this.m_lock = this;
        }

        public MonitorResourceLock(object obj)
            : base(true)
        {
            this.m_lock = obj;
        }

        protected override void OnWaitToWrite()
        {
            Monitor.Enter(this.m_lock);
        }

        protected override void OnDoneWriting()
        {
            Monitor.Exit(this.m_lock);
        }
    }
}