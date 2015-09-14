using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Code.Configuration
{
    [XmlRoot("RemoteConfigurationManager")]
    public class RemoteConfigurationManagerConfiguration : IXmlSerializable
    {
        public const string TagName = "RemoteConfigurationManager";
        private string applicationName;
        private int timeout;
        private int readwriteTimeout;
        private int timeInterval;
        private string remoteConfigurationUrl;
        private string localConfigurationFolder;
        private bool backupConfig;
        private int maxBackupFiles;
        private bool checkRemoteConfig;
        private static RemoteConfigurationManagerConfiguration defaultConfig;
        public string ApplicationName
        {
            get
            {
                return this.applicationName;
            }
            set
            {
                this.applicationName = value;
            }
        }
        public int Timeout
        {
            get
            {
                return this.timeout;
            }
            set
            {
                this.timeout = value;
            }
        }
        public int ReadWriteTimeout
        {
            get
            {
                return this.readwriteTimeout;
            }
            set
            {
                this.readwriteTimeout = value;
            }
        }
        public int TimerInterval
        {
            get
            {
                return this.timeInterval;
            }
            set
            {
                this.timeInterval = value;
            }
        }
        public string RemoteConfigurationUrl
        {
            get
            {
                return this.remoteConfigurationUrl;
            }
            set
            {
                this.remoteConfigurationUrl = value;
            }
        }
        public string LocalConfigurationFolder
        {
            get
            {
                return this.localConfigurationFolder;
            }
            set
            {
                this.localConfigurationFolder = value;
            }
        }
        public string LocalApplicationFolder
        {
            get
            {
                return ConfigUtility.Combine(this.localConfigurationFolder, this.applicationName);
            }
        }
        public bool BackupConfig
        {
            get
            {
                return this.backupConfig;
            }
            set
            {
                this.backupConfig = value;
            }
        }
        public int MaxBackupFiles
        {
            get
            {
                return this.maxBackupFiles;
            }
            set
            {
                this.maxBackupFiles = value;
            }
        }
        public bool CheckRemoteConfig
        {
            get
            {
                return this.checkRemoteConfig;
            }
            set
            {
                this.checkRemoteConfig = value;
            }
        }
        public static RemoteConfigurationManagerConfiguration DefaultConfig
        {
            get
            {
                if (RemoteConfigurationManagerConfiguration.defaultConfig == null)
                {
                    RemoteConfigurationManagerConfiguration config = new RemoteConfigurationManagerConfiguration();
                    config.ApplicationName = ConfigUtility.ApplicationName;
                    config.Timeout = 5000;
                    config.ReadWriteTimeout = 5000;
                    config.TimerInterval = 30000;
                    if (ConfigUtility.IsProd)
                    {
                        config.RemoteConfigurationUrl = "";
                    }
                    else if (ConfigUtility.IsStage)
                    {
                        config.RemoteConfigurationUrl = "";
                    }
                    else if (ConfigUtility.IsTesting)
                    {
                        config.RemoteConfigurationUrl = "";
                    }
                    else
                    {
                        config.RemoteConfigurationUrl = "";
                    }
                    config.LocalConfigurationFolder = ConfigUtility.RootConfigFolder;
                    config.BackupConfig = false;
                    config.MaxBackupFiles = 10;
                    config.CheckRemoteConfig = true;
                    RemoteConfigurationManagerConfiguration.defaultConfig = config;
                }
                return RemoteConfigurationManagerConfiguration.defaultConfig;
            }
        }
        private void EnsureLocalApplicationFolder()
        {
            if (!string.IsNullOrEmpty(this.localConfigurationFolder) && !string.IsNullOrEmpty(this.applicationName) && !System.IO.Directory.Exists(this.LocalApplicationFolder))
            {
                System.IO.Directory.CreateDirectory(this.LocalApplicationFolder);
            }
        }
        public XmlSchema GetSchema()
        {
            return null;
        }
        public void ReadXml(XmlReader reader)
        {
            this.applicationName = ConfigUtility.GetStringValue(reader, "applicationName", RemoteConfigurationManagerConfiguration.DefaultConfig.ApplicationName);
            this.timeout = ConfigUtility.GetIntValue(reader, "timeout", RemoteConfigurationManagerConfiguration.DefaultConfig.Timeout);
            this.readwriteTimeout = ConfigUtility.GetIntValue(reader, "readwriteTimeout", RemoteConfigurationManagerConfiguration.DefaultConfig.ReadWriteTimeout);
            this.timeInterval = ConfigUtility.GetIntValue(reader, "timeInterval", RemoteConfigurationManagerConfiguration.DefaultConfig.TimerInterval);
            this.remoteConfigurationUrl = ConfigUtility.GetStringValue(reader, "remoteConfigurationUrl", RemoteConfigurationManagerConfiguration.DefaultConfig.RemoteConfigurationUrl);
            this.localConfigurationFolder = ConfigUtility.GetStringValue(reader, "localConfigurationFolder", RemoteConfigurationManagerConfiguration.DefaultConfig.LocalConfigurationFolder);
            this.backupConfig = ConfigUtility.GetBoolValue(reader, "backupConfig", RemoteConfigurationManagerConfiguration.DefaultConfig.BackupConfig);
            this.maxBackupFiles = ConfigUtility.GetIntValue(reader, "maxBackupFiles", RemoteConfigurationManagerConfiguration.DefaultConfig.MaxBackupFiles);
            this.checkRemoteConfig = ConfigUtility.GetBoolValue(reader, "checkRemoteConfig", RemoteConfigurationManagerConfiguration.DefaultConfig.CheckRemoteConfig);
            this.EnsureLocalApplicationFolder();
        }
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("applicationName", this.applicationName);
            writer.WriteAttributeString("timeout", this.timeout.ToString());
            writer.WriteAttributeString("readwriteTimeout", this.readwriteTimeout.ToString());
            writer.WriteAttributeString("timeInterval", this.timeInterval.ToString());
            writer.WriteAttributeString("remoteConfigurationUrl", this.remoteConfigurationUrl);
            writer.WriteAttributeString("localConfigurationFolder", this.localConfigurationFolder);
            writer.WriteAttributeString("backupConfig", this.backupConfig.ToString());
            writer.WriteAttributeString("maxBackupFiles", this.maxBackupFiles.ToString());
            writer.WriteAttributeString("checkRemoteConfig", this.checkRemoteConfig.ToString());
        }
    }
}
