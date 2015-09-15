using Code.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UnitTestProject1.Config
{
    [XmlRoot("CampusConnections")]
    public class CampusConnectionCollection : IPostSerializer
    {
        [XmlElement("add")]
        public CampusConnectionEntity[] Entries;
        private const string SectionName = "campusConnections";
        private static CampusConnectionCollection instance;
        private static System.EventHandler _handler;
        private NameValueCollection collection = new NameValueCollection();

        public static CampusConnectionCollection Instance
        {
            get
            {
                return CampusConnectionCollection.instance;
            }
            set
            {
                CampusConnectionCollection.instance = value;
                if (CampusConnectionCollection._handler != null)
                {
                    CampusConnectionCollection._handler(value, System.EventArgs.Empty);
                }
            }
        }

        [XmlIgnore]
        public string this[string key]
        { 
            get
            {
                return this.collection[key];
            }
        }

        static CampusConnectionCollection()
        {
            CampusConnectionCollection.instance = RemoteConfigurationManager.Instance.GetSection<CampusConnectionCollection>(SectionName);
        }

        public void PostSerializer()
        {
            if (this.Entries != null)
            {
                CampusConnectionEntity[] entries = this.Entries;
                for (int i = 0; i < entries.Length; i++)
                {
                    CampusConnectionEntity entry = entries[i];
                    this.collection[entry.Name] = entry.ConnectionString;
                }
            }
            //foreach (string key in ConfigurationManager.AppSettings)
            //{
            //    this.collection[key] = ConfigurationManager.AppSettings[key];
            //}
        }
    }
}
