using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Wintellect.Threading.ThreadPool
{
    public sealed class IOCompletionPort : IDisposable
    {
        private SafeIOCompletionPortHandle m_hIOCP;
        private static readonly SafeFileHandle c_InvalidFileHandle = new SafeFileHandle((IntPtr)(-1), true);
        private static readonly SafeIOCompletionPortHandle c_InvalidIOCompletionPortHandle = new SafeIOCompletionPortHandle(IntPtr.Zero, true);

        public IOCompletionPort(int maxConcurrency)
        {
            this.m_hIOCP = IOCompletionPort.CreateIoCompletionPort(maxConcurrency);
        }

        private static SafeIOCompletionPortHandle CreateIoCompletionPort(int numConcurrentThreads)
        {
            return IOCompletionPort.CreateIoCompletionPort(IOCompletionPort.c_InvalidFileHandle, IOCompletionPort.c_InvalidIOCompletionPortHandle, UIntPtr.Zero, (uint)numConcurrentThreads);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_hIOCP.Close();
            }
        }

        public void PostStatus(WaitCallback cb)
        {
            this.PostStatus(cb, null);
        }

        public void PostStatus(WaitCallback cb, object state)
        {
            if (cb == null)
            {
                throw new ArgumentNullException("cb", "delegate must not be null");
            }
            GCHandle value = GCHandle.Alloc(cb);
            GCHandle value2 = default(GCHandle);
            if (state != null)
            {
                value2 = GCHandle.Alloc(state);
            }
            if (!IOCompletionPort.PostQueuedCompletionStatus(this.m_hIOCP, 0u, (IntPtr)value, (state == null) ? IntPtr.Zero : ((IntPtr)value2)))
            {
                value.Free();
                if (state != null)
                {
                    value2.Free();
                }
                throw new OverflowException("Failed to post callback");
            }
        }

        public void GetStatus(int millseconds, out bool timedOut, out WaitCallback cb, out object state)
        {
            cb = null;
            state = null;
            uint num;
            IntPtr value;
            IntPtr intPtr;
            bool queuedCompletionStatus = IOCompletionPort.GetQueuedCompletionStatus(this.m_hIOCP, out num, out value, out intPtr, (uint)millseconds);
            int lastWin32Error = Marshal.GetLastWin32Error();
            timedOut = (!queuedCompletionStatus && intPtr == IntPtr.Zero && (long)lastWin32Error == 258L);
            if (timedOut)
            {
                return;
            }
            GCHandle gCHandle = (GCHandle)value;
            cb = (WaitCallback)gCHandle.Target;
            gCHandle.Free();
            if (intPtr != IntPtr.Zero)
            {
                GCHandle gCHandle2 = (GCHandle)intPtr;
                state = gCHandle2.Target;
                gCHandle2.Free();
            }
        }

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        private static extern SafeIOCompletionPortHandle CreateIoCompletionPort(SafeFileHandle hFile, SafeIOCompletionPortHandle hExistingIOCP, UIntPtr CompKey, uint NumConcurrentThreads);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        private static extern bool GetQueuedCompletionStatus(SafeIOCompletionPortHandle hIOCP, out uint numBytesTransferred, out IntPtr cb, out IntPtr state, uint milliseconds);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        private static extern bool PostQueuedCompletionStatus(SafeIOCompletionPortHandle hIOCP, uint numBytesTransferred, IntPtr cb, IntPtr state);
    }
}