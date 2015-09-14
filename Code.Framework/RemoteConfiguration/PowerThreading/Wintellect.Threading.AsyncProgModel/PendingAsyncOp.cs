using System;

namespace Wintellect.Threading.AsyncProgModel
{
    public class PendingAsyncOp
    {
        private MultiAsyncResult m_multiAsyncResult;
        private int m_ResultIndex;

        protected MultiAsyncResult MultiAsyncResult
        {
            get
            {
                return this.m_multiAsyncResult;
            }
        }

        protected int ResultIndex
        {
            get
            {
                return this.m_ResultIndex;
            }
        }

        public string Name
        {
            get
            {
                return this.m_multiAsyncResult.GetName(this.m_ResultIndex);
            }
        }

        internal PendingAsyncOp(MultiAsyncResult multiAsyncResult, int resultIndex)
        {
            this.m_multiAsyncResult = multiAsyncResult;
            this.m_ResultIndex = resultIndex;
        }

        public void SetResult(bool cancel)
        {
            this.m_multiAsyncResult.SetResult<object, object>(this.m_ResultIndex, null, cancel);
        }

        public void SetFailure(Exception e, bool cancel)
        {
            this.m_multiAsyncResult.SetFailure(this.m_ResultIndex, e, cancel);
        }
    }

    public class PendingAsyncOp<TAsyncObj> : PendingAsyncOp
    {
        private TAsyncObj m_AsyncObj;

        public TAsyncObj GetAsyncObj()
        {
            return this.m_AsyncObj;
        }

        internal PendingAsyncOp(MultiAsyncResult multiAsyncResult, int resultIndex, TAsyncObj asyncObj)
            : base(multiAsyncResult, resultIndex)
        {
            this.m_AsyncObj = asyncObj;
        }
    }

    public class PendingAsyncOp<TAsyncObj, TResult> : PendingAsyncOp<TAsyncObj>
    {
        internal PendingAsyncOp(MultiAsyncResult multiAsyncResult, int resultIndex, TAsyncObj asyncObj)
            : base(multiAsyncResult, resultIndex, asyncObj)
        {
        }

        public virtual void SetResult(TResult result, bool cancel)
        {
            base.MultiAsyncResult.SetResult<object, TResult>(base.ResultIndex, result, cancel);
        }
    }

    public sealed class PendingAsyncOp<TAsyncObj, TState, TResult> : PendingAsyncOp<TAsyncObj, TResult>
    {
        internal PendingAsyncOp(MultiAsyncResult multiAsyncResult, int resultIndex, TAsyncObj asyncObj)
            : base(multiAsyncResult, resultIndex, asyncObj)
        {
        }

        public TState GetState()
        {
            return base.MultiAsyncResult.GetState<TState, TResult>(base.ResultIndex);
        }

        public override void SetResult(TResult result, bool cancel)
        {
            base.MultiAsyncResult.SetResult<TState, TResult>(base.ResultIndex, result, cancel);
        }
    }
}