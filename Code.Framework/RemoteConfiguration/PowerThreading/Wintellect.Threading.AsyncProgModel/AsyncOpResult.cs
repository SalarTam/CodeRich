using System;

namespace Wintellect.Threading.AsyncProgModel
{
    public class AsyncOpResult
    {
        private AsyncOpResultState m_AsyncOpResultState;
        private string m_Name;
        private Exception m_Exception;

        public AsyncOpResultState ResultState
        {
            get
            {
                return this.m_AsyncOpResultState;
            }
            protected set
            {
                this.m_AsyncOpResultState = value;
            }
        }

        public string Name
        {
            get
            {
                return this.m_Name;
            }
        }

        public Exception Exception
        {
            get
            {
                return this.m_Exception;
            }
            set
            {
                this.m_Exception = value;
                this.ResultState = AsyncOpResultState.Failed;
            }
        }

        public AsyncOpResult(string name)
        {
            this.m_Name = name;
        }
    }

    public sealed class AsyncOpResult<TState, TResult> : AsyncOpResult
    {
        private TState m_State;
        private TResult m_Result;

        public TState State
        {
            get
            {
                return this.m_State;
            }
        }

        public TResult Result
        {
            get
            {
                return this.m_Result;
            }
            set
            {
                this.m_Result = value;
                base.ResultState = AsyncOpResultState.Completed;
            }
        }

        public AsyncOpResult(string name)
            : this(name, default(TState))
        {
        }

        public AsyncOpResult(string name, TState state)
            : base(name)
        {
            this.m_State = state;
            this.m_Result = default(TResult);
        }
    }
}