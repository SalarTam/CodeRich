using System;
using System.Diagnostics;
using System.Threading;

namespace Wintellect.Threading.ResourceLocks.Diagnostics
{
    public class TimeoutNotifierResourceLock : DiagnosticResourceLock
    {
        private long m_timeout;

        public TimeoutNotifierResourceLock(IResourceLock resLock, long timeout)
            : base(resLock)
        {
            this.m_timeout = timeout;
        }

        public TimeoutNotifierResourceLock(IResourceLock resLock, TimeSpan timeout)
            : this(resLock, timeout.Ticks)
        {
        }

        protected virtual void OnTimeout(object state)
        {
            StackTrace stackTrace = (StackTrace)state;
            string message = "Timed out while waiting for lock. Stack trace of waiting thread follows:" + Environment.NewLine + stackTrace.ToString();
            throw new TimeoutException(message);
        }

        protected override void OnWaitToRead()
        {
            StackTrace state = new StackTrace(0, true);
            using (new Timer(new TimerCallback(this.OnTimeout), state, this.m_timeout, -1L))
            {
                base.InnerLock.WaitToRead();
            }
        }

        protected override void OnDoneReading()
        {
            base.InnerLock.DoneReading();
        }

        protected override void OnWaitToWrite()
        {
            StackTrace state = new StackTrace(0, true);
            using (new Timer(new TimerCallback(this.OnTimeout), state, this.m_timeout, -1L))
            {
                base.InnerLock.WaitToWrite();
            }
        }

        protected override void OnDoneWriting()
        {
            base.InnerLock.DoneWriting();
        }
    }
}