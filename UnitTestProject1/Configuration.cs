using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using Code.Configuration;
using UnitTestProject1.Config;

namespace UnitTestProject1
{
    [TestClass]
    public class Configuration
    {
        [TestMethod]
        public void AppSettingUtilityTest()
        {
            var code = AppSettingUtility.FullTimeSearchHotKeywords;

            var environment = AppSettingUtility.environment;

            var memcached = RemoteConfigurationManager.Instance.GetSection<MemCachedCollection>("memcachedSettings");

            var ss = "";
        }
        [TestMethod]
        public void DbConnctionUtilityTest()
        {
            var evaregdb = CampusConnectionUtility.Evareg;
            RemoteConfigurationManager.Instance.InvalidConfig("campusConnections", 1);
            var ss = "";
        }
    }
}
