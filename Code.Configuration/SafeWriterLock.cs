using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Configuration
{
    internal class SafeWriterLock : ISafeLock, System.IDisposable
    {
        private ReaderWriterLockSlim locker;
        public SafeWriterLock(ReaderWriterLockSlim locker)
        {
            this.locker = locker;
        }
        public void Dispose()
        {
            this.locker.ExitWriteLock();
        }
        public void AcquireLock()
        {
            this.locker.EnterWriteLock();
        }
    }
}
