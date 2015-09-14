using System;
using System.Reflection;

namespace Wintellect.Threading.AsyncProgModel
{
    public class AsyncResultReflection<TResult> : AsyncResult<TResult>
    {
        private readonly MethodInfo m_mi;
        private readonly object m_target;
        private readonly object[] m_args;

        public AsyncResultReflection(AsyncCallback ac, object state, object target, MethodInfo mi, params object[] args)
            : base(ac, state)
        {
            this.m_target = target;
            this.m_mi = mi;
            this.m_args = args;
            base.BeginInvokeOnWorkerThread();
        }

        protected override TResult OnCompleteOperation(IAsyncResult ar)
        {
            return (TResult)((object)this.m_mi.Invoke(this.m_target, this.m_args));
        }
    }
}