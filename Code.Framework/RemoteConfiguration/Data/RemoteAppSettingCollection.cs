using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml.Serialization;

namespace Code.Framework.RemoteConfiguration
{
    public class AppSettingEntry
    {
        [XmlAttribute("key")]
        public string Key;

        [XmlAttribute("value")]
        public string Value;
    }

    [XmlRoot(RemoteAppSettingCollection.SectionName)]
    public class RemoteAppSettingCollection : IPostSerializer
    {
        private const string SectionName = "appSettings";

        static RemoteAppSettingCollection()
        {
            instance = RemoteConfigurationManager.Instance.GetSection<RemoteAppSettingCollection>(SectionName);
        }

        private static RemoteAppSettingCollection instance;

        public static RemoteAppSettingCollection Instance
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

        private static EventHandler _handler;

        public static void RegisterConfigChangedNotification(EventHandler handler)
        {
            _handler += handler;
        }

        private NameValueCollection collection = new NameValueCollection();

        [XmlElement("add")]
        public AppSettingEntry[] Entries;

        [XmlIgnore]
        public string this[string key]
        {
            get
            {
                return collection[key];
            }
        }

        #region IPostSerializer 成员

        public void PostSerializer()
        {
            if (Entries != null)
            {
                foreach (AppSettingEntry entry in Entries)
                {
                    collection[entry.Key] = entry.Value;
                }
            }

            //merge together
            foreach (string key in ConfigurationManager.AppSettings)
            {
                collection[key] = ConfigurationManager.AppSettings[key];
            }
        }

        #endregion IPostSerializer 成员
    }
}