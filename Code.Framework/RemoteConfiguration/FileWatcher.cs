using System;
using System.Collections.Generic;
using System.IO;
using Wintellect.Threading.AsyncProgModel;
using Wintellect.Threading.ResourceLocks;

namespace Code.Framework.RemoteConfiguration
{
    internal sealed class DirectoryWatcher
    {
        private SafeReaderWriterLock filesLock;
        private Dictionary<string, EventHandler> files;
        private List<string> pendingFileReloads;
        private const int CHANGE_FILE_DELAY = 15000;
        private string directory;
        private static readonly ResourceLock fileLoadResourceLock = new OneManyResourceLock();

        public DirectoryWatcher(string directory)
        {
            filesLock = new SafeReaderWriterLock();
            files = new Dictionary<string, EventHandler>();
            this.directory = directory;
            pendingFileReloads = new List<string>();
            InitWatcher(directory);
        }

        /// <summary>
        /// call only once when DirectoryWatcher created
        /// </summary>
        /// <param name="directory"></param>
        private void InitWatcher(string directory)
        {
            FileSystemWatcher scareCrow = new FileSystemWatcher();
            scareCrow.Path = directory;
            scareCrow.IncludeSubdirectories = false;
            scareCrow.NotifyFilter = NotifyFilters.Attributes;

            scareCrow.Changed += new FileSystemEventHandler(scareCrow_Changed);
            scareCrow.EnableRaisingEvents = true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="delegateMethod">delegateMethod(string filePath,Empty), filePath is in lower case</param>
        public void AddFile(string fileName, EventHandler delegateMethod)
        {
            fileName = fileName.ToLower();

            using (filesLock.AcquireWriterLock())
            {
                if (!files.ContainsKey(fileName))
                    files.Add(fileName, delegateMethod);
            }
        }

        private EventHandler GetEventHandler(string fileName)
        {
            fileName = fileName.ToLower();
            EventHandler handler;

            using (filesLock.AcquireReaderLock())
            {
                files.TryGetValue(fileName, out handler);
            }
            return handler;
        }

        private bool ContainsFile(string fileName)
        {
            fileName = fileName.ToLower();
            bool contains;

            using (filesLock.AcquireReaderLock())
            {
                contains = files.ContainsKey(fileName);
            }
            return contains;
        }

        private void scareCrow_Changed(object sender, FileSystemEventArgs e)
        {
            string fileName = e.Name.ToLower();
            using (fileLoadResourceLock.WaitToWrite())
            {
                if (pendingFileReloads.Contains(fileName) || !ContainsFile(fileName))
                    return;

                pendingFileReloads.Add(fileName);
            }

            CountdownTimer timer = new CountdownTimer();
            timer.BeginCountdown(CHANGE_FILE_DELAY, DelayedProcessFileChanged, fileName);
        }

        internal void ProcessFileChanged(string fileName)
        {
            EventHandler delegateMethod = GetEventHandler(fileName);
            if (delegateMethod != null)
            {
                try
                {
                    string filePath = directory + "\\" + fileName;
                    delegateMethod(filePath, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void DelayedProcessFileChanged(IAsyncResult ar)
        {
            string fileName = (string)ar.AsyncState;

            using (fileLoadResourceLock.WaitToWrite())
            {
                pendingFileReloads.Remove(fileName);
            }

            //only one Handler for one file!!
            ProcessFileChanged(fileName);
        }
    }

    public class FileWatcher
    {
        private object dirsLock;
        private Dictionary<string, DirectoryWatcher> directories;

        private static FileWatcher instance = new FileWatcher();

        public static FileWatcher Instance
        {
            get
            {
                return instance;
            }
        }

        protected FileWatcher()
        {
            dirsLock = new object();
            directories = new Dictionary<string, DirectoryWatcher>();
        }

        public void AddFile(string filePath, EventHandler handler)
        {
            string dir = Path.GetDirectoryName(filePath).ToLower();
            string fileName = Path.GetFileName(filePath).ToLower();
            DirectoryWatcher watcher;
            lock (dirsLock)
            {
                if (!directories.TryGetValue(dir, out watcher))
                {
                    watcher = new DirectoryWatcher(dir);
                    directories.Add(dir, watcher);
                }
            }
            watcher.AddFile(fileName, handler);
        }

        internal void ProcessFileChanged(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath).ToLower();
            string fileName = Path.GetFileName(filePath).ToLower();
            DirectoryWatcher watcher;
            lock (dirsLock)
            {
                if (!directories.TryGetValue(dir, out watcher)) return;
            }
            watcher.ProcessFileChanged(fileName);
        }
    }
}