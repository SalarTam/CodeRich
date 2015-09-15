using Code.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    public class AppSettingUtility
    {
        public static string GetRemoteAppSettingConfig(string key)
        {
            return AppSettingCollection.Instance[key];
        }
        public static string FullTimeSearchHotKeywords
        {
            get
            {
                return GetRemoteAppSettingConfig("FullTimeSearchHotKeywords");

            }
        }
        public static string environment
        {
            get
            {
                return GetRemoteAppSettingConfig("environment");

            }
        }
        
    }
}
