using System;
using Wintellect.Threading.AsyncProgModel;

namespace Wintellect.Threading
{
    public sealed class SoaLockReleaser : IDisposable
    {
        private SoaLockCallback m_Callback;
        private SoaReadWriteLock m_Rwl;
        private bool m_Reader;
        private object m_State;
        private AsyncResultNoReturn m_AsyncResult;

        public SoaReadWriteLock Lock
        {
            get
            {
                return this.m_Rwl;
            }
        }

        public object State
        {
            get
            {
                return this.m_State;
            }
        }

        internal SoaLockReleaser(SoaLockCallback callback, SoaReadWriteLock rwl, bool reader)
        {
            this.m_Callback = callback;
            this.m_Rwl = rwl;
            this.m_Reader = reader;
        }

        internal SoaLockReleaser(SoaLockCallback callback, SoaReadWriteLock rwl, bool reader, object state)
            : this(callback, rwl, reader)
        {
            this.m_State = state;
        }

        internal SoaLockReleaser(SoaLockCallback callback, SoaReadWriteLock rwl, bool reader, object state, AsyncResultNoReturn ar)
            : this(callback, rwl, reader, state)
        {
            this.m_AsyncResult = ar;
        }

        internal void Invoke(object o)
        {
            this.Invoke();
        }

        internal void Invoke()
        {
            try
            {
                if (this.m_AsyncResult == null)
                {
                    this.m_Callback(this);
                }
                else
                {
                    Exception exception = null;
                    try
                    {
                        this.m_Callback(this);
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                    finally
                    {
                        this.m_AsyncResult.SetAsCompleted(exception, false);
                    }
                }
            }
            finally
            {
                if (this != null)
                {
                    ((IDisposable)this).Dispose();
                }
            }
        }

        public void Release()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (this.m_Rwl == null)
            {
                return;
            }
            if (this.m_Reader)
            {
                this.m_Rwl.ReleaseReader();
            }
            else
            {
                this.m_Rwl.ReleaseWriter();
            }
            this.m_Rwl = null;
        }
    }
}