using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Xml;

namespace Code.Framework.RemoteConfiguration
{
    public enum NetworkEnvironment
    {
        Production,
        Development,
        Testing,
        Labs
    }

    public sealed class ConfigUtility
    {
        //配置文件本地存放地址（从远程配置下载的配置文件也会存放在此目录）
        public const string DefaultRootConfigFolder = "c:\\zhaopin.configs";

        private static string applicationName;

        private static string baseDomain;

        private static string encodedExecutablePath;

        private static string executablePath;

        private static string rootConfigFolder;

        public static string ApplicationName
        {
            get
            {
                if (string.IsNullOrEmpty(applicationName))
                {
                    applicationName = FrameworkConfigManager.GetValue("FrameworkConfigName"); //System.Configuration.ConfigurationManager.AppSettings["applicationName"];
                    if (string.IsNullOrEmpty(applicationName))
                        applicationName = EncodedExecutablePath;
                }
                return applicationName;
            }
        }

        public static string BaseDomain
        {
            get
            {
                if (string.IsNullOrEmpty(baseDomain))
                {
                    baseDomain = FrameworkConfigManager.GetValue("FrameworkBaseDomain"); //System.Configuration.ConfigurationManager.AppSettings["baseDomain"];
                    if (string.IsNullOrEmpty(baseDomain))
                        baseDomain = "zhaopin.com";
                }
                return baseDomain;
            }
        }

        public static NetworkEnvironment CurrentEnvironment
        {
            get
            {
                string strEnv = FrameworkConfigManager.GetValue("FrameworkEnvironment"); //System.Configuration.ConfigurationManager.AppSettings["environment"];
                if (!string.IsNullOrEmpty(strEnv))
                {
                    try
                    {
                        NetworkEnvironment env = (NetworkEnvironment)Enum.Parse(typeof(NetworkEnvironment), strEnv);
                        return env;
                    }
                    catch
                    {
                    }
                }

                try
                {
                    IPHostEntry iph = Dns.GetHostEntry(Dns.GetHostName()); //Dns.GetHostByName(Dns.GetHostName());
                    if (iph.AddressList != null)
                    {
                        foreach (IPAddress address in iph.AddressList)
                        {
                            byte[] bytes = address.GetAddressBytes();
                            if (bytes[0] == 10 && bytes[1] == 22)
                                return NetworkEnvironment.Production;
                        }
                    }
                }
                catch
                {
                }

                return NetworkEnvironment.Development;
            }
        }

        //public static string DefaultApplicationConfigFolder = Path.GetFullPath(Path.Combine(RootConfigFolder, ApplicationName));

        public static string DefaultApplicationConfigFolder
        {
            get
            {
                //ConfigUtility utility = new ConfigUtility();

                if (System.Web.HttpContext.Current != null)
                {
                    return Path.GetFullPath(Path.Combine(RootConfigFolder, ApplicationName));
                    //return System.Web.HttpContext.Current.Server.MapPath("~/" + Path.Combine(RootConfigFolder, ApplicationName));
                }
                else
                {
                    return AppDomain.CurrentDomain.BaseDirectory + Path.Combine(RootConfigFolder, ApplicationName);
                }
            }
        }

        public static string EncodedExecutablePath
        {
            get
            {
                if (encodedExecutablePath == null)
                    encodedExecutablePath = EncodeToPath(ExecutablePath);
                return encodedExecutablePath;
            }
        }

        public static string ExecutablePath
        {
            get
            {
                if (string.IsNullOrEmpty(executablePath))
                {
                    if (IsWebApplication)
                    {
                        executablePath = System.AppDomain.CurrentDomain.BaseDirectory;
                    }
                    else
                    {
                        System.Reflection.Assembly ass = System.Reflection.Assembly.GetEntryAssembly();
                        if (ass != null)
                            executablePath = ass.Location;
                        else
                            executablePath = System.AppDomain.CurrentDomain.BaseDirectory;
                    }
                }
                return executablePath;
            }
        }

        //        return IPAddress.Parse(ip);
        //    }
        //}
        public static bool IsProd
        {
            get
            {
                return CurrentEnvironment == NetworkEnvironment.Production;
            }
        }

        public static bool IsStage
        {
            get
            {
                return CurrentEnvironment == NetworkEnvironment.Labs;
            }
        }

        //public static IPAddress LocalIPAdddress
        //{
        //    get
        //    {
        //        string hostName = Dns.GetHostName();
        //        string ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0]; //Dns.Resolve(hostName).AddressList[0].ToString();
        public static bool IsTesting
        {
            get
            {
                return CurrentEnvironment == NetworkEnvironment.Testing;
            }
        }

        public static bool IsWebApplication
        {
            get
            {
                string AppVirtualPath = System.Web.HttpRuntime.AppDomainAppVirtualPath;
                if (!string.IsNullOrEmpty(AppVirtualPath))
                {
                    /*
                     * Use the root web.config. This did check where the request was and
                     * load that web.config. In longhorn we cannot call HttpContext.Request in
                     * the Application Start. This throws an exception. Changed to always pull
                     * the root web.config
                     * */
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static string RootConfigFolder
        {
            get
            {
                if (string.IsNullOrEmpty(rootConfigFolder))
                {
                    rootConfigFolder = FrameworkConfigManager.GetValue("FrameworkConfigRoot"); //System.Configuration.ConfigurationManager.AppSettings["configRoot"];
                    if (string.IsNullOrEmpty(rootConfigFolder))
                    {
                        if (System.Web.HttpContext.Current != null)
                        {
                            rootConfigFolder = DefaultRootConfigFolder;
                        }
                        else
                        {
                            rootConfigFolder = "Configs";
                        }
                    }
                }
                return rootConfigFolder;
            }
        }

        /// <summary>
        /// Build absolute url based on BaseDomain
        /// </summary>
        /// <param name="host">host name without domain</param>
        /// <param name="relativeUrl"></param>
        /// <returns></returns>
        public static string BuildUrl(string host, string relativeUrl)
        {
            if (string.IsNullOrEmpty(host))
                return string.Concat("http://", BaseDomain, relativeUrl);
            else
                return string.Concat("http://", host, ".", BaseDomain, relativeUrl);
        }

        public static string Combine(string folder, string file)
        {
            return Path.Combine(folder, file);
        }

        //static string rootLogFolder;
        //public static string RootLogFolder
        //{
        //    get
        //    {
        //        if (rootLogFolder == null)
        //        {
        //            rootLogFolder = FrameworkProvider.GetValue("logRoot"); //System.Configuration.ConfigurationManager.AppSettings["logRoot"];
        //            if (rootLogFolder == null)
        //                rootLogFolder = DefaultRootLogFolder;
        //        }
        //        return rootLogFolder;
        //    }
        //}

        public static void DumpStack(TextWriter writer)
        {
            StackTrace trace = new StackTrace();
            foreach (StackFrame frame in trace.GetFrames())
            {
                string file = frame.GetFileName();
                int line = frame.GetFileLineNumber();
                int column = frame.GetFileColumnNumber();
                string methodName = frame.GetMethod().Name;
                string clsName = frame.GetMethod().DeclaringType.FullName;
                writer.WriteLine(clsName + "." + methodName + "," + file + ":" + line);
            }
        }

        public static bool GetBoolValue(XmlReader reader, string name, bool defaultValue)
        {
            bool val;
            if (!bool.TryParse(reader.GetAttribute(name), out val))
                val = defaultValue;
            return val;
        }

        public static int GetIntValue(XmlReader reader, string name, int defaultValue)
        {
            int val;
            if (!int.TryParse(reader.GetAttribute(name), out val))
                val = defaultValue;
            return val;
        }

        public static string GetStringValue(XmlReader reader, string name, string defaultValue)
        {
            string val;
            val = reader.GetAttribute(name);
            if (val == null)
                val = defaultValue;
            return val;
        }

        private static string EncodeToPath(string path)
        {
            char[] chs = path.ToCharArray();
            for (int i = 0; i < path.Length; i++)
            {
                if (!IsLetterOrNumber(chs[i]))
                {
                    chs[i] = '_';
                }
            }
            return new string(chs);
        }

        private static bool IsLetterOrNumber(char i)
        {
            return ((i >= 'A' && i <= 'Z') || (i >= 'a' && i <= 'z') || (i >= '0' && i <= '9'));
        }
    }
}