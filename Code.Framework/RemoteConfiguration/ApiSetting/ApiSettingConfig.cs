using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Code.Framework.RemoteConfiguration.ApiSetting
{
    [XmlRoot("ApiSettingConfig")]
    public class ApiSettingConfig
    {
        [XmlElement("ApiSite")]
        public List<ApiSite> ApiSites {get;set;}
    }

    public class ApiSite
    {
        public ApiSite() { }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Value")]
        public string Value { get; set; }

        [XmlElement("Api")]
        public List<Api> Apis { get; set; }
    }

    public class Api
    {
        public Api() { }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Value")]
        public string Value { get; set; }

        [XmlElement("Parameter")]
        public List<Parameter> Parameters{get;set;}
       
    }

    public class Parameter 
    {
        public Parameter() { }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("DataType")]
        public string DataType { get; set; }

        [XmlAttribute("Required")]
        public int Required { get; set; }
    }
}
