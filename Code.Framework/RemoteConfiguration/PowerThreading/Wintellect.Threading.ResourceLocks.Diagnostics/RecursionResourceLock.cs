using System;
using System.Threading;

namespace Wintellect.Threading.ResourceLocks.Diagnostics
{
    public class RecursionResourceLock : DiagnosticResourceLock
    {
        private struct ThreadIdAndRecurseCount
        {
            public int m_Id;
            public int m_Count;

            public override string ToString()
            {
                return string.Format("Id={0}, Count={1}", this.m_Id, this.m_Count);
            }
        }

        private RecursionResourceLock.ThreadIdAndRecurseCount m_WriterThreadIdAndRecurseCount;
        private RecursionResourceLock.ThreadIdAndRecurseCount[] m_ReaderThreadIdsAndRecurseCounts;

        public RecursionResourceLock(IResourceLock resLock, int maxReaders)
            : base(resLock)
        {
            this.m_ReaderThreadIdsAndRecurseCounts = new RecursionResourceLock.ThreadIdAndRecurseCount[maxReaders];
        }

        private bool TryFindThreadIdIndex(int threadId, out int index)
        {
            for (index = 0; index < this.m_ReaderThreadIdsAndRecurseCounts.Length; index++)
            {
                if (this.m_ReaderThreadIdsAndRecurseCounts[index].m_Id == threadId)
                {
                    return true;
                }
            }
            return false;
        }

        private void AddThreadIdWithRecurseCountOf1(int callingThreadId)
        {
            for (int i = 0; i < this.m_ReaderThreadIdsAndRecurseCounts.Length; i++)
            {
                if (this.m_ReaderThreadIdsAndRecurseCounts[i].m_Id == 0)
                {
                    if (InterlockedEx.IfThen(ref this.m_ReaderThreadIdsAndRecurseCounts[i].m_Id, 0, callingThreadId))
                    {
                        this.m_ReaderThreadIdsAndRecurseCounts[i].m_Count = 1;
                        return;
                    }
                    i = -1;
                }
            }
            throw new InvalidOperationException("More current reader threads than allowed!");
        }

        protected override void OnWaitToWrite()
        {
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            if (managedThreadId == this.m_WriterThreadIdAndRecurseCount.m_Id)
            {
                this.m_WriterThreadIdAndRecurseCount.m_Count = this.m_WriterThreadIdAndRecurseCount.m_Count + 1;
                return;
            }
            base.InnerLock.WaitToWrite();
            Interlocked.Exchange(ref this.m_WriterThreadIdAndRecurseCount.m_Id, managedThreadId);
            this.m_WriterThreadIdAndRecurseCount.m_Count = 0;
        }

        protected override void OnDoneWriting()
        {
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            if (this.m_WriterThreadIdAndRecurseCount.m_Id != managedThreadId)
            {
                throw new InvalidOperationException("Calling thread doesn't own this lock for writing!");
            }
            if ((this.m_WriterThreadIdAndRecurseCount.m_Count = this.m_WriterThreadIdAndRecurseCount.m_Count - 1) > 0)
            {
                return;
            }
            Interlocked.Exchange(ref this.m_WriterThreadIdAndRecurseCount.m_Id, 0);
            base.InnerLock.DoneWriting();
        }

        protected override void OnWaitToRead()
        {
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            int num;
            if (this.TryFindThreadIdIndex(managedThreadId, out num))
            {
                RecursionResourceLock.ThreadIdAndRecurseCount[] expr_22_cp_0 = this.m_ReaderThreadIdsAndRecurseCounts;
                int expr_22_cp_1 = num;
                expr_22_cp_0[expr_22_cp_1].m_Count = expr_22_cp_0[expr_22_cp_1].m_Count + 1;
                return;
            }
            base.InnerLock.WaitToRead();
            this.AddThreadIdWithRecurseCountOf1(managedThreadId);
        }

        protected override void OnDoneReading()
        {
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            int num;
            if (!this.TryFindThreadIdIndex(managedThreadId, out num))
            {
                throw new InvalidOperationException("Calling thread doesn't own the lock for reading!");
            }
            RecursionResourceLock.ThreadIdAndRecurseCount[] expr_2D_cp_0 = this.m_ReaderThreadIdsAndRecurseCounts;
            int expr_2D_cp_1 = num;
            if ((expr_2D_cp_0[expr_2D_cp_1].m_Count = expr_2D_cp_0[expr_2D_cp_1].m_Count - 1) == 0)
            {
                Interlocked.Exchange(ref this.m_ReaderThreadIdsAndRecurseCounts[num].m_Id, 0);
                base.InnerLock.DoneReading();
            }
        }
    }
}