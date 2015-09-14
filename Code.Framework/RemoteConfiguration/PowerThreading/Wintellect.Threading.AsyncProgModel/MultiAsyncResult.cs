using System;
using System.Collections.Generic;
using System.Threading;

namespace Wintellect.Threading.AsyncProgModel
{
    public class MultiAsyncResult : AsyncResult<bool>
    {
        private const int c_StatePending = 0;
        private const int c_StateDone = 1;
        private const int c_StateCancelled = 2;
        private int m_State;
        private int m_NumQueuedOps;
        private int m_NumCompletedOps = -1;
        private int m_NumFailedOps;
        private List<AsyncOpResult> m_AsyncOpResults = new List<AsyncOpResult>();

        public bool WasCancelled
        {
            get
            {
                return this.m_State == 2;
            }
        }

        public int NumQueuedOps
        {
            get
            {
                return this.m_NumQueuedOps;
            }
        }

        public int NumCompletedOps
        {
            get
            {
                return this.m_NumCompletedOps;
            }
        }

        public int NumFailedOps
        {
            get
            {
                return this.m_NumFailedOps;
            }
        }

        public MultiAsyncResult(AsyncCallback ac, object state)
            : base(ac, state)
        {
        }

        public bool EndMultiAsyncResult(IAsyncResult ar)
        {
            return !this.WasCancelled;
        }

        public AsyncOpResult[] GetAsyncOpResults()
        {
            return this.m_AsyncOpResults.ToArray();
        }

        public PendingAsyncOp<TAsyncObj, TState, TResult> CreateAsyncOp<TAsyncObj, TState, TResult>(string name, TAsyncObj asyncObj, TState state)
        {
            AsyncOpResult<TState, TResult> aor = new AsyncOpResult<TState, TResult>(name, state);
            int resultIndex = this.AddAsyncOpResult(aor);
            return new PendingAsyncOp<TAsyncObj, TState, TResult>(this, resultIndex, asyncObj);
        }

        public PendingAsyncOp<TAsyncObj, TResult> CreateAsyncOp<TAsyncObj, TResult>(string name, TAsyncObj asyncObj)
        {
            AsyncOpResult<object, TResult> aor = new AsyncOpResult<object, TResult>(name, null);
            int resultIndex = this.AddAsyncOpResult(aor);
            return new PendingAsyncOp<TAsyncObj, TResult>(this, resultIndex, asyncObj);
        }

        public PendingAsyncOp<TAsyncObj> CreateAsyncOp<TAsyncObj>(string name, TAsyncObj asyncObj)
        {
            AsyncOpResult<object, object> aor = new AsyncOpResult<object, object>(name, null);
            int resultIndex = this.AddAsyncOpResult(aor);
            return new PendingAsyncOp<TAsyncObj, object>(this, resultIndex, asyncObj);
        }

        public PendingAsyncOp CreateAsyncOp(string name)
        {
            AsyncOpResult<object, object> aor = new AsyncOpResult<object, object>(name, null);
            int resultIndex = this.AddAsyncOpResult(aor);
            return new PendingAsyncOp(this, resultIndex);
        }

        private int AddAsyncOpResult(AsyncOpResult aor)
        {
            Interlocked.Increment(ref this.m_NumQueuedOps);
            this.m_AsyncOpResults.Add(aor);
            return this.m_AsyncOpResults.Count - 1;
        }

        public void DoneQueueing(int timeoutMilliseconds)
        {
            CountdownTimer countdownTimer = new CountdownTimer();
            countdownTimer.BeginCountdown(timeoutMilliseconds, new AsyncCallback(this.TimeoutCancel), countdownTimer);
            this.AnOpCompleted();
        }

        private void TimeoutCancel(IAsyncResult ar)
        {
            CountdownTimer countdownTimer = (CountdownTimer)ar.AsyncState;
            countdownTimer.EndCountdown(ar);
            this.TryToCancel();
        }

        public bool TryToCancel()
        {
            int num = Interlocked.CompareExchange(ref this.m_State, 2, 0);
            bool flag = num == 0;
            if (flag)
            {
                base.SetAsCompleted(true, false);
            }
            return flag;
        }

        internal TState GetState<TState, TResult>(int resultIndex)
        {
            return ((AsyncOpResult<TState, TResult>)this.m_AsyncOpResults[resultIndex]).State;
        }

        internal string GetName(int resultIndex)
        {
            return this.m_AsyncOpResults[resultIndex].Name;
        }

        internal void SetResult<TState, TResult>(int resultIndex, TResult result, bool cancel)
        {
            ((AsyncOpResult<TState, TResult>)this.m_AsyncOpResults[resultIndex]).Result = result;
            this.AnOpCompleted();
            if (cancel)
            {
                this.TryToCancel();
            }
        }

        internal void SetFailure(int resultIndex, Exception e, bool cancel)
        {
            this.m_AsyncOpResults[resultIndex].Exception = e;
            Interlocked.Increment(ref this.m_NumFailedOps);
            this.AnOpCompleted();
            if (cancel)
            {
                this.TryToCancel();
            }
        }

        private void AnOpCompleted()
        {
            if (Interlocked.Increment(ref this.m_NumCompletedOps) == this.m_NumQueuedOps)
            {
                int num = Interlocked.CompareExchange(ref this.m_State, 1, 0);
                if (num == 2)
                {
                    return;
                }
                base.SetAsCompleted(false, false);
            }
        }
    }
}