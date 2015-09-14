using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Configuration
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
    public class ConfigurationVersionAttribute : System.Attribute
    {
        private int majorVersion;
        public int MajorVersion
        {
            get
            {
                return this.majorVersion;
            }
            set
            {
                this.majorVersion = value;
            }
        }
        public ConfigurationVersionAttribute()
        {
            this.majorVersion = 1;
        }
        public ConfigurationVersionAttribute(int majorVersion)
        {
            this.majorVersion = majorVersion;
        }
    }
}
