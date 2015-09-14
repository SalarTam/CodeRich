using System;

namespace Wintellect.Threading.ResourceLocks
{
    public interface IResourceLock : IDisposable
    {
        IDisposable WaitToWrite();

        void DoneWriting();

        IDisposable WaitToRead();

        void DoneReading();
    }
}