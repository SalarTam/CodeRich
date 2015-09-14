using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Xml;

namespace Code.Configuration
{
    public class LocalConfigurationManager : BaseConfigurationManager
    {
        private static LocalConfigurationManager instance;
        private static string localBaseConfigFolder;
        private static System.Configuration.Configuration _systemConfig;
        private static XmlDocument _systemConfigXml;
        public static LocalConfigurationManager Instance
        {
            get
            {
                return LocalConfigurationManager.instance;
            }
        }
        public static string LocalBaseConfigFolder
        {
            get
            {
                return LocalConfigurationManager.localBaseConfigFolder;
            }
        }
        internal static System.Configuration.Configuration LocalMainConfig
        {
            get
            {
                return LocalConfigurationManager._systemConfig;
            }
        }
        internal static XmlDocument SystemConfigXml
        {
            get
            {
                return LocalConfigurationManager._systemConfigXml;
            }
        }
        static LocalConfigurationManager()
        {
            LocalConfigurationManager._systemConfig = LocalConfigurationManager.GetExeConfig();
            LocalConfigurationManager.localBaseConfigFolder = System.IO.Path.GetDirectoryName(LocalConfigurationManager._systemConfig.FilePath);
            LocalConfigurationManager.instance = new LocalConfigurationManager();
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(LocalConfigurationManager._systemConfig.FilePath))
            {
                doc.Load(LocalConfigurationManager._systemConfig.FilePath);
            }
            else
            {
                doc.LoadXml("<configuration/>");
            }
            LocalConfigurationManager._systemConfigXml = doc;
        }
        private LocalConfigurationManager()
        {
        }
        public static string Combine(string folder, string file)
        {
            return ConfigUtility.Combine(folder, file);
        }
        public static string MapConfigPath(string fileName)
        {
            return LocalConfigurationManager.Combine(LocalConfigurationManager.localBaseConfigFolder, fileName);
        }
        private static System.Configuration.Configuration GetExeConfig()
        {
            string AppVirtualPath = HttpRuntime.AppDomainAppVirtualPath;
            if (AppVirtualPath != null && 0 < AppVirtualPath.Length)
            {
                return WebConfigurationManager.OpenWebConfiguration(AppVirtualPath);
            }
            return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }
        private static string GetSectionConfigSource(string name)
        {
            XmlNodeList nodeList = LocalConfigurationManager._systemConfigXml.DocumentElement.GetElementsByTagName(name);
            if (nodeList.Count == 0)
            {
                return string.Empty;
            }
            XmlElement elm = (XmlElement)nodeList[0];
            return elm.GetAttribute("configSource");
        }
        private static string GetConfigSectionFileName(string name)
        {
            string configSource = LocalConfigurationManager.GetSectionConfigSource(name);
            string folder = System.IO.Path.GetDirectoryName(LocalConfigurationManager._systemConfig.FilePath);
            if (configSource.Length == 0)
            {
                return "";
            }
            return LocalConfigurationManager.Combine(folder, configSource);
        }
        protected override object OnCreate(string sectionName, System.Type type, out int major, out int minor)
        {
            major = XmlSerializerSectionHandler.GetConfigurationClassMajorVersion(type);
            minor = 0;
            string configPath = LocalConfigurationManager.GetConfigSectionFileName(sectionName);
            if (configPath.Length == 0)
            {
                return null;
            }
            object retVal;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(configPath);
                retVal = XmlSerializerSectionHandler.GetConfigInstance(doc.DocumentElement, type);
                XmlSerializerSectionHandler.GetConfigVersion(doc.DocumentElement, out major, out minor);
            }
            catch (System.Exception ex)
            {
                BaseConfigurationManager.HandleException(ex, string.Concat(new string[]
                {
                    "Error when create local configuration: sectionName=",
                    sectionName,
                    ",type=",
                    type.Name,
                    ", create entry config instead"
                }), sectionName);
                retVal = System.Activator.CreateInstance(type);
            }
            XmlSerializerSectionHandler.SetupWatcher(configPath, retVal);
            return retVal;
        }
    }
}