using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Configuration
{
    public class FileWatcher
    {
        private object dirsLock;
        private System.Collections.Generic.Dictionary<string, DirectoryWatcher> directories;
        private static FileWatcher instance = new FileWatcher();
        public static FileWatcher Instance
        {
            get
            {
                return FileWatcher.instance;
            }
        }
        protected FileWatcher()
        {
            this.dirsLock = new object();
            this.directories = new System.Collections.Generic.Dictionary<string, DirectoryWatcher>();
        }
        public void AddFile(string filePath, System.EventHandler handler)
        {
            string dir = System.IO.Path.GetDirectoryName(filePath).ToLower();
            string fileName = System.IO.Path.GetFileName(filePath).ToLower();
            DirectoryWatcher watcher;
            lock (this.dirsLock)
            {
                if (!this.directories.TryGetValue(dir, out watcher))
                {
                    watcher = new DirectoryWatcher(dir);
                    this.directories.Add(dir, watcher);
                }
            }
            watcher.AddFile(fileName, handler);
        }
        internal void ProcessFileChanged(string filePath)
        {
            string dir = System.IO.Path.GetDirectoryName(filePath).ToLower();
            string fileName = System.IO.Path.GetFileName(filePath).ToLower();
            DirectoryWatcher watcher;
            lock (this.dirsLock)
            {
                if (!this.directories.TryGetValue(dir, out watcher))
                {
                    return;
                }
            }
            watcher.ProcessFileChanged(fileName);
        }
    }
}
