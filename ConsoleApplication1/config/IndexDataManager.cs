using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Code.Configuration;

namespace ConsoleApplication1.config
{
    public class IndexDataManager
    {
        private const string SectionName = "IndexData";
        private static readonly IndexData instance;
        static IndexDataManager()
        {
            instance = RemoteConfigurationManager.Instance.GetSection<IndexData>(SectionName);

        }

        public static IndexData CreateInstance()
        {
            return instance;
        }
    }
    [XmlRoot("IndexData")]
    public class IndexData
    {
        [XmlElement("city")]
        public List<CityGroup> GroupCity { get; set; }
    }

    public class CityGroup
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlElement("industry")]
        public List<IndustryGroup> GroupIndustry { get; set; }

    }

    public class IndustryGroup
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("cols")]
        public int Count { get; set; }
        [XmlText]
        public string ChildIds { get; set; }
    }
}
