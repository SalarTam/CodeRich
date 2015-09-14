using System;
using System.Diagnostics;
using System.Threading;

namespace Wintellect.Threading.ResourceLocks
{
    public abstract class ResourceLock : IResourceLock, IFormattable, IDisposable
    {
        private class DoneWritingDisposer : IDisposable
        {
            private ResourceLock m_resLockObj;

            public DoneWritingDisposer(ResourceLock resLockObj)
            {
                this.m_resLockObj = resLockObj;
            }

            public void Dispose()
            {
                this.m_resLockObj.DoneWriting();
            }
        }

        private class DoneReadingDisposer : IDisposable
        {
            private ResourceLock m_resLockObj;

            public DoneReadingDisposer(ResourceLock resLockObj)
            {
                this.m_resLockObj = resLockObj;
            }

            public void Dispose()
            {
                this.m_resLockObj.DoneReading();
            }
        }

        private IDisposable m_doneReadingDisposer;
        private IDisposable m_doneWritingDisposer;

        public bool IsMutualExclusive
        {
            get
            {
                return this.m_doneReadingDisposer == this.m_doneWritingDisposer;
            }
        }

        protected ResourceLock()
            : this(false)
        {
        }

        protected ResourceLock(bool lockIsReallyMutuallyExclusive)
        {
            this.m_doneWritingDisposer = new ResourceLock.DoneWritingDisposer(this);
            if (lockIsReallyMutuallyExclusive)
            {
                this.m_doneReadingDisposer = this.m_doneWritingDisposer;
                return;
            }
            this.m_doneReadingDisposer = new ResourceLock.DoneReadingDisposer(this);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public IDisposable WaitToWrite()
        {
            Thread.BeginCriticalRegion();
            this.OnWaitToWrite();
            return this.m_doneWritingDisposer;
        }

        protected abstract void OnWaitToWrite();

        public void DoneWriting()
        {
            this.OnDoneWriting();
            Thread.EndCriticalRegion();
        }

        protected abstract void OnDoneWriting();

        public IDisposable WaitToRead()
        {
            Thread.BeginCriticalRegion();
            this.OnWaitToRead();
            return this.m_doneReadingDisposer;
        }

        protected virtual void OnWaitToRead()
        {
            this.OnWaitToWrite();
        }

        public void DoneReading()
        {
            this.OnDoneReading();
            Thread.EndCriticalRegion();
        }

        protected virtual void OnDoneReading()
        {
            this.OnDoneWriting();
        }

        [Conditional("Stress")]
        protected static void StressPause()
        {
            Thread.Sleep(2);
        }

        protected static void StallThread()
        {
            ThreadUtil.StallThread();
        }

        protected static bool IfThen(ref int val, int @if, int then)
        {
            return InterlockedEx.IfThen(ref val, @if, then);
        }

        protected static bool IfThen(ref int val, int @if, int then, out int prevVal)
        {
            return InterlockedEx.IfThen(ref val, @if, then, out prevVal);
        }

        public string ToString(string format)
        {
            return this.ToString(format, null);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return this.ToString(null, formatProvider);
        }

        public virtual string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
            {
                return this.ToString();
            }
            if (string.Compare(format, "extra", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return this.ToString();
            }
            throw new FormatException("Unknown format string: " + format);
        }
    }
}