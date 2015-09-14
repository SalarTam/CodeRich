using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using Wintellect.Threading.ResourceLocks;

namespace Code.Configuration
{
    public class XmlSerializerSectionHandler : IConfigurationSectionHandler
    {
        private class EventObject
        {
            private System.EventHandler handler;
            private object sender;
            private System.EventArgs args;
            public EventObject(System.EventHandler handler, object sender, System.EventArgs args)
            {
                this.handler = handler;
                this.sender = sender;
                this.args = args;
            }
            public void Execute()
            {
                this.handler(this.sender, this.args);
            }
        }
        private const int CHANGE_CONFIG_DELAY = 5000;
        internal const int DefaultMajorVersion = 1;
        internal const int DefaultMinorVersion = 1;
        internal const int DefaultUninitMinorVersion = 0;
        private static readonly ConfigInstances configInstances = new ConfigInstances();
        private static readonly System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<System.EventHandler>> reloadTypeDelegates = new System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<System.EventHandler>>();
        private static readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<System.EventHandler>> reloadFileDelegates = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<System.EventHandler>>();
        private static readonly ResourceLock reloadDelegatesResourceLock = new OneResourceLock();
        public object Create(object parent, object configContext, XmlNode section)
        {
            object retVal = XmlSerializerSectionHandler.GetConfigInstance(section);
            System.Configuration.Configuration config = LocalConfigurationManager.LocalMainConfig;
            ConfigurationSection configSection = config.GetSection(section.Name);
            if (!configSection.SectionInformation.RestartOnExternalChanges)
            {
                XmlSerializerSectionHandler.SetupWatcher(config, configSection, retVal);
            }
            return retVal;
        }
        public static T CreateAndSetupWatcher<T>(string path)
        {
            return XmlSerializerSectionHandler.CreateAndSetupWatcher<T>(path, null);
        }
        public static T CreateAndSetupWatcher<T>(string path, System.EventHandler OnConfigFileChangedByFile)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            return XmlSerializerSectionHandler.CreateAndSetupWatcher<T>(doc.DocumentElement, path, OnConfigFileChangedByFile);
        }
        public static object CreateAndSetupWatcher(XmlNode section, string path, System.Type type, System.EventHandler OnConfigFileChangedByFile)
        {
            object obj = XmlSerializerSectionHandler.GetConfigInstance(section, type);
            XmlSerializerSectionHandler.SetupWatcher(path, obj);
            if (OnConfigFileChangedByFile != null)
            {
                XmlSerializerSectionHandler.RegisterReloadNotification(path, OnConfigFileChangedByFile);
            }
            return obj;
        }
        public static T CreateAndSetupWatcher<T>(XmlNode section, string path, System.EventHandler OnConfigFileChangedByFile)
        {
            return (T)((object)XmlSerializerSectionHandler.CreateAndSetupWatcher(section, path, typeof(T), OnConfigFileChangedByFile));
        }
        private static T GetConfigInstance<T>(XmlNode section)
        {
            return (T)((object)XmlSerializerSectionHandler.GetConfigInstance(section, typeof(T)));
        }
        public static int GetConfigurationClassMajorVersion<T>()
        {
            return XmlSerializerSectionHandler.GetConfigurationClassMajorVersion(typeof(T));
        }
        public static int GetConfigurationClassMajorVersion(System.Type type)
        {
            object[] objAttrs = type.GetCustomAttributes(typeof(ConfigurationVersionAttribute), false);
            if (objAttrs == null || objAttrs.Length == 0)
            {
                return 1;
            }
            return ((ConfigurationVersionAttribute)objAttrs[0]).MajorVersion;
        }
        internal static void GetConfigVersion(XmlElement section, out int major, out int minor)
        {
            if (!int.TryParse(section.GetAttribute("majorVersion"), out major))
            {
                major = 1;
            }
            if (!int.TryParse(section.GetAttribute("minorVersion"), out minor))
            {
                minor = 1;
            }
        }
        public static object GetConfigInstance(XmlNode section, System.Type t)
        {
            int fileMajorVersion;
            int fileMinorVersion;
            XmlSerializerSectionHandler.GetConfigVersion((XmlElement)section, out fileMajorVersion, out fileMinorVersion);
            XmlSerializerSectionHandler.GetConfigurationClassMajorVersion(t);
            XmlSerializer ser = new XmlSerializer(t);
            object result;
            try
            {
                object obj = ser.Deserialize(new XmlNodeReader(section));
                if (obj is IPostSerializer)
                {
                    ((IPostSerializer)obj).PostSerializer();
                }
                result = obj;
            }
            catch (System.Exception ex)
            {
                System.Exception innerEx = ex;
                if (ex.InnerException != null)
                {
                    innerEx = ex.InnerException;
                }
                throw new ConfigurationErrorsException(string.Format("XmlSerializerSectionHandler failed to GetConfigInstance from '{0}'.  Error: \r\n{1}", t.FullName, innerEx.ToString()), innerEx);
            }
            return result;
        }
        public static T GetConfigInstance<T>(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            return XmlSerializerSectionHandler.GetConfigInstance<T>(doc.DocumentElement);
        }
        private static object GetConfigInstance(XmlNode section)
        {
            XPathNavigator nav = section.CreateNavigator();
            string typeName = (string)nav.Evaluate("string(@type)");
            System.Type t = System.Type.GetType(typeName);
            if (t == null)
            {
                throw new ConfigurationErrorsException("XmlSerializerSectionHandler failed to create type '" + typeName + "'.  Please ensure this is a valid type string.", section);
            }
            return XmlSerializerSectionHandler.GetConfigInstance(section, t);
        }
        private static string GetConfigFilePath(System.Configuration.Configuration confFile, ConfigurationSection section)
        {
            string configSource = section.SectionInformation.ConfigSource;
            if (configSource == string.Empty)
            {
                return System.IO.Path.GetFullPath(confFile.FilePath);
            }
            return ConfigUtility.Combine(System.IO.Path.GetDirectoryName(confFile.FilePath), configSource);
        }
        private static void SetupWatcher(System.Configuration.Configuration config, ConfigurationSection configSection, object configInstance)
        {
            string filePath = XmlSerializerSectionHandler.GetConfigFilePath(config, configSection);
            XmlSerializerSectionHandler.SetupWatcher(filePath, configInstance);
        }
        public static void SetupWatcher(string filePath, object configInstance)
        {
            string fileName = System.IO.Path.GetFileName(filePath);
            if (XmlSerializerSectionHandler.configInstances.Add(fileName, configInstance))
            {
                FileWatcher.Instance.AddFile(filePath, new System.EventHandler(XmlSerializerSectionHandler.DelayedProcessConfigChange));
            }
        }
        private static void CloneObject(object srcObject, object targetObject)
        {
            System.Type type = targetObject.GetType();
            System.Reflection.PropertyInfo propInstance = type.GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.SetProperty);
            if (propInstance != null && propInstance.CanRead && propInstance.CanWrite)
            {
                propInstance.SetValue(null, srcObject, null);
                return;
            }
            ICopyable srcCopyable = srcObject as ICopyable;
            if (srcCopyable != null)
            {
                srcCopyable.CopyTo(targetObject);
                return;
            }
            System.Reflection.PropertyInfo[] props = type.GetProperties();
            System.Reflection.PropertyInfo[] array = props;
            for (int i = 0; i < array.Length; i++)
            {
                System.Reflection.PropertyInfo prop = array[i];
                if (prop.CanRead && prop.CanWrite)
                {
                    prop.SetValue(targetObject, prop.GetValue(srcObject, null), null);
                }
            }
            System.Reflection.FieldInfo[] fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            System.Reflection.FieldInfo[] array2 = fields;
            for (int j = 0; j < array2.Length; j++)
            {
                System.Reflection.FieldInfo field = array2[j];
                field.SetValue(targetObject, field.GetValue(srcObject));
            }
        }
        private static void CallEventHandler(object obj)
        {
            XmlSerializerSectionHandler.EventObject evtObj = (XmlSerializerSectionHandler.EventObject)obj;
            evtObj.Execute();
        }
        private static void DelayedProcessConfigChange(object sender, System.EventArgs args)
        {
            string filePath = ((string)sender).ToLower();
            string fileName = System.IO.Path.GetFileName(filePath);
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            ConfigurationManager.RefreshSection(doc.DocumentElement.Name);
            object configInstance = XmlSerializerSectionHandler.configInstances[fileName];
            System.Type newSettingsType = configInstance.GetType();
            object newSettings = XmlSerializerSectionHandler.GetConfigInstance(doc.DocumentElement, newSettingsType);
            XmlSerializerSectionHandler.CloneObject(newSettings, configInstance);
            System.Collections.Generic.List<System.EventHandler> typeHandlers = new System.Collections.Generic.List<System.EventHandler>();
            using (XmlSerializerSectionHandler.reloadDelegatesResourceLock.WaitToRead())
            {
                System.Collections.Generic.List<System.EventHandler> delegateMethods;
                if (XmlSerializerSectionHandler.reloadTypeDelegates.TryGetValue(newSettingsType, out delegateMethods))
                {
                    typeHandlers.AddRange(delegateMethods);
                }
                if (XmlSerializerSectionHandler.reloadFileDelegates.TryGetValue(filePath, out delegateMethods))
                {
                    typeHandlers.AddRange(delegateMethods);
                }
            }
            FileChangedEventArgs eventArgs = new FileChangedEventArgs(filePath);
            foreach (System.EventHandler delegateMethod in typeHandlers)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(XmlSerializerSectionHandler.CallEventHandler), new XmlSerializerSectionHandler.EventObject(delegateMethod, newSettings, eventArgs));
            }
        }
        public static void RegisterReloadNotification(System.Type type, System.EventHandler delegateMethod)
        {
            XmlSerializerSectionHandler.RegisterReloadNotification(type, delegateMethod, true);
        }
        public static void RegisterReloadNotification(System.Type type, System.EventHandler delegateMethod, bool allowMultiple)
        {
            using (XmlSerializerSectionHandler.reloadDelegatesResourceLock.WaitToWrite())
            {
                System.Collections.Generic.List<System.EventHandler> delegateMethods;
                if (!allowMultiple || !XmlSerializerSectionHandler.reloadTypeDelegates.TryGetValue(type, out delegateMethods))
                {
                    delegateMethods = new System.Collections.Generic.List<System.EventHandler>();
                    delegateMethods.Add(delegateMethod);
                    XmlSerializerSectionHandler.reloadTypeDelegates[type] = delegateMethods;
                }
                else
                {
                    delegateMethods.Add(delegateMethod);
                }
            }
        }
        public static void RegisterReloadNotification(string filePath, System.EventHandler delegateMethod)
        {
            XmlSerializerSectionHandler.RegisterReloadNotification(filePath, delegateMethod, true);
        }
        public static void RegisterReloadNotification(string filePath, System.EventHandler delegateMethod, bool allowMultiple)
        {
            filePath = filePath.ToLower();
            using (XmlSerializerSectionHandler.reloadDelegatesResourceLock.WaitToWrite())
            {
                System.Collections.Generic.List<System.EventHandler> delegateMethods;
                if (!allowMultiple || !XmlSerializerSectionHandler.reloadFileDelegates.TryGetValue(filePath, out delegateMethods))
                {
                    delegateMethods = new System.Collections.Generic.List<System.EventHandler>();
                    delegateMethods.Add(delegateMethod);
                    XmlSerializerSectionHandler.reloadFileDelegates[filePath] = delegateMethods;
                }
                else
                {
                    delegateMethods.Add(delegateMethod);
                }
            }
        }
    }
}
