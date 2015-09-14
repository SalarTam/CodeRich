using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Code.Configuration
{
    [XmlRoot("appSettings")]
    public class AppSettingCollection : IPostSerializer
    {
        private const string SectionName = "appSettings";
        private static AppSettingCollection instance;
        private static System.EventHandler _handler;
        private NameValueCollection collection = new NameValueCollection();
        [XmlElement("add")]
        public AppSettingEntry[] Entries;
        public static AppSettingCollection Instance
        {
            get
            {
                return AppSettingCollection.instance;
            }
            set
            {
                AppSettingCollection.instance = value;
                if (AppSettingCollection._handler != null)
                {
                    AppSettingCollection._handler(value, System.EventArgs.Empty);
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
        static AppSettingCollection()
        {
            AppSettingCollection.instance = RemoteConfigurationManager.Instance.GetSection<AppSettingCollection>("appSettings");
        }
        public static void RegisterConfigChangedNotification(System.EventHandler handler)
        {
            AppSettingCollection._handler = (System.EventHandler)System.Delegate.Combine(AppSettingCollection._handler, handler);
        }
        public void PostSerializer()
        {
            if (this.Entries != null)
            {
                AppSettingEntry[] entries = this.Entries;
                for (int i = 0; i < entries.Length; i++)
                {
                    AppSettingEntry entry = entries[i];
                    this.collection[entry.Key] = entry.Value;
                }
            }
            foreach (string key in ConfigurationManager.AppSettings)
            {
                this.collection[key] = ConfigurationManager.AppSettings[key];
            }
        }
    }
}
