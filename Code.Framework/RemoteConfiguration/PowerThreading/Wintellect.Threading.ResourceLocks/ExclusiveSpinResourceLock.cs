namespace Wintellect.Threading.ResourceLocks
{
    public sealed class ExclusiveSpinResourceLock : ResourceLock
    {
        private SpinWaitLock m_lock = default(SpinWaitLock);

        public ExclusiveSpinResourceLock()
            : base(true)
        {
        }

        protected override void OnWaitToWrite()
        {
            this.m_lock.Enter();
        }

        protected override void OnDoneWriting()
        {
            this.m_lock.Exit();
        }
    }
}