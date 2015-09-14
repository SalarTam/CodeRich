using System;

namespace Wintellect.Threading.ThreadPool
{
    public sealed class CallbackThreadPoolEventArgs : EventArgs
    {
        private CallbackReason m_reason;

        public CallbackReason Reason
        {
            get
            {
                return this.m_reason;
            }
        }

        public CallbackThreadPoolEventArgs(CallbackReason reason)
        {
            this.m_reason = reason;
        }
    }
}