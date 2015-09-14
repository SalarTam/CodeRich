using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Configuration
{
    internal class SafeUpgradeReaderLock : ISafeLock, System.IDisposable
    {
        private ReaderWriterLockSlim locker;
        public SafeUpgradeReaderLock(ReaderWriterLockSlim locker)
        {
            this.locker = locker;
        }
        public void Dispose()
        {
            this.locker.ExitUpgradeableReadLock();
        }
        public void AcquireLock()
        {
            this.locker.EnterUpgradeableReadLock();
        }
    }
}
