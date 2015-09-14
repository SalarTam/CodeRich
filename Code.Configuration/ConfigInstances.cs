using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Configuration
{
    internal class ConfigInstances
    {
        private SafeReaderWriterLock configLock = new SafeReaderWriterLock();
        private System.Collections.Generic.Dictionary<string, object> configInstances = new System.Collections.Generic.Dictionary<string, object>();
        public object this[string path]
        {
            get
            {
                if (path == null)
                {
                    return null;
                }
                path = path.ToLower();
                object objRet;
                using (this.configLock.AcquireReaderLock())
                {
                    this.configInstances.TryGetValue(path, out objRet);
                }
                return objRet;
            }
        }
        public bool ContainsKey(string path)
        {
            if (path == null)
            {
                return false;
            }
            path = path.ToLower();
            bool contains;
            using (this.configLock.AcquireReaderLock())
            {
                contains = this.configInstances.ContainsKey(path);
            }
            return contains;
        }
        public bool Add(string path, object obj)
        {
            if (path == null)
            {
                return false;
            }
            path = path.ToLower();
            bool added;
            using (this.configLock.AcquireWriterLock())
            {
                if (this.configInstances.ContainsKey(path))
                {
                    added = false;
                }
                else
                {
                    this.configInstances.Add(path, obj);
                    added = true;
                }
            }
            return added;
        }
    }
}
