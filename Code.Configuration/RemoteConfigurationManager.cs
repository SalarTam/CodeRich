using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Timers;
using System.Xml;
using System.Xml.Serialization;

namespace Code.Configuration
{
    [XmlRoot(Namespace = "Code.Configuration")]
    public class RemoteConfigurationManager : BaseConfigurationManager
    {
        private class DownloadParam
        {
            public string Name;
            public string LocalPath;
            public string Url;
            public RemoteConfigurationManager.DownloadChecker Checker;
            public DownloadParam(string name, string url, string path, RemoteConfigurationManager.DownloadChecker checker)
            {
                this.Name = name;
                this.Url = url;
                this.LocalPath = path;
                this.Checker = checker;
            }
        }
        private delegate void DownloadChecker(string sectionName, System.IO.Stream stream);
        private const string RemoteConfigFileAppSettingKey = "RemoteConfigFile";
        private const string RemoteConfigurationManagerConfigFileName = "RemoteConfigurationManager.config";
        private RemoteConfigurationManagerConfiguration config;
        private static RemoteConfigurationManager instance = new RemoteConfigurationManager();
        private System.Threading.Timer timer;
        public static RemoteConfigurationManager Instance
        {
            get
            {
                return RemoteConfigurationManager.instance;
            }
        }
        private static string GetRemoteConfigFile()
        {
            string remoteFile = ConfigurationManager.AppSettings["RemoteConfigFile"];
            if (remoteFile == null)
            {
                remoteFile = LocalConfigurationManager.MapConfigPath("RemoteConfigurationManager.config");
            }
            else
            {
                remoteFile = LocalConfigurationManager.MapConfigPath(remoteFile);
            }
            if (!System.IO.File.Exists(remoteFile))
            {
                BaseConfigurationManager.Log(string.Concat(new string[]
                {
                    "Config file '",
                    remoteFile,
                    "' doesn't exists, use/create new configuration files in '",
                    ConfigUtility.DefaultApplicationConfigFolder,
                    "'"
                }));
                remoteFile = ConfigUtility.Combine(ConfigUtility.DefaultApplicationConfigFolder, "RemoteConfigurationManager.config");
                if (!System.IO.File.Exists(remoteFile))
                {
                    System.IO.Directory.CreateDirectory(ConfigUtility.DefaultApplicationConfigFolder);
                    using (XmlTextWriter writer = new XmlTextWriter(remoteFile, System.Text.Encoding.UTF8))
                    {
                        writer.WriteStartElement("RemoteConfigurationManager");
                        RemoteConfigurationManagerConfiguration.DefaultConfig.WriteXml(writer);
                        writer.WriteEndElement();
                        writer.Close();
                    }
                }
            }
            return remoteFile;
        }
        protected RemoteConfigurationManager()
        {
            string configFile = RemoteConfigurationManager.GetRemoteConfigFile();
            try
            {
                this.config = XmlSerializerSectionHandler.CreateAndSetupWatcher<RemoteConfigurationManagerConfiguration>(configFile);
                if (this.config.CheckRemoteConfig)
                {
                    this.timer = new System.Threading.Timer(new System.Threading.TimerCallback(this.TimerCallback));
                    this.TimerCallback(null);
                }
            }
            catch (System.Exception ex)
            {
                BaseConfigurationManager.HandleException(ex, "Unabled to load RemoteConfigurationManager configuration file, Please set 'RemoteConfigFile' in appSettings", "RemoteConfigurationManager");
                throw ex;
            }
        }
        private void TimerCallback(object stat = null)
        {
            try
            {
                RemoteConfigSectionCollection lstParams = new RemoteConfigSectionCollection(this.config.ApplicationName);
                lock (this.configLocker)
                {
                    foreach (ConfigEntry entry in this.configEntries.Values)
                    {
                        if (entry.IsSet)
                        {
                            lstParams.AddSection(entry.Name, entry.MajorVersion, entry.MinorVersion);
                        }
                    }
                }
                if (lstParams.Count != 0)
                {
                    lstParams = this.GetServerVersions(lstParams);
                    if (lstParams.Count != 0)
                    {
                        BaseConfigurationManager.Log(string.Format("获得新的配置文件：SectionName:{0} MajorVersion:{1} MinorVersion:{2}", lstParams[0].SectionName, lstParams[0].MajorVersion, lstParams[0].MinorVersion));
                        foreach (RemoteConfigSectionParam param in lstParams.Sections)
                        {
                            System.Threading.ThreadPool.QueueUserWorkItem(delegate (object obj)
                            {
                                RemoteConfigurationManager.DownloadParam dp = (RemoteConfigurationManager.DownloadParam)obj;
                                RemoteConfigurationManager.Download(dp.Name, dp.Url, dp.LocalPath, dp.Checker);
                            }, new RemoteConfigurationManager.DownloadParam(param.SectionName, param.DownloadUrl, this.GetPath(param.SectionName), new RemoteConfigurationManager.DownloadChecker(this.CheckDownloadStream)));
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                BaseConfigurationManager.Log(ex.ToString());
            }
            finally
            {
                this.timer.Change(this.config.TimerInterval, -1);
            }
        }
        private static string GetSectionName(string path)
        {
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }
        private string GetFileName(string sectionName)
        {
            return sectionName + ".config";
        }
        private string GetPath(string sectionName)
        {
            return ConfigUtility.Combine(this.config.LocalApplicationFolder, this.GetFileName(sectionName));
        }
        private static void RemoveOldBackupFiles(string sectionName)
        {
            string[] files = System.IO.Directory.GetFiles(RemoteConfigurationManager.instance.config.LocalApplicationFolder, sectionName + ".*.config");
            if (files.Length > RemoteConfigurationManager.instance.config.MaxBackupFiles)
            {
                System.Collections.Generic.List<string> lst = new System.Collections.Generic.List<string>(files);
                lst.Sort();
                while (lst.Count > RemoteConfigurationManager.instance.config.MaxBackupFiles)
                {
                    System.IO.File.Delete(lst[0]);
                    lst.RemoveAt(0);
                }
            }
        }
        private static string GetTempFileName(string filePath)
        {
            return filePath + "." + System.Guid.NewGuid().ToString("N");
        }
        private static string GetBackupFileName(string filePath)
        {
            return filePath.Substring(0, filePath.LastIndexOf('.') + 1) + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".config";
        }
        private void OnConfigFileChanged(object sender, System.EventArgs args)
        {
            string filePath = ((FileChangedEventArgs)args).FileName;
            string sectionName = RemoteConfigurationManager.GetSectionName(filePath);
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            int major;
            int minor;
            XmlSerializerSectionHandler.GetConfigVersion(doc.DocumentElement, out major, out minor);
            ConfigEntry entry = base.GetEntry(sectionName);
            if (entry != null)
            {
                entry.MinorVersion = minor;
            }
        }
        private object CreateLocalObject(System.Type type, string path, out int major, out int minor)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(path);
                    object obj = XmlSerializerSectionHandler.CreateAndSetupWatcher(doc.DocumentElement, path, type, new System.EventHandler(this.OnConfigFileChanged));
                    XmlSerializerSectionHandler.GetConfigVersion(doc.DocumentElement, out major, out minor);
                    return obj;
                }
            }
            catch (System.Exception ex)
            {
                BaseConfigurationManager.HandleException(ex, "RemoteConfigurationManager.CreateLocalObject,type=" + type.Name + ",path=" + path, type.Name);
            }
            major = XmlSerializerSectionHandler.GetConfigurationClassMajorVersion(type);
            minor = 0;
            return null;
        }
        protected override object OnCreate(string sectionName, System.Type type, out int major, out int minor)
        {
            this.GetFileName(sectionName);
            string path = this.GetPath(sectionName);
            object obj = this.CreateLocalObject(type, path, out major, out minor);
            if (obj != null)
            {
                return obj;
            }
            major = XmlSerializerSectionHandler.GetConfigurationClassMajorVersion(type);
            minor = 0;
            try
            {
                RemoteConfigSectionParam param = this.GetServerVersion(sectionName, major);
                if (param != null && RemoteConfigurationManager.Download(param.SectionName, param.DownloadUrl, path, new RemoteConfigurationManager.DownloadChecker(this.CheckDownloadStream)))
                {
                    obj = this.CreateLocalObject(type, path, out major, out minor);
                }
            }
            catch (System.Exception ex)
            {
                BaseConfigurationManager.HandleException(ex, "Error when download configuration '" + sectionName + "' from remote server for the firet time", sectionName);
            }
            if (obj == null)
            {
                BaseConfigurationManager.Log(string.Concat(new string[]
                {
                    "Cannot get section '",
                    sectionName,
                    "' with type '",
                    type.Name,
                    "' from RemoteConfiguration, create empty instance instead"
                }));
                obj = System.Activator.CreateInstance(type);
                XmlSerializerSectionHandler.SetupWatcher(path, obj);
                XmlSerializerSectionHandler.RegisterReloadNotification(path, new System.EventHandler(this.OnConfigFileChanged));
            }
            return obj;
        }
        private RemoteConfigSectionParam GetServerVersion(string name, int majorVersion)
        {
            RemoteConfigSectionCollection lstParams = new RemoteConfigSectionCollection(this.config.ApplicationName);
            lstParams.AddSection(name, majorVersion, 0);
            lstParams = this.GetServerVersions(lstParams);
            if (lstParams.Count == 0)
            {
                return null;
            }
            return lstParams[0];
        }
        private RemoteConfigSectionCollection GetServerVersions(RemoteConfigSectionCollection lstInputParams)
        {
            RemoteConfigSectionCollection result;
            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                XmlSerializer ser = new XmlSerializer(typeof(RemoteConfigSectionCollection));
                ser.Serialize(ms, lstInputParams);
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(this.config.RemoteConfigurationUrl);
                req.ContentType = "text/xml";
                req.Proxy = null;
                req.Method = "POST";
                req.Timeout = this.config.Timeout;
                req.ReadWriteTimeout = this.config.ReadWriteTimeout;
                req.ContentLength = ms.Length;
                req.ServicePoint.Expect100Continue = false;
                req.ServicePoint.UseNagleAlgorithm = false;
                req.KeepAlive = false;
                using (System.IO.Stream stream = req.GetRequestStream())
                {
                    byte[] buf = ms.ToArray();
                    stream.Write(buf, 0, buf.Length);
                    stream.Close();
                }
                RemoteConfigSectionCollection lstOutput;
                using (HttpWebResponse rsp = (HttpWebResponse)req.GetResponse())
                {
                    using (System.IO.Stream stream2 = rsp.GetResponseStream())
                    {
                        lstOutput = (RemoteConfigSectionCollection)ser.Deserialize(stream2);
                        stream2.Close();
                    }
                    rsp.Close();
                }
                result = lstOutput;
            }
            catch (System.Exception ex)
            {
                BaseConfigurationManager.HandleException(ex, "Unabled to GetServerVersions from '" + this.config.RemoteConfigurationUrl + "'", "RemoteConfigurationManager");
                result = new RemoteConfigSectionCollection();
            }
            return result;
        }
        private static void WriteStreamToFile(System.IO.Stream stream, string file)
        {
            using (System.IO.FileStream fout = new System.IO.FileStream(file, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                byte[] buf = new byte[4096];
                int length;
                while ((length = stream.Read(buf, 0, buf.Length)) > 0)
                {
                    fout.Write(buf, 0, length);
                }
                fout.Close();
            }
        }
        private static bool Download(string resourceName, string url, string targetPath, RemoteConfigurationManager.DownloadChecker checker)
        {
            bool result;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Proxy = null;
                req.Method = "GET";
                req.KeepAlive = false;
                HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();
                string tmpFile = RemoteConfigurationManager.GetTempFileName(targetPath);
                using (System.IO.Stream rspStream = rsp.GetResponseStream())
                {
                    try
                    {
                        if (checker != null)
                        {
                            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                            {
                                byte[] buf = new byte[4096];
                                int length;
                                while ((length = rspStream.Read(buf, 0, buf.Length)) > 0)
                                {
                                    ms.Write(buf, 0, length);
                                }
                                ms.Position = 0L;
                                checker(resourceName, ms);
                                ms.Position = 0L;
                                RemoteConfigurationManager.WriteStreamToFile(ms, tmpFile);
                                ms.Close();
                                goto IL_B5;
                            }
                        }
                        RemoteConfigurationManager.WriteStreamToFile(rspStream, tmpFile);
                    IL_B5:
                        if (System.IO.File.Exists(targetPath))
                        {
                            if (!RemoteConfigurationManager.Instance.config.BackupConfig)
                            {
                                System.IO.File.Delete(targetPath);
                            }
                            else
                            {
                                RemoteConfigurationManager.RemoveOldBackupFiles(RemoteConfigurationManager.GetSectionName(targetPath));
                                System.IO.File.Move(targetPath, RemoteConfigurationManager.GetBackupFileName(targetPath));
                            }
                        }
                        System.IO.File.Move(tmpFile, targetPath);
                    }
                    finally
                    {
                        System.IO.File.Delete(tmpFile);
                    }
                }
                rsp.Close();
                result = true;
            }
            catch (System.Exception ex)
            {
                BaseConfigurationManager.HandleException(ex, string.Concat(new string[]
                {
                    "Unabled to download '",
                    url,
                    "' to '",
                    targetPath,
                    "'"
                }), resourceName);
                result = false;
            }
            return result;
        }
        public void InvalidAllConfigs()
        {
            RemoteConfigSectionCollection lstParams = new RemoteConfigSectionCollection(this.config.ApplicationName);
            lock (this.configLocker)
            {
                foreach (ConfigEntry entry in this.configEntries.Values)
                {
                    if (entry.IsSet)
                    {
                        lstParams.AddSection(entry.Name, entry.MajorVersion, entry.MinorVersion);
                    }
                }
            }
            this.InvalidConfigs(lstParams);
        }
        public void InvalidConfig(string name, int majorVersion)
        {
            RemoteConfigSectionCollection lstParams = new RemoteConfigSectionCollection(this.config.ApplicationName);
            lock (this.configLocker)
            {
                foreach (ConfigEntry entry in this.configEntries.Values)
                {
                    if (entry.Name == name)
                    {
                        if (entry.IsSet && entry.MajorVersion == majorVersion)
                        {
                            int arg_63_0 = entry.MinorVersion;
                            lstParams.AddSection(entry.Name, entry.MajorVersion, entry.MinorVersion);
                            break;
                        }
                        break;
                    }
                }
            }
            this.InvalidConfigs(lstParams);
        }
        private void InvalidConfigs(RemoteConfigSectionCollection lstParams)
        {
            if (lstParams.Count == 0)
            {
                return;
            }
            RemoteConfigSectionCollection newParams = this.GetServerVersions(lstParams);
            if (newParams.Count == 0)
            {
                return;
            }
            System.Collections.Generic.Dictionary<string, RemoteConfigSectionParam> tblOldParam = new System.Collections.Generic.Dictionary<string, RemoteConfigSectionParam>(lstParams.Count);
            foreach (RemoteConfigSectionParam item in lstParams)
            {
                tblOldParam.Add(item.SectionName, item);
            }
            foreach (RemoteConfigSectionParam param in newParams.Sections)
            {
                string localPath = this.GetPath(param.SectionName);
                if (!RemoteConfigurationManager.Download(param.SectionName, param.DownloadUrl, localPath, new RemoteConfigurationManager.DownloadChecker(this.CheckDownloadStream)))
                {
                    throw new ConfigurationErrorsException(string.Concat(new string[]
                    {
                        "Unabled to download '",
                        param.DownloadUrl,
                        "' to '",
                        localPath,
                        "'"
                    }));
                }
                FileWatcher.Instance.ProcessFileChanged(localPath);
                BaseConfigurationManager.Log(string.Format("Minor version of config '{0}({1})' has been updated manually from {2} to {3}", new object[]
                {
                    param.SectionName,
                    param.MajorVersion,
                    tblOldParam[param.SectionName].MinorVersion,
                    param.MinorVersion
                }));
            }
        }
        private void TimerCallback(object sender, ElapsedEventArgs args)
        {
            RemoteConfigSectionCollection lstParams = new RemoteConfigSectionCollection(this.config.ApplicationName);
            lock (this.configLocker)
            {
                foreach (ConfigEntry entry in this.configEntries.Values)
                {
                    if (entry.IsSet)
                    {
                        lstParams.AddSection(entry.Name, entry.MajorVersion, entry.MinorVersion);
                    }
                }
            }
            if (lstParams.Count == 0)
            {
                return;
            }
            lstParams = this.GetServerVersions(lstParams);
            if (lstParams.Count == 0)
            {
                return;
            }
            foreach (RemoteConfigSectionParam param in lstParams.Sections)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(delegate (object obj)
                {
                    RemoteConfigurationManager.DownloadParam dp = (RemoteConfigurationManager.DownloadParam)obj;
                    RemoteConfigurationManager.Download(dp.Name, dp.Url, dp.LocalPath, dp.Checker);
                }, new RemoteConfigurationManager.DownloadParam(param.SectionName, param.DownloadUrl, this.GetPath(param.SectionName), new RemoteConfigurationManager.DownloadChecker(this.CheckDownloadStream)));
            }
        }
        private void CheckDownloadStream(string sectionName, System.IO.Stream stream)
        {
            ConfigEntry entry = base.GetEntry(sectionName);
            if (entry == null)
            {
                throw new ConfigurationErrorsException("No entry '" + sectionName + "' in RemoteConfigurationManager");
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(stream);
            XmlSerializerSectionHandler.GetConfigInstance(doc.DocumentElement, entry.EntryType);
        }
    }
}
