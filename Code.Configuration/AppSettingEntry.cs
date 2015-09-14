using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Code.Configuration
{
    public class AppSettingEntry
    {
        [XmlAttribute("key")]
        public string Key;
        [XmlAttribute("value")]
        public string Value;
    }
}
