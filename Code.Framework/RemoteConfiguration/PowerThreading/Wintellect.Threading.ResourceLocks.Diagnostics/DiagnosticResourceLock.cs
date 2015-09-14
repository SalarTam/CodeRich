namespace Wintellect.Threading.ResourceLocks.Diagnostics
{
    public abstract class DiagnosticResourceLock : ResourceLock
    {
        private IResourceLock m_resLock;
        private bool m_IsMutualExclusive;
        private ResourceLock m_lock = new ExclusiveSpinResourceLock();

        protected IResourceLock InnerLock
        {
            get
            {
                return this.m_resLock;
            }
        }

        protected bool IsMutualExclusize
        {
            get
            {
                return this.m_IsMutualExclusive;
            }
        }

        protected DiagnosticResourceLock(ResourceLock resLock)
            : this(resLock, resLock.IsMutualExclusive)
        {
        }

        protected DiagnosticResourceLock(IResourceLock resLock)
            : this(resLock, false)
        {
        }

        protected DiagnosticResourceLock(IResourceLock resLock, bool isMutualExclusive)
            : base(isMutualExclusive)
        {
            this.m_resLock = resLock;
            this.m_IsMutualExclusive = isMutualExclusive;
        }

        public override bool Equals(object obj)
        {
            return this.m_resLock.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.m_resLock.GetHashCode();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_resLock.Dispose();
            }
        }

        protected override void OnWaitToWrite()
        {
            this.m_resLock.WaitToWrite();
        }

        protected override void OnDoneWriting()
        {
            this.m_resLock.DoneWriting();
        }

        protected override void OnWaitToRead()
        {
            this.m_resLock.WaitToRead();
        }

        protected override void OnDoneReading()
        {
            this.m_resLock.DoneReading();
        }
    }
}