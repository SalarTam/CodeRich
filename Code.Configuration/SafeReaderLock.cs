using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Configuration
{
    internal class SafeReaderLock : ISafeLock, System.IDisposable
    {
        private ReaderWriterLockSlim locker;
        public SafeReaderLock(ReaderWriterLockSlim locker)
        {
            this.locker = locker;
        }
        public void Dispose()
        {
            this.locker.ExitReadLock();
        }
        public void AcquireLock()
        {
            this.locker.EnterReadLock();
        }
    }
}
