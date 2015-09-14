using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Code.Configuration
{
    public class RemoteConfigSectionCollection
    {
        [XmlAttribute("machine")]
        public string Machine;
        [XmlAttribute("application")]
        public string Application;
        [XmlElement("section")]
        public System.Collections.Generic.List<RemoteConfigSectionParam> Sections;
        public int Count
        {
            get
            {
                return this.Sections.Count;
            }
        }
        public RemoteConfigSectionParam this[int index]
        {
            get
            {
                return this.Sections[index];
            }
        }
        public void AddSection(string sectionName, int major, int minor)
        {
            this.AddSection(sectionName, major, minor, null);
        }
        public void AddSection(string sectionName, int major, int minor, string url)
        {
            RemoteConfigSectionParam param = new RemoteConfigSectionParam();
            param.SectionName = sectionName;
            param.MajorVersion = major;
            param.MinorVersion = minor;
            param.DownloadUrl = url;
            this.Sections.Add(param);
        }
        public RemoteConfigSectionCollection()
        {
            this.Machine = System.Environment.MachineName;
            this.Sections = new System.Collections.Generic.List<RemoteConfigSectionParam>();
        }
        public RemoteConfigSectionCollection(string appName) : this()
        {
            this.Application = appName;
        }
        public System.Collections.Generic.IEnumerator<RemoteConfigSectionParam> GetEnumerator()
        {
            return this.Sections.GetEnumerator();
        }
    }
}
