namespace Wintellect.Threading.ResourceLocks
{
    public sealed class NullResourceLock : ResourceLock
    {
        public NullResourceLock()
            : base(true)
        {
        }

        protected override void OnWaitToWrite()
        {
        }

        protected override void OnDoneWriting()
        {
        }
    }
}