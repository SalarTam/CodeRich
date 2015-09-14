using System;
using System.Collections.Generic;
using Code.Framework.CampusLog;


namespace Code.Framework.RemoteConfiguration
{
    public abstract class BaseConfigurationManager
    {
        protected BaseConfigurationManager()
        {
            configEntries = new Dictionary<string, ConfigEntry>();
            configLocker = new object();
        }

        internal Dictionary<string, ConfigEntry> configEntries;
        protected object configLocker;

        protected abstract object OnCreate(string sectionName, Type type, out int major, out int minor);

        internal ConfigEntry GetEntry(string sectionName)
        {
            sectionName = sectionName.ToLower();
            ConfigEntry entry;
            lock (configLocker)
            {
                configEntries.TryGetValue(sectionName, out entry);
            }
            return entry;
        }

        public T GetSection<T>(string section)
        {
            string sectionName = section.ToLower();
            ConfigEntry entry = GetEntry(sectionName);
            if (entry == null)
            {
                lock (configLocker)
                {
                    if (!configEntries.TryGetValue(sectionName, out entry))
                    {
                        entry = new ConfigEntry(section, typeof(T), OnCreate);
                        configEntries.Add(sectionName, entry);
                    }
                }
            }
            return (T)entry.Value;
        }

        public T GetSection<T>(string section, out int major, out int minor)
        {
            string sectionName = section.ToLower();
            ConfigEntry entry = GetEntry(sectionName);
            if (entry == null)
            {
                lock (configLocker)
                {
                    if (!configEntries.TryGetValue(sectionName, out entry))
                    {
                        entry = new ConfigEntry(section, typeof(T), OnCreate);
                        configEntries.Add(sectionName, entry);
                    }
                }
            }
            major = entry.MajorVersion;
            minor = entry.MinorVersion;
            return (T)entry.Value;
        }

        public static void HandleException(Exception ex, string msg, string sectionName)
        {
            CampusLogHelper.Error(sectionName,string.Format("ex:{0},msg:{1},sectionName:{2}", ex.Message, msg, sectionName));
        }

        public static void Log(string msg)
        {
            CampusLogHelper.Error("RemoteConfig", msg);
        }
    }
}