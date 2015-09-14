using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Configuration
{
    public abstract class BaseConfigurationManager
    {
        internal System.Collections.Generic.Dictionary<string, ConfigEntry> configEntries;
        protected object configLocker;
        protected BaseConfigurationManager()
        {
            this.configEntries = new System.Collections.Generic.Dictionary<string, ConfigEntry>();
            this.configLocker = new object();
        }
        protected abstract object OnCreate(string sectionName, System.Type type, out int major, out int minor);
        internal ConfigEntry GetEntry(string sectionName)
        {
            sectionName = sectionName.ToLower();
            ConfigEntry entry;
            lock (this.configLocker)
            {
                this.configEntries.TryGetValue(sectionName, out entry);
            }
            return entry;
        }
        public T GetSection<T>(string section)
        {
            string sectionName = section.ToLower();
            ConfigEntry entry = this.GetEntry(sectionName);
            if (entry == null)
            {
                lock (this.configLocker)
                {
                    if (!this.configEntries.TryGetValue(sectionName, out entry))
                    {
                        entry = new ConfigEntry(section, typeof(T), new ConfigEntry.CreateObjectDelegate(this.OnCreate));
                        this.configEntries.Add(sectionName, entry);
                    }
                }
            }
            return (T)((object)entry.Value);
        }
        public static void HandleException(System.Exception ex, string msg, string sectionName)
        {
        }
        public static void Log(string msg)
        {
        }
    }
}
