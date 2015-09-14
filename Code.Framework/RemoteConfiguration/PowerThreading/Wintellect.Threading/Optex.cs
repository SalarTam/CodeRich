using System;
using System.Threading;

namespace Wintellect.Threading
{
    public sealed class Optex : IDisposable
    {
        private const int c_lsFree = 0;
        private const int c_lsOwned = 1;
        private const int c_1Waiter = 2;
        private int m_LockState;
        private Semaphore m_WaiterLock = new Semaphore(0, 2147483647);

        public void Dispose()
        {
            this.m_WaiterLock.Close();
            this.m_WaiterLock = null;
        }

        public void Enter()
        {
            Thread.BeginCriticalRegion();
            while (true)
            {
                int num = Optex.InterlockedOr(ref this.m_LockState, 1);
                if ((num & 1) == 0)
                {
                    break;
                }
                if (Optex.IfThen(ref this.m_LockState, num, num + 2))
                {
                    this.m_WaiterLock.WaitOne();
                }
            }
        }

        public void Exit()
        {
            int num = Optex.InterlockedAnd(ref this.m_LockState, -2);
            if (num != 1)
            {
                num &= -2;
                if (Optex.IfThen(ref this.m_LockState, num & -2, num - 2))
                {
                    this.m_WaiterLock.Release(1);
                }
            }
            Thread.EndCriticalRegion();
        }

        private static int InterlockedAnd(ref int target, int with)
        {
            int num = target;
            int num2;
            do
            {
                num2 = num;
                num = Interlocked.CompareExchange(ref target, num2 & with, num2);
            }
            while (num2 != num);
            return num;
        }

        private static int InterlockedOr(ref int target, int with)
        {
            int num = target;
            int num2;
            do
            {
                num2 = num;
                num = Interlocked.CompareExchange(ref target, num2 | with, num2);
            }
            while (num2 != num);
            return num;
        }

        private static bool IfThen(ref int val, int @if, int then)
        {
            return Interlocked.CompareExchange(ref val, then, @if) == @if;
        }

        private static bool IfThen(ref int val, int @if, int then, out int prevVal)
        {
            prevVal = Interlocked.CompareExchange(ref val, then, @if);
            return prevVal == @if;
        }
    }
}