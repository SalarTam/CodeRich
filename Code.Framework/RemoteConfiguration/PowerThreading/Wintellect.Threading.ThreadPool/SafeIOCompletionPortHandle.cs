using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Wintellect.Threading.ThreadPool
{
    internal sealed class SafeIOCompletionPortHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeIOCompletionPortHandle()
            : base(true)
        {
        }

        public SafeIOCompletionPortHandle(IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            base.SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return SafeIOCompletionPortHandle.CloseHandle(this.handle);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr handle);
    }
}