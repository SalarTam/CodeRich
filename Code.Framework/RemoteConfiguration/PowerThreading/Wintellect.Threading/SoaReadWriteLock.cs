using System;
using System.Collections.Generic;
using System.Threading;
using Wintellect.Threading.AsyncProgModel;

namespace Wintellect.Threading
{
    public sealed class SoaReadWriteLock
    {
        private const int c_LockIsFree = 0;
        private const int c_LockIsOwnedByWriter = 1;
        private const int c_LockIsOwnedByReaders = 2;
        private const int c_LockIsOwnedByReadersAndWritersPending = 3;
        private const int c_QueuesAreOwned = 4;
        private int m_LockState;
        private int m_NumReadersReading;
        private static readonly bool IsSingleCPUMachine = Environment.ProcessorCount == 1;
        private Queue<SoaLockReleaser> m_qWriters = new Queue<SoaLockReleaser>();
        private Queue<SoaLockReleaser> m_qReaders = new Queue<SoaLockReleaser>();

        public void ReadFromResource(SoaLockCallback callback)
        {
            this.ReadFromResource(callback, null);
        }

        public void ReadFromResource(SoaLockCallback callback, object state)
        {
            this.ReadFromResource(new SoaLockReleaser(callback, this, true, state));
        }

        public IAsyncResult BeginReadFromResource(SoaLockCallback callback, object state, AsyncCallback asyncCallback, object asyncState)
        {
            AsyncResultNoReturn asyncResultNoReturn = new AsyncResultNoReturn(asyncCallback, asyncState);
            this.ReadFromResource(new SoaLockReleaser(callback, this, true, state, asyncResultNoReturn));
            return asyncResultNoReturn;
        }

        public void EndReadFromResource(IAsyncResult ar)
        {
            this.EndInvoke(ar);
        }

        private void ReadFromResource(SoaLockReleaser releaser)
        {
            while (!SoaReadWriteLock.IfThen(ref this.m_LockState, 0, 2) && this.m_LockState != 2)
            {
                if (SoaReadWriteLock.IfThen(ref this.m_LockState, 3, 4))
                {
                    this.m_qReaders.Enqueue(releaser);
                    Interlocked.Exchange(ref this.m_LockState, 3);
                    return;
                }
                if (SoaReadWriteLock.IfThen(ref this.m_LockState, 1, 4))
                {
                    this.m_qReaders.Enqueue(releaser);
                    Interlocked.Exchange(ref this.m_LockState, 1);
                    return;
                }
                SoaReadWriteLock.StallThread();
            }
            Interlocked.Increment(ref this.m_NumReadersReading);
            releaser.Invoke();
        }

        internal void ReleaseReader()
        {
            int value;
            while (!SoaReadWriteLock.IfThen(ref this.m_LockState, 2, 4, out value) && !SoaReadWriteLock.IfThen(ref this.m_LockState, 3, 4, out value))
            {
                SoaReadWriteLock.StallThread();
            }
            if (Interlocked.Decrement(ref this.m_NumReadersReading) > 0)
            {
                Interlocked.Exchange(ref this.m_LockState, value);
                return;
            }
            if (this.m_qWriters.Count > 0)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(this.m_qWriters.Dequeue().Invoke));
                Interlocked.Exchange(ref this.m_LockState, 1);
                return;
            }
            Interlocked.Exchange(ref this.m_LockState, 0);
        }

        public void WriteToResource(SoaLockCallback callback)
        {
            this.WriteToResource(callback, null);
        }

        public void WriteToResource(SoaLockCallback callback, object state)
        {
            this.WriteToResource(new SoaLockReleaser(callback, this, false, state));
        }

        public IAsyncResult BeginWriteToResource(SoaLockCallback callback, object state, AsyncCallback asyncCallback, object asyncState)
        {
            AsyncResultNoReturn asyncResultNoReturn = new AsyncResultNoReturn(asyncCallback, asyncState);
            this.WriteToResource(new SoaLockReleaser(callback, this, false, state, asyncResultNoReturn));
            return asyncResultNoReturn;
        }

        public void EndWriteToResource(IAsyncResult ar)
        {
            this.EndInvoke(ar);
        }

        private void EndInvoke(IAsyncResult ar)
        {
            ((AsyncResultNoReturn)ar).EndInvoke();
        }

        private void WriteToResource(SoaLockReleaser releaser)
        {
            while (!SoaReadWriteLock.IfThen(ref this.m_LockState, 0, 1))
            {
                if (SoaReadWriteLock.IfThen(ref this.m_LockState, 1, 4))
                {
                    this.m_qWriters.Enqueue(releaser);
                    Interlocked.Exchange(ref this.m_LockState, 1);
                    return;
                }
                if (SoaReadWriteLock.IfThen(ref this.m_LockState, 2, 4) || SoaReadWriteLock.IfThen(ref this.m_LockState, 3, 4))
                {
                    this.m_qWriters.Enqueue(releaser);
                    Interlocked.Exchange(ref this.m_LockState, 3);
                    return;
                }
                SoaReadWriteLock.StallThread();
            }
            releaser.Invoke();
        }

        internal void ReleaseWriter()
        {
            while (!SoaReadWriteLock.IfThen(ref this.m_LockState, 1, 4))
            {
                SoaReadWriteLock.StallThread();
            }
            if (this.m_qWriters.Count > 0)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(this.m_qWriters.Dequeue().Invoke));
                Interlocked.Exchange(ref this.m_LockState, 1);
                return;
            }
            if (this.m_qReaders.Count > 0)
            {
                Interlocked.Exchange(ref this.m_NumReadersReading, this.m_qReaders.Count);
                while (this.m_qReaders.Count > 0)
                {
                    System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(this.m_qReaders.Dequeue().Invoke));
                }
                Interlocked.Exchange(ref this.m_LockState, 2);
                return;
            }
            Interlocked.Exchange(ref this.m_LockState, 0);
        }

        private static void StallThread()
        {
            ThreadUtil.StallThread();
        }

        private static bool IfThen(ref int val, int @if, int then)
        {
            return InterlockedEx.IfThen(ref val, @if, then);
        }

        private static bool IfThen(ref int val, int @if, int then, out int prevVal)
        {
            return InterlockedEx.IfThen(ref val, @if, then, out prevVal);
        }
    }
}