using System;
using System.Threading;
namespace Code.Configuration
{
	public class SafeReaderWriterLock
	{
		private SafeUpgradeReaderLock upgradableReaderLock;
		private SafeReaderLock readerLock;
		private SafeWriterLock writerLock;
		private ReaderWriterLockSlim locker;
		public SafeReaderWriterLock()
		{
			this.locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
			this.upgradableReaderLock = new SafeUpgradeReaderLock(this.locker);
			this.readerLock = new SafeReaderLock(this.locker);
			this.writerLock = new SafeWriterLock(this.locker);
		}
		public ISafeLock AcquireReaderLock()
		{
			this.readerLock.AcquireLock();
			return this.readerLock;
		}
		public ISafeLock AcquireWriterLock()
		{
			this.writerLock.AcquireLock();
			return this.writerLock;
		}
		public ISafeLock AcquireUpgradeableReaderLock()
		{
			this.upgradableReaderLock.AcquireLock();
			return this.upgradableReaderLock;
		}
	}
}
