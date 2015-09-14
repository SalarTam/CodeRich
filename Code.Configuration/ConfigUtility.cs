using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Code.Configuration
{
    public sealed class ConfigUtility
    {
        public const string DefaultRootConfigFolder = "c:\\code.configs";
        public const string DefaultRootLogFolder = "c:\\code.logfiles";
        private static string baseDomain;
        private static string executablePath;
        private static string applicationName;
        private static string rootConfigFolder;
        private static string rootLogFolder;
        public static string DefaultApplicationConfigFolder = System.IO.Path.GetFullPath(System.IO.Path.Combine(ConfigUtility.RootConfigFolder, ConfigUtility.ApplicationName));
        public static string DefaultApplicationLogFolder = System.IO.Path.GetFullPath(System.IO.Path.Combine(ConfigUtility.RootLogFolder, ConfigUtility.ApplicationName));
        private static string encodedExecutablePath;
        public static bool IsWebApplication
        {
            get
            {
                string AppVirtualPath = HttpRuntime.AppDomainAppVirtualPath;
                return !string.IsNullOrEmpty(AppVirtualPath);
            }
        }
        public static string BaseDomain
        {
            get
            {
                if (ConfigUtility.baseDomain == null)
                {
                    ConfigUtility.baseDomain = ConfigurationManager.AppSettings["baseDomain"];
                    if (ConfigUtility.baseDomain == null)
                    {
                        ConfigUtility.baseDomain = "code.com";
                       // ConfigUtility.baseDomain = "baidu.com";
                    }
                }
                return ConfigUtility.baseDomain;
            }
        }
        public static IPAddress LocalIPAdddress
        {
            get
            {
                string hostName = Dns.GetHostName();
                string ip = Dns.Resolve(hostName).AddressList[0].ToString();
                return IPAddress.Parse(ip);
            }
        }
        public static NetworkEnvironment CurrentEnvironment
        {
            get
            {
                string strEnv = ConfigurationManager.AppSettings["environment"];
                if (!string.IsNullOrEmpty(strEnv))
                {
                    try
                    {
                        NetworkEnvironment env = (NetworkEnvironment)System.Enum.Parse(typeof(NetworkEnvironment), strEnv);
                        NetworkEnvironment result = env;
                        return result;
                    }
                    catch
                    {
                    }
                }
                try
                {
                    IPHostEntry iph = Dns.GetHostByName(Dns.GetHostName());
                    if (iph.AddressList != null)
                    {
                        IPAddress[] addressList = iph.AddressList;
                        for (int i = 0; i < addressList.Length; i++)
                        {
                            IPAddress address = addressList[i];
                            byte[] bytes = address.GetAddressBytes();
                            if (bytes[0] == 10 && bytes[1] == 22)
                            {
                                NetworkEnvironment result = NetworkEnvironment.Production;
                                return result;
                            }
                        }
                    }
                }
                catch
                {
                }
                return NetworkEnvironment.Development;
            }
        }
        public static bool IsProd
        {
            get
            {
                return ConfigUtility.CurrentEnvironment == NetworkEnvironment.Production;
            }
        }
        public static bool IsTesting
        {
            get
            {
                return ConfigUtility.CurrentEnvironment == NetworkEnvironment.Testing;
            }
        }
        public static bool IsStage
        {
            get
            {
                return ConfigUtility.CurrentEnvironment == NetworkEnvironment.Labs;
            }
        }
        public static string ExecutablePath
        {
            get
            {
                if (ConfigUtility.executablePath == null)
                {
                    if (ConfigUtility.IsWebApplication)
                    {
                        ConfigUtility.executablePath = System.AppDomain.CurrentDomain.BaseDirectory;
                    }
                    else
                    {
                        System.Reflection.Assembly ass = System.Reflection.Assembly.GetEntryAssembly();
                        if (ass != null)
                        {
                            ConfigUtility.executablePath = ass.Location;
                        }
                        else
                        {
                            ConfigUtility.executablePath = System.AppDomain.CurrentDomain.BaseDirectory;
                        }
                    }
                }
                return ConfigUtility.executablePath;
            }
        }
        public static string ApplicationName
        {
            get
            {
                if (ConfigUtility.applicationName == null)
                {
                    ConfigUtility.applicationName = ConfigurationManager.AppSettings["applicationName"];
                    if (ConfigUtility.applicationName == null)
                    {
                        ConfigUtility.applicationName = ConfigUtility.EncodedExecutablePath;
                    }
                }
                return ConfigUtility.applicationName;
            }
        }
        public static string RootConfigFolder
        {
            get
            {
                if (ConfigUtility.rootConfigFolder == null)
                {
                    ConfigUtility.rootConfigFolder = ConfigurationManager.AppSettings["configRoot"];
                    if (ConfigUtility.rootConfigFolder == null)
                    {
                        ConfigUtility.rootConfigFolder = "c:\\code.configs";
                    }
                }
                return ConfigUtility.rootConfigFolder;
            }
        }
        public static string RootLogFolder
        {
            get
            {
                if (ConfigUtility.rootLogFolder == null)
                {
                    ConfigUtility.rootLogFolder = ConfigurationManager.AppSettings["logRoot"];
                    if (ConfigUtility.rootLogFolder == null)
                    {
                        ConfigUtility.rootLogFolder = "c:\\code.logfiles";
                    }
                }
                return ConfigUtility.rootLogFolder;
            }
        }
        public static string EncodedExecutablePath
        {
            get
            {
                if (ConfigUtility.encodedExecutablePath == null)
                {
                    ConfigUtility.encodedExecutablePath = ConfigUtility.EncodeToPath(ConfigUtility.ExecutablePath);
                }
                return ConfigUtility.encodedExecutablePath;
            }
        }
        public static string BuildUrl(string host, string relativeUrl)
        {
            if (string.IsNullOrEmpty(host))
            {
                return "http://" + ConfigUtility.BaseDomain + relativeUrl;
            }
            return string.Concat(new string[]
            {
                "http://",
                host,
                ".",
                ConfigUtility.BaseDomain,
                relativeUrl
            });
        }
        public static string Combine(string folder, string file)
        {
            return System.IO.Path.GetFullPath(System.IO.Path.Combine(folder, file));
        }
        private static bool IsLetterOrNumber(char i)
        {
            return (i >= 'A' && i <= 'Z') || (i >= 'a' && i <= 'z') || (i >= '0' && i <= '9');
        }
        private static string EncodeToPath(string path)
        {
            char[] chs = path.ToCharArray();
            for (int i = 0; i < path.Length; i++)
            {
                if (!ConfigUtility.IsLetterOrNumber(chs[i]))
                {
                    chs[i] = '_';
                }
            }
            return new string(chs);
        }
        public static int GetIntValue(XmlReader reader, string name, int defaultValue)
        {
            int val;
            if (!int.TryParse(reader.GetAttribute(name), out val))
            {
                val = defaultValue;
            }
            return val;
        }
        public static bool GetBoolValue(XmlReader reader, string name, bool defaultValue)
        {
            bool val;
            if (!bool.TryParse(reader.GetAttribute(name), out val))
            {
                val = defaultValue;
            }
            return val;
        }
        public static string GetStringValue(XmlReader reader, string name, string defaultValue)
        {
            string val = reader.GetAttribute(name);
            if (val == null)
            {
                val = defaultValue;
            }
            return val;
        }
        public static void DumpStack(System.IO.TextWriter writer)
        {
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
            System.Diagnostics.StackFrame[] frames = trace.GetFrames();
            for (int i = 0; i < frames.Length; i++)
            {
                System.Diagnostics.StackFrame frame = frames[i];
                string file = frame.GetFileName();
                int line = frame.GetFileLineNumber();
                frame.GetFileColumnNumber();
                string methodName = frame.GetMethod().Name;
                string clsName = frame.GetMethod().DeclaringType.FullName;
                writer.WriteLine(string.Concat(new object[]
                {
                    clsName,
                    ".",
                    methodName,
                    ",",
                    file,
                    ":",
                    line
                }));
            }
        }
    }
}
