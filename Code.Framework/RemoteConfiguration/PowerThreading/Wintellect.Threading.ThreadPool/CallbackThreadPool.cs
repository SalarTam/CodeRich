using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Wintellect.Threading.ThreadPool
{
    public class CallbackThreadPool : IDisposable
    {
        private const uint c_WAIT_TIMEOUT = 258u;
        private IOCompletionPort m_iocp;
        private int m_idleThreadTimeout;
        private int m_maxThreadsAllowed = 2147483647;
        private int m_numThreadsInPool;
        private int m_maxThreadsEverInPool;
        private int m_numBusyThreads;
        private int m_numItemsPosted;
        private int m_numItemsProcessed;
        private EventHandler<CallbackThreadPoolEventArgs> m_threadEvent;
        private object m_threadEventLock = new object();

        public event EventHandler<CallbackThreadPoolEventArgs> ThreadEvent
        {
            add
            {
                object threadEventLock;
                Monitor.Enter(threadEventLock = this.m_threadEventLock);
                try
                {
                    this.m_threadEvent = (EventHandler<CallbackThreadPoolEventArgs>)Delegate.Combine(this.m_threadEvent, value);
                }
                finally
                {
                    Monitor.Exit(threadEventLock);
                }
            }
            remove
            {
                object threadEventLock;
                Monitor.Enter(threadEventLock = this.m_threadEventLock);
                try
                {
                    this.m_threadEvent = (EventHandler<CallbackThreadPoolEventArgs>)Delegate.Remove(this.m_threadEvent, value);
                }
                finally
                {
                    Monitor.Exit(threadEventLock);
                }
            }
        }

        public int MaxThreadsAllowed
        {
            get
            {
                return this.m_maxThreadsAllowed;
            }
            set
            {
                Interlocked.Exchange(ref this.m_maxThreadsAllowed, value);
            }
        }

        public int ThreadsInPool
        {
            get
            {
                return this.m_numThreadsInPool;
            }
        }

        public int MaxThreadsEverInPool
        {
            get
            {
                return this.m_maxThreadsEverInPool;
            }
        }

        public int BusyThreads
        {
            get
            {
                return this.m_numBusyThreads;
            }
        }

        public int NumItemsPosted
        {
            get
            {
                return this.m_numItemsPosted;
            }
        }

        public int NumItemsProcessed
        {
            get
            {
                return this.m_numItemsProcessed;
            }
        }

        public int NumItemsPending
        {
            get
            {
                return this.NumItemsPosted - this.NumItemsProcessed;
            }
        }

        public virtual void OnThreadEvent(CallbackReason reason)
        {
            EventHandler<CallbackThreadPoolEventArgs> threadEvent = this.m_threadEvent;
            if (threadEvent != null)
            {
                threadEvent(this, new CallbackThreadPoolEventArgs(reason));
            }
        }

        public CallbackThreadPool(int idleThreadTimeout)
            : this(idleThreadTimeout, 2147483647, 0)
        {
        }

        public CallbackThreadPool(int idleThreadTimeout, int maxThreadsAllowed)
            : this(idleThreadTimeout, maxThreadsAllowed, 0)
        {
        }

        public CallbackThreadPool(int idleThreadTimeout, int maxThreadsAllowed, int maxConcurrency)
        {
            this.m_maxThreadsAllowed = maxThreadsAllowed;
            this.m_idleThreadTimeout = idleThreadTimeout;
            this.m_iocp = new IOCompletionPort(maxConcurrency);
        }

        ~CallbackThreadPool()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_iocp.Dispose();
            }
        }

        public void QueueUserWorkItem(WaitCallback cb)
        {
            this.QueueUserWorkItem(cb, null);
        }

        public virtual void QueueUserWorkItem(WaitCallback cb, object state)
        {
            if (this.m_numThreadsInPool == 0)
            {
                this.AddThreadToPool();
            }
            Interlocked.Increment(ref this.m_numItemsPosted);
            this.m_iocp.PostStatus(cb, state);
        }

        private static int InterlockedMax(ref int target, int val)
        {
            int num = target;
            int num2;
            do
            {
                num2 = num;
                num = Interlocked.CompareExchange(ref target, Math.Max(num2, val), num2);
            }
            while (num2 != num);
            return num;
        }

        private void AddThreadToPool()
        {
            Interlocked.Increment(ref this.m_numThreadsInPool);
            CallbackThreadPool.InterlockedMax(ref this.m_maxThreadsEverInPool, this.m_numThreadsInPool);
            Interlocked.Increment(ref this.m_numBusyThreads);
            new Thread(new ThreadStart(this.ThreadPoolFunc))
            {
                IsBackground = true
            }.Start();
        }

        protected void ThreadPoolFunc()
        {
            try
            {
                this.OnThreadEvent(CallbackReason.AddingThreadToPool);
                bool flag = true;
                while (flag)
                {
                    Interlocked.Decrement(ref this.m_numBusyThreads);
                    bool flag2;
                    WaitCallback waitCallback;
                    object state;
                    this.m_iocp.GetStatus(this.m_idleThreadTimeout, out flag2, out waitCallback, out state);
                    int num = Interlocked.Increment(ref this.m_numBusyThreads);
                    if (flag2)
                    {
                        break;
                    }
                    if (num == this.m_numThreadsInPool && num < this.m_maxThreadsAllowed)
                    {
                        this.AddThreadToPool();
                    }
                    this.OnThreadEvent(CallbackReason.MethodCall);
                    Interlocked.Increment(ref this.m_numItemsProcessed);
                    waitCallback(state);
                    this.OnThreadEvent(CallbackReason.MethodReturn);
                }
            }
            finally
            {
                Interlocked.Decrement(ref this.m_numBusyThreads);
                this.OnThreadEvent(CallbackReason.RemovingThreadFromPool);
                Interlocked.Decrement(ref this.m_numThreadsInPool);
            }
        }

        private bool ThreadHasIoPending(Thread t)
        {
            bool result;
            if (!CallbackThreadPool.GetThreadIOPendingFlag(IntPtr.Zero, out result))
            {
                throw new Exception();
            }
            return result;
        }

        [DllImport("Kernel32")]
        private static extern bool GetThreadIOPendingFlag(IntPtr hThread, out bool fIsIoPending);

        protected long IncreaseMaximumThreadsInPool(int increment)
        {
            return (long)Interlocked.Add(ref this.m_maxThreadsAllowed, increment);
        }
    }
}