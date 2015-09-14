using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Code.Configuration
{
    public class RemoteConfigSectionParam
    {
        [XmlAttribute("name")]
        public string SectionName;
        [XmlAttribute("majorVerion")]
        public int MajorVersion;
        [XmlAttribute("minorVerion")]
        public int MinorVersion;
        [XmlAttribute("downloadUrl")]
        public string DownloadUrl;
    }
}
