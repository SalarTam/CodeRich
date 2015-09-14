using System;
using System.Reflection;
using System.Threading;

namespace Wintellect.Threading.AsyncProgModel
{
    public class AsyncResultNoReturn : IAsyncResult
    {
        private const int c_StatePending = 0;
        private const int c_StateCompletedSynchronously = 1;
        private const int c_StateCompletedAsynchronously = 2;
        private readonly AsyncCallback m_AsyncCallback;
        private readonly object m_AsyncState;
        private int m_CompletedState;
        private ManualResetEvent m_AsyncWaitHandle;
        private Exception m_exception;
        private static AsyncCallback s_AsyncCallbackHelper = new AsyncCallback(AsyncResultNoReturn.AsyncCallbackCompleteOpHelperNoReturnValue);
        private static WaitCallback s_WaitCallbackHelper = new WaitCallback(AsyncResultNoReturn.WaitCallbackCompleteOpHelperNoReturnValue);

        public object AsyncState
        {
            get
            {
                return this.m_AsyncState;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return this.m_CompletedState == 1;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (this.m_AsyncWaitHandle == null)
                {
                    bool isCompleted = this.IsCompleted;
                    ManualResetEvent manualResetEvent = new ManualResetEvent(isCompleted);
                    if (Interlocked.CompareExchange<ManualResetEvent>(ref this.m_AsyncWaitHandle, manualResetEvent, null) != null)
                    {
                        manualResetEvent.Close();
                    }
                    else
                    {
                        if (!isCompleted && this.IsCompleted)
                        {
                            this.m_AsyncWaitHandle.Set();
                        }
                    }
                }
                return this.m_AsyncWaitHandle;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return this.m_CompletedState != 0;
            }
        }

        public AsyncResultNoReturn(AsyncCallback asyncCallback, object state)
        {
            this.m_AsyncCallback = asyncCallback;
            this.m_AsyncState = state;
        }

        public void SetAsCompleted(Exception exception, bool completedSynchronously)
        {
            this.m_exception = exception;
            int num = Interlocked.Exchange(ref this.m_CompletedState, completedSynchronously ? 1 : 2);
            if (num != 0)
            {
                throw new InvalidOperationException("You can set a result only once");
            }
            if (this.m_AsyncWaitHandle != null)
            {
                this.m_AsyncWaitHandle.Set();
            }
            if (this.m_AsyncCallback != null)
            {
                this.m_AsyncCallback(this);
            }
        }

        public void EndInvoke()
        {
            if (!this.IsCompleted)
            {
                this.AsyncWaitHandle.WaitOne();
                this.AsyncWaitHandle.Close();
                this.m_AsyncWaitHandle = null;
            }
            if (this.m_exception != null)
            {
                throw this.m_exception;
            }
        }

        protected static AsyncCallback GetAsyncCallbackHelper()
        {
            return AsyncResultNoReturn.s_AsyncCallbackHelper;
        }

        protected IAsyncResult BeginInvokeOnWorkerThread()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(AsyncResultNoReturn.s_WaitCallbackHelper, this);
            return this;
        }

        private static void AsyncCallbackCompleteOpHelperNoReturnValue(IAsyncResult otherAsyncResult)
        {
            AsyncResultNoReturn asyncResultNoReturn = (AsyncResultNoReturn)otherAsyncResult.AsyncState;
            asyncResultNoReturn.CompleteOpHelper(otherAsyncResult);
        }

        private static void WaitCallbackCompleteOpHelperNoReturnValue(object o)
        {
            AsyncResultNoReturn asyncResultNoReturn = (AsyncResultNoReturn)o;
            asyncResultNoReturn.CompleteOpHelper(null);
        }

        private void CompleteOpHelper(IAsyncResult ar)
        {
            Exception exception = null;
            try
            {
                this.OnCompleteOperation(ar);
            }
            catch (TargetInvocationException ex)
            {
                exception = ex.InnerException;
            }
            catch (Exception ex2)
            {
                exception = ex2;
            }
            finally
            {
                this.SetAsCompleted(exception, false);
            }
        }

        protected virtual void OnCompleteOperation(IAsyncResult ar)
        {
        }
    }
}