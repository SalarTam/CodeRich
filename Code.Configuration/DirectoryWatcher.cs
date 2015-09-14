using System;
using System.Collections.Generic;
using System.IO;
using Wintellect.Threading.AsyncProgModel;
using Wintellect.Threading.ResourceLocks;

namespace Code.Configuration
{
    internal sealed class DirectoryWatcher
    {
        private const int CHANGE_FILE_DELAY = 5000;
        private SafeReaderWriterLock filesLock;
        private System.Collections.Generic.Dictionary<string, System.EventHandler> files;
        private System.Collections.Generic.List<string> pendingFileReloads;
        private string directory;
        private static readonly ResourceLock fileLoadResourceLock = new OneManyResourceLock();
        public DirectoryWatcher(string directory)
        {
            this.filesLock = new SafeReaderWriterLock();
            this.files = new System.Collections.Generic.Dictionary<string, System.EventHandler>();
            this.directory = directory;
            this.pendingFileReloads = new System.Collections.Generic.List<string>();
            this.InitWatcher(directory);
        }
        private void InitWatcher(string directory)
        {
            FileSystemWatcher scareCrow = new FileSystemWatcher();
            scareCrow.Path = directory;
            scareCrow.IncludeSubdirectories = false;
            scareCrow.NotifyFilter = NotifyFilters.Attributes;
            scareCrow.Changed += new FileSystemEventHandler(this.scareCrow_Changed);
            scareCrow.EnableRaisingEvents = true;
        }
        public void AddFile(string fileName, System.EventHandler delegateMethod)
        {
            fileName = fileName.ToLower();
            using (this.filesLock.AcquireWriterLock())
            {
                if (!this.files.ContainsKey(fileName))
                {
                    this.files.Add(fileName, delegateMethod);
                }
            }
        }
        private System.EventHandler GetEventHandler(string fileName)
        {
            fileName = fileName.ToLower();
            System.EventHandler handler;
            using (this.filesLock.AcquireReaderLock())
            {
                this.files.TryGetValue(fileName, out handler);
            }
            return handler;
        }
        private bool ContainsFile(string fileName)
        {
            fileName = fileName.ToLower();
            bool contains;
            using (this.filesLock.AcquireReaderLock())
            {
                contains = this.files.ContainsKey(fileName);
            }
            return contains;
        }
        private void scareCrow_Changed(object sender, FileSystemEventArgs e)
        {
            string fileName = e.Name.ToLower();
            using (DirectoryWatcher.fileLoadResourceLock.WaitToWrite())
            {
                if (this.pendingFileReloads.Contains(fileName) || !this.ContainsFile(fileName))
                {
                    return;
                }
                this.pendingFileReloads.Add(fileName);
            }
            CountdownTimer timer = new CountdownTimer();
            timer.BeginCountdown(5000, new System.AsyncCallback(this.DelayedProcessFileChanged), fileName);
        }
        internal void ProcessFileChanged(string fileName)
        {
            System.EventHandler delegateMethod = this.GetEventHandler(fileName);
            if (delegateMethod != null)
            {
                try
                {
                    string filePath = this.directory + "\\" + fileName;
                    delegateMethod(filePath, System.EventArgs.Empty);
                }
                catch (System.Exception)
                {
                }
            }
        }
        private void DelayedProcessFileChanged(System.IAsyncResult ar)
        {
            string fileName = (string)ar.AsyncState;
            using (DirectoryWatcher.fileLoadResourceLock.WaitToWrite())
            {
                this.pendingFileReloads.Remove(fileName);
            }
            this.ProcessFileChanged(fileName);
        }
    }
}
