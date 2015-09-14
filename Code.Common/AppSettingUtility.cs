using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Code.Configuration;

namespace Code.Common
{
    public class AppSettingUtility
    {
        public static string GetRemoteAppSettingConfig(string key)
        {
            return AppSettingCollection.Instance[key];
            //return AppSettingProvider.GetAppSetting(key);
        }

        /// <summary>
        /// 是否开启代理
        /// </summary>
        public static bool IsOpenProxy
        {
            get
            {
                var isOpenProxy = GetRemoteAppSettingConfig("IsOpenProxy");
                return !string.IsNullOrEmpty(isOpenProxy);
            }
        }

        public static string IsOpenProxystr
        {
            get
            {
                var isOpenProxy = GetRemoteAppSettingConfig("IsOpenProxystr");
                return isOpenProxy;
            }
        }
    }
}
