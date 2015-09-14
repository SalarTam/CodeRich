using System;
using System.Reflection;
using System.Threading;

namespace Wintellect.Threading.AsyncProgModel
{
    public abstract class AsyncResult<TResult> : AsyncResultNoReturn
    {
        private TResult m_result;
        private static AsyncCallback s_AsyncCallbackHelper = new AsyncCallback(AsyncResult<TResult>.AsyncCallbackCompleteOpHelperWithReturnValue);
        private static WaitCallback s_WaitCallbackHelper = new WaitCallback(AsyncResult<TResult>.WaitCallbackCompleteOpHelperWithReturnValue);

        protected AsyncResult(AsyncCallback asyncCallback, object state)
            : base(asyncCallback, state)
        {
        }

        protected void SetAsCompleted(TResult result, bool completedSynchronously)
        {
            this.m_result = result;
            base.SetAsCompleted(null, completedSynchronously);
        }

        public new TResult EndInvoke()
        {
            base.EndInvoke();
            return this.m_result;
        }

        protected new static AsyncCallback GetAsyncCallbackHelper()
        {
            return AsyncResult<TResult>.s_AsyncCallbackHelper;
        }

        private static void AsyncCallbackCompleteOpHelperWithReturnValue(IAsyncResult otherAsyncResult)
        {
            AsyncResult<TResult> asyncResult = (AsyncResult<TResult>)otherAsyncResult.AsyncState;
            asyncResult.CompleteOpHelper(otherAsyncResult);
        }

        protected new IAsyncResult BeginInvokeOnWorkerThread()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(AsyncResult<TResult>.s_WaitCallbackHelper, this);
            return this;
        }

        private static void WaitCallbackCompleteOpHelperWithReturnValue(object o)
        {
            AsyncResult<TResult> asyncResult = (AsyncResult<TResult>)o;
            asyncResult.CompleteOpHelper(null);
        }

        private void CompleteOpHelper(IAsyncResult ar)
        {
            try
            {
                this.SetAsCompleted(this.OnCompleteOperation(ar), false);
            }
            catch (TargetInvocationException ex)
            {
                base.SetAsCompleted(ex.InnerException, false);
            }
            catch (Exception exception)
            {
                base.SetAsCompleted(exception, false);
            }
        }

        protected new virtual TResult OnCompleteOperation(IAsyncResult ar)
        {
            return default(TResult);
        }
    }
}