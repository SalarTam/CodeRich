using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;


namespace Code.Framework.RemoteConfiguration
{
    [XmlRoot(Namespace = "Zhaopin.Campus.Framework.RemoteConfiguration")]
    public class RemoteConfigurationManager : BaseConfigurationManager
    {
        private const string RemoteConfigFileAppSettingKey = "RemoteConfigFile";
        private const string RemoteConfigurationManagerConfigFileName = "RemoteConfigurationManager.config";
        private static RemoteConfigurationManager instance = new RemoteConfigurationManager();
        private RemoteConfigurationManagerConfiguration config;
        private System.Threading.Timer timer;
        private static bool timerRunning = false;
        private static bool downNow = false;
        private static object downLocker = new object();

        protected RemoteConfigurationManager()
            : base()
        {
            string configFile = GetRemoteConfigFile();
            try
            {
                config =
                    XmlSerializerSectionHandler.CreateAndSetupWatcher<RemoteConfigurationManagerConfiguration>(configFile);

                if (config.CheckRemoteConfig)
                {
                    timer = new Timer(TimerCallback);
                    TimerCallback();
                    //timer.Change(config.TimerInterval, Timeout.Infinite);
                    //System.Timers.Timer timer = new System.Timers.Timer(config.TimerInterval);
                    //timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerCallback);
                    //timer.Start();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex,
                    "Unabled to load RemoteConfigurationManager configuration file, Please set '" + RemoteConfigFileAppSettingKey + "' in appSettings",
                    "RemoteConfigurationManager");
                throw ex;
            }
        }

        private delegate void DownloadChecker(string sectionName, Stream stream);

        public static RemoteConfigurationManager Instance
        {
            get
            {
                return instance;
            }
        }

        public void InvalidAllConfigs()
        {
            RemoteConfigSectionCollection lstParams = new RemoteConfigSectionCollection(config.ApplicationName);
            lock (configLocker)
            {
                foreach (ConfigEntry entry in configEntries.Values)
                {
                    if (entry.IsSet)
                        lstParams.AddSection(entry.Name, entry.MajorVersion, entry.MinorVersion);
                }
            }
            InvalidConfigs(lstParams);
        }

        public void InvalidConfig(string name, int majorVersion)
        {
            RemoteConfigSectionCollection lstParams = new RemoteConfigSectionCollection(config.ApplicationName);
            int orgMinorVersion = 0;
            lock (configLocker)
            {
                foreach (ConfigEntry entry in configEntries.Values)
                {
                    if (entry.Name == name)
                    {
                        if (entry.IsSet && entry.MajorVersion == majorVersion)
                        {
                            orgMinorVersion = entry.MinorVersion;
                            lstParams.AddSection(entry.Name, entry.MajorVersion, entry.MinorVersion);
                        }
                        break;
                    }
                }
            }
            InvalidConfigs(lstParams);
        }

        private static bool Download(string resourceName, string url, string targetPath, DownloadChecker checker)
        {
            //it's because of windows issue!!
            //WebClient client = new WebClient();
            //client.DownloadFile(url, targetPath);
            bool ret = false;
            if (!downNow)
            {
                downNow = true;
                HttpWebRequest req = null;
                HttpWebResponse rsp = null;
                Stream rspStream = null;
                string tmpFile = string.Empty;
                try
                {
                    req = (HttpWebRequest)HttpWebRequest.Create(url);
                    req.Proxy = null;
                    req.Method = "GET";
                    req.KeepAlive = false;

                    rsp = (HttpWebResponse)req.GetResponse();
                    tmpFile = GetTempFileName(targetPath);
                    rspStream = rsp.GetResponseStream();

                    //using (FileStream fout = new FileStream(tmpFile, FileMode.Create, FileAccess.Write))

                    if (checker != null)
                    {
                        //check before download
                        //Response.ContentLength is not reliable if Response use buffered mode
                        using (MemoryStream ms = new MemoryStream())
                        {
                            byte[] buf = new byte[4096];
                            int length;
                            while ((length = rspStream.Read(buf, 0, buf.Length)) > 0)
                                ms.Write(buf, 0, length);
                            ms.Position = 0;
                            checker(resourceName, ms);
                            ms.Position = 0;
                            WriteStreamToFile(ms, tmpFile);
                            ms.Close();
                        }
                    }
                    else
                        WriteStreamToFile(rspStream, tmpFile);

                    //need to check version here at the first place!

                    //this sucks, but this is to reduce the confliction of writing and reading
                    // because of sucks of Windows, the copyfile is non-transaction.
                    // we must remove the file before change its name!!!
                    if (File.Exists(targetPath))
                    {
                        if (!Instance.config.BackupConfig)
                            File.Delete(targetPath);
                        else
                        {
                            RemoveOldBackupFiles(GetSectionName(targetPath));
                            File.Move(targetPath, GetBackupFileName(targetPath));
                        }
                    }
                    File.Move(tmpFile, targetPath);

                    //File.Replace(tmpFile, targetPath, null);

                    ret = true;
                }
                catch (Exception ex)
                {
                    HandleException(ex, "Unabled to download '" + url + "' to '" + targetPath + "'", resourceName);
                    return false;
                }
                finally
                {
                    if (tmpFile != string.Empty && File.Exists(tmpFile))
                    {
                        File.Delete(tmpFile);
                    }

                    if (rspStream != null)
                    {
                        rsp.Close();
                        rsp = null;
                    }
                    if (rsp != null)
                    {
                        rsp.Close();
                        rsp = null;
                    }
                    req = null;
                    downNow = false;
                }
            }
            return true;
        }

        private static string GetBackupFileName(string filePath)
        {
            return filePath.Substring(0, filePath.LastIndexOf('.') + 1) + DateTime.Now.ToString("yyyyMMddHHmmss") + ".config";
        }

        private static string GetRemoteConfigFile()
        {
            //TODO: we MUST create default configuration here in case that "RemoteConfigurationManagerConfiguration.config" does not exist
            string remoteFile = FrameworkConfigManager.GetValue(RemoteConfigFileAppSettingKey); //System.Configuration.ConfigurationManager.AppSettings[RemoteConfigFileAppSettingKey];
            if (string.IsNullOrEmpty(remoteFile))
                remoteFile = LocalConfigurationManager.MapConfigPath(RemoteConfigurationManagerConfigFileName);
            else
                remoteFile = LocalConfigurationManager.MapConfigPath(remoteFile);
            if (!File.Exists(remoteFile))
            {
                Log("Config file '" + remoteFile + "' doesn't exists, use/create new configuration files in '" + ConfigUtility.DefaultApplicationConfigFolder + "'");
                remoteFile = ConfigUtility.Combine(ConfigUtility.DefaultApplicationConfigFolder, RemoteConfigurationManagerConfigFileName);
                if (!File.Exists(remoteFile))
                {
                    Directory.CreateDirectory(ConfigUtility.DefaultApplicationConfigFolder);
                    using (XmlTextWriter writer = new XmlTextWriter(remoteFile, Encoding.UTF8))
                    {
                        writer.WriteStartElement(RemoteConfigurationManagerConfiguration.TagName);
                        RemoteConfigurationManagerConfiguration.DefaultConfig.WriteXml(writer);
                        writer.WriteEndElement();
                        writer.Close();
                    }
                }
            }
            return remoteFile;
        }

        private static string GetSectionName(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        private static string GetTempFileName(string filePath)
        {
            return filePath + "."
                + System.DateTime.Now.Millisecond; //Guid.NewGuid().ToString("N");
        }

        private static void RemoveOldBackupFiles(string sectionName)
        {
            string[] files = Directory.GetFiles(instance.config.LocalApplicationFolder, sectionName + ".*.config");
            if (files.Length > instance.config.MaxBackupFiles)
            {
                List<string> lst = new List<string>(files);
                lst.Sort();

                while (lst.Count > instance.config.MaxBackupFiles)
                {
                    File.Delete(lst[0]);
                    lst.RemoveAt(0);
                }
            }
        }

        private static void WriteStreamToFile(Stream stream, string file)
        {
            FileStream fout = null;
            try
            {
                fout = new FileStream(file, FileMode.Create, FileAccess.Write);
                byte[] buf = new byte[4096];
                int length;
                while ((length = stream.Read(buf, 0, buf.Length)) > 0)
                    fout.Write(buf, 0, length);
                fout.Close();
            }
            catch (System.Exception ex)
            {
                CampusLogHelper.Error("RemoteConfig", ex);
            }
            finally
            {
                if (fout != null)
                {
                    fout.Dispose();
                    fout = null;
                }
            }
        }

        private void CheckDownloadStream(string sectionName, Stream stream)
        {
            ConfigEntry entry = this.GetEntry(sectionName);
            if (entry == null)
                throw new System.Configuration.ConfigurationErrorsException("No entry '" + sectionName + "' in RemoteConfigurationManager");

            XmlDocument doc = new XmlDocument();
            doc.Load(stream);
            XmlSerializerSectionHandler.GetConfigInstance(doc.DocumentElement, entry.EntryType);
        }

        private object CreateLocalObject(Type type, string path, out int major, out int minor)
        {
            try
            {
                if (File.Exists(path))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(path);
                    object obj = XmlSerializerSectionHandler.CreateAndSetupWatcher(doc.DocumentElement,
                        path, type, OnConfigFileChanged);

                    XmlSerializerSectionHandler.GetConfigVersion(doc.DocumentElement, out major, out minor);
                    return obj;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "RemoteConfigurationManager.CreateLocalObject,type=" + type.Name + ",path=" + path, type.Name);
            }
            major = XmlSerializerSectionHandler.GetConfigurationClassMajorVersion(type);
            minor = XmlSerializerSectionHandler.DefaultUninitMinorVersion;
            return null;
        }

        private string GetFileName(string sectionName)
        {
            return sectionName + ".config";
        }

        private string GetPath(string sectionName)
        {
            return ConfigUtility.Combine(config.LocalApplicationFolder, GetFileName(sectionName));
        }

        private RemoteConfigSectionParam GetServerVersion(string name, int majorVersion)
        {
            RemoteConfigSectionCollection lstParams = new RemoteConfigSectionCollection(config.ApplicationName);
            lstParams.AddSection(name, majorVersion, XmlSerializerSectionHandler.DefaultUninitMinorVersion);
            lstParams = GetServerVersions(lstParams);
            if (lstParams.Count == 0)
                return null;
            else
                return lstParams[0];
        }

        private RemoteConfigSectionCollection GetServerVersions(RemoteConfigSectionCollection lstInputParams)
        {
            MemoryStream ms = null;
            XmlSerializer ser = null;
            HttpWebRequest req = null;
            try
            {
                ms = new MemoryStream();
                ser = new XmlSerializer(typeof(RemoteConfigSectionCollection));
                ser.Serialize(ms, lstInputParams);

                req = (HttpWebRequest)HttpWebRequest.Create(config.RemoteConfigurationUrl);
                req.ContentType = "text/xml";
                req.Proxy = null;
                req.Method = "POST";
                req.Timeout = config.Timeout;
                req.ReadWriteTimeout = config.ReadWriteTimeout;
                req.ContentLength = ms.Length;
                req.ServicePoint.Expect100Continue = false;
                req.ServicePoint.UseNagleAlgorithm = false;
                req.KeepAlive = false;

                using (Stream stream = req.GetRequestStream())
                {
                    byte[] buf = ms.ToArray();
                    stream.Write(buf, 0, buf.Length);
                    stream.Close();
                }

                RemoteConfigSectionCollection lstOutput;
                using (HttpWebResponse rsp = (HttpWebResponse)req.GetResponse())
                {
                    using (Stream stream = rsp.GetResponseStream())
                    {
                        lstOutput = (RemoteConfigSectionCollection)ser.Deserialize(stream);
                        stream.Close();
                    }
                    rsp.Close();
                }
                return lstOutput;
            }
            catch (Exception ex)
            {
                HandleException(ex, "Unabled to GetServerVersions from '" + config.RemoteConfigurationUrl + "'", "RemoteConfigurationManager");
                return new RemoteConfigSectionCollection();
            }
            finally
            {
                if (ms != null)
                {
                    ms.Dispose();
                    ms = null;
                }
                req = null;
                ser = null;
            }
        }

        private void InvalidConfigs(RemoteConfigSectionCollection lstParams)
        {
            if (lstParams.Count == 0) return;
            RemoteConfigSectionCollection newParams = GetServerVersions(lstParams);
            if (newParams.Count == 0) return;

            Dictionary<string, RemoteConfigSectionParam> tblOldParam = new Dictionary<string, RemoteConfigSectionParam>(lstParams.Count);
            foreach (RemoteConfigSectionParam item in lstParams)
                tblOldParam.Add(item.SectionName, item);

            foreach (RemoteConfigSectionParam param in newParams.Sections)
            {
                string localPath = GetPath(param.SectionName);
                if (!Download(param.SectionName, param.DownloadUrl, localPath, CheckDownloadStream))
                {
                    throw new System.Configuration.ConfigurationErrorsException("Unabled to download '" + param.DownloadUrl + "' to '" + localPath + "'");
                }
                FileWatcher.Instance.ProcessFileChanged(localPath);

                Log(string.Format("Minor version of config '{0}({1})' has been updated manually from {2} to {3}",
                            param.SectionName, param.MajorVersion,
                            tblOldParam[param.SectionName].MinorVersion, param.MinorVersion));
            }
        }

        private void OnConfigFileChanged(object sender, EventArgs args)
        {
            string filePath = ((FileChangedEventArgs)args).FileName;
            string sectionName = GetSectionName(filePath);

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            int major, minor;
            XmlSerializerSectionHandler.GetConfigVersion(doc.DocumentElement, out major, out minor);

            ConfigEntry entry = GetEntry(sectionName);
            if (entry != null)
                entry.MinorVersion = minor;
        }

        private void TimerCallback(object stat = null)
        {
            if (!timerRunning)
            {
                lock (configLocker)
                {
                    timerRunning = true;
                    try
                    {
                        //Log(string.Format("start Time:{0}", DateTime.Now.ToString()));
                        RemoteConfigSectionCollection lstParams = new RemoteConfigSectionCollection(config.ApplicationName);
                        //configLocker.AcquireReaderLock(-1);
                        //using (configLocker.AcquireReaderLock())

                        foreach (ConfigEntry entry in configEntries.Values)
                        {
                            if (entry.IsSet)
                                lstParams.AddSection(entry.Name, entry.MajorVersion, entry.MinorVersion);
                        }
                        //configLocker.ReleaseReaderLock();
                        if (lstParams.Count != 0)
                        {
                            lstParams = GetServerVersions(lstParams);
                            if (lstParams.Count != 0)
                            {
                                Log(string.Format("获得新的配置文件：SectionName:{0} MajorVersion:{1} MinorVersion:{2}", lstParams[0].SectionName, lstParams[0].MajorVersion, lstParams[0].MinorVersion));
                                foreach (RemoteConfigSectionParam param in lstParams.Sections)
                                {
                                    ThreadPool.QueueUserWorkItem(
                                        delegate(object obj)
                                        {
                                            DownloadParam dp = (DownloadParam)obj;
                                            Download(dp.Name, dp.Url, dp.LocalPath, dp.Checker);
                                        },
                                        new DownloadParam(param.SectionName,
                                            param.DownloadUrl,
                                            GetPath(param.SectionName),
                                            CheckDownloadStream)
                                            );
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log(ex.ToString());
                    }
                    finally
                    {
                        timer.Change(config.TimerInterval, Timeout.Infinite);
                        timerRunning = false;
                    }
                }
            }
        }

        #region Override entry

        protected override object OnCreate(string sectionName, Type type, out int major, out int minor)
        {
            string fileName = GetFileName(sectionName);
            //string path = LocalConfigurationManager.LocalBaseConfigFolder + "\\" + GetPath(sectionName);
            string path = GetPath(sectionName);
            object obj = CreateLocalObject(type, path, out major, out minor);
            if (obj != null)
                return obj;

            //Get Remote Config version
            major = XmlSerializerSectionHandler.GetConfigurationClassMajorVersion(type);
            minor = XmlSerializerSectionHandler.DefaultUninitMinorVersion;
            try
            {
                RemoteConfigSectionParam param = GetServerVersion(sectionName, major);
                if (param != null)
                {
                    //download from remote!
                    if (Download(param.SectionName, param.DownloadUrl, path, CheckDownloadStream))
                        obj = CreateLocalObject(type, path, out major, out minor);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "Error when download configuration '" + sectionName + "' from remote server for the firet time", sectionName);
            }

            //if object is null use default object instead
            if (obj == null)
            {
                Log("Cannot get section '" + sectionName + "' with type '" + type.Name + "' from RemoteConfiguration, create empty instance instead");
                obj = Activator.CreateInstance(type);
                XmlSerializerSectionHandler.SetupWatcher(path, obj);
                XmlSerializerSectionHandler.RegisterReloadNotification(path, OnConfigFileChanged);
            }
            return obj;
        }

        #endregion Override entry

        private void TimerCallback(object sender, System.Timers.ElapsedEventArgs args)
        {
            RemoteConfigSectionCollection lstParams = new RemoteConfigSectionCollection(config.ApplicationName);
            //configLocker.AcquireReaderLock(-1);
            //using (configLocker.AcquireReaderLock())
            if (!timerRunning)
            {
                lock (configLocker)
                {
                    timerRunning = true;
                    foreach (ConfigEntry entry in configEntries.Values)
                    {
                        if (entry.IsSet)
                            lstParams.AddSection(entry.Name, entry.MajorVersion, entry.MinorVersion);
                    }
                    //configLocker.ReleaseReaderLock();

                    if (lstParams.Count != 0)
                    {
                        lstParams = GetServerVersions(lstParams);
                        if (lstParams.Count == 0) return;
                        foreach (RemoteConfigSectionParam param in lstParams.Sections)
                        {
                            ThreadPool.QueueUserWorkItem(
                                delegate(object obj)
                                {
                                    DownloadParam dp = (DownloadParam)obj;
                                    Download(dp.Name, dp.Url, dp.LocalPath, dp.Checker);
                                },
                                new DownloadParam(param.SectionName,
                                    param.DownloadUrl,
                                    GetPath(param.SectionName),
                                    CheckDownloadStream)
                                    );
                        }
                    }
                    timerRunning = false;
                }
            }
        }

        private class DownloadParam
        {
            public DownloadChecker Checker;
            public string LocalPath;
            public string Name;
            public string Url;

            public DownloadParam(string name, string url, string path, DownloadChecker checker)
            {
                this.Name = name;
                this.Url = url;
                this.LocalPath = path; //LocalConfigurationManager.LocalBaseConfigFolder + "\\" + path;
                this.Checker = checker;
            }
        }
    }
}