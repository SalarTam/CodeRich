using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;

namespace Code.Framework.RemoteConfiguration
{
    public class ConnectionStringEntry
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("connectionString")]
        public string ConnectionString;

        [XmlAttribute("providerName")]
        public string ProviderName;
    }

    [XmlRoot(RemoteConnectionStringCollection.SectionName)]
    public class RemoteConnectionStringCollection : IPostSerializer
    {
        private const string SectionName = "connectionStrings";
        private static RemoteConnectionStringCollection instance;

        static RemoteConnectionStringCollection()
        {
            instance = RemoteConfigurationManager.Instance.GetSection<RemoteConnectionStringCollection>(SectionName);
        }

        private static EventHandler _handler;

        public static void RegisterConfigChangedNotification(EventHandler handler)
        {
            _handler += handler;
        }

        public static RemoteConnectionStringCollection Instance
        {
            get
            {
                return instance;
            }
            set
            {
                instance = value;
                if (_handler != null)
                    _handler(value, EventArgs.Empty);
            }
        }

        [XmlElement]
        public bool EnabledDatabaseConnectivityState = false;

        [XmlElement("add")]
        public ConnectionStringEntry[] Entries;

        private NameValueCollection collection = new NameValueCollection();

        [XmlIgnore]
        public string this[string name]
        {
            get
            {
                return collection[name];
            }
        }

        public IEnumerator GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        #region IPostSerializer 成员

        /// <summary>
        /// combine local connectionString and remote connectionString together, local connection string will be the final if the name is conflict
        /// </summary>
        public void PostSerializer()
        {
            if (Entries != null)
            {
                foreach (ConnectionStringEntry entry in Entries)
                {
                    collection[entry.Name] = entry.ConnectionString;
                }
            }

            foreach (ConnectionStringSettings entry in ConfigurationManager.ConnectionStrings)
            {
                collection[entry.Name] = entry.ConnectionString;
            }
        }

        #endregion IPostSerializer 成员
    }
}