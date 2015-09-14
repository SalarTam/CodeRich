using System;
using System.Threading;

namespace Wintellect.Threading.AsyncProgModel
{
    public sealed class CountdownTimer
    {
        private sealed class CountdownAsyncResult : AsyncResultNoReturn
        {
            private Timer m_Timer;

            public CountdownAsyncResult(int ms, AsyncCallback ac, object state)
                : base(ac, state)
            {
                this.m_Timer = new Timer(new TimerCallback(this.CountdownDone), null, ms, -1);
            }

            private void CountdownDone(object state)
            {
                base.SetAsCompleted(null, false);
                this.m_Timer.Dispose();
                this.m_Timer = null;
            }
        }

        public IAsyncResult BeginCountdown(int ms, AsyncCallback ac, object state)
        {
            return new CountdownTimer.CountdownAsyncResult(ms, ac, state);
        }

        public void EndCountdown(IAsyncResult ar)
        {
        }
    }
}