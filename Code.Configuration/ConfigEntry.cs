using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Configuration
{
    internal class ConfigEntry
    {
        public delegate object CreateObjectDelegate(string sectionName, System.Type type, out int majorVersion, out int minorVersion);
        public string Name;
        private int major;
        private int minor;
        private bool isSet;
        private object locker;
        private System.Type type;
        private ConfigEntry.CreateObjectDelegate OnCreate;
        private object val;
        public int MajorVersion
        {
            get
            {
                return this.major;
            }
            set
            {
                this.major = value;
            }
        }
        public int MinorVersion
        {
            get
            {
                return this.minor;
            }
            set
            {
                this.minor = value;
            }
        }
        public System.Type EntryType
        {
            get
            {
                return this.type;
            }
        }
        public bool IsSet
        {
            get
            {
                return this.isSet;
            }
        }
        public object Value
        {
            get
            {
                if (!this.isSet)
                {
                    lock (this.locker)
                    {
                        if (!this.isSet)
                        {
                            this.val = this.OnCreate(this.Name, this.type, out this.major, out this.minor);
                            this.isSet = true;
                        }
                    }
                }
                return this.val;
            }
        }
        public ConfigEntry(string sectionName, System.Type type, ConfigEntry.CreateObjectDelegate creater)
        {
            this.Name = sectionName;
            this.type = type;
            this.isSet = false;
            this.locker = new object();
            this.OnCreate = creater;
        }
    }
}
