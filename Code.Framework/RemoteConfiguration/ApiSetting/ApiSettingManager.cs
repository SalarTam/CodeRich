using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Code.Framework.RemoteConfiguration.ApiSetting
{
    public class ApiSettingManager
    {
        private const string SectionName = "ApiSettingConfig";
        private static ApiSettingConfig instance;


        static ApiSettingManager()
        {
            instance = RemoteConfigurationManager.Instance.GetSection<ApiSettingConfig>(SectionName);
        }
        public static ApiSettingConfig Instance
        {
            get
            {
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        public static ApiSite GetApiSite(string name)
        {
            var config = ApiSettingManager.instance;

            return (from item in config.ApiSites
                    where item.Name == name
                    select item).FirstOrDefault();

            //return new ApiSite();
        }
    }
}
