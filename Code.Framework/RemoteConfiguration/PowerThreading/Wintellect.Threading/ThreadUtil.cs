using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Wintellect.Threading
{
    public static class ThreadUtil
    {
        public static readonly bool IsSingleCpuMachine = Environment.ProcessorCount == 1;

        public static void StallThread()
        {
            if (ThreadUtil.IsSingleCpuMachine)
            {
                ThreadUtil.SwitchToThread();
                return;
            }
            Thread.SpinWait(1);
        }

        [DllImport("Kernel32", ExactSpelling = true)]
        public static extern bool SwitchToThread();
    }
}