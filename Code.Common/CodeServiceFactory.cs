using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Net;
using System.Configuration;
using System.IO;
using Code.Common;

namespace Zhaopin.ESB.Clinet
{
    public class CodeServiceFactory<T> where T : class
    {
        private static Dictionary<string, object> dictionaryCache = new Dictionary<string, object>();
        private static readonly Object thisLock = new Object();
        //private static string ApplicationName
        //{
        //    get
        //    {
        //        string applicationName = System.Configuration.ConfigurationManager.AppSettings["applicationName"];
        //        if (applicationName != null)
        //        {
        //            return applicationName;
        //        }
        //        else
        //        {
        //            Exception ex = new Exception("ApplicationName in the configuration file can not be empty");
        //            throw ex;
        //        }
        //    }
        //}
        private static string GetImplementName(string ns)
        {
            return ns.Replace("ServiceInterface", "ServiceImp");
        }

        

        private static object CreateImpObject(string serviceNamespace, string typeName)
        {
            Type serviceType = (Type)AssembliesProvider.Instance.GetCurrentAssemblyList().FirstOrDefault(s => s.FullName.Split(',')[0] == serviceNamespace).GetType(typeName);
            if (serviceType != null)
            {
                BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty;
                object objType = serviceType.InvokeMember("Instance", bindingFlags, null, null, null);
                lock (thisLock)
                {
                    if (!dictionaryCache.ContainsKey(typeName))
                    {
                        dictionaryCache.Add(typeName, objType);
                        //logger.Info(String.Format("Add {0} to the Dictionary Cache", objType.ToString()));
                    }
                }

                return objType;
            }
            else
            {
                Exception ex = new Exception(String.Format("The current run program of the directory does not contain implementation dll,Namespace:{0},Type:{1}", serviceNamespace, typeName));
                //logger.HandleException(ex, "Configuration Error");
                throw ex;
            }
        }

        public static T CreateInstance()
        {
            string serviceNamespace = GetImplementName(typeof(T).Namespace);
            string serviceModule = GetImplementName(typeof(T).Module.Name.Substring(0, typeof(T).Module.Name.Length - 4));
            string serviceTypeName = typeof(T).Name.Remove(0, 1);
            //string applicationName = ApplicationName;
            T obj = default(T);
            object objType = null;
                // Whole,Local
            try
            {

                string typeName = String.Format("{0}.{1}", serviceNamespace, serviceTypeName);
                dictionaryCache.TryGetValue(typeName, out objType);
                if (objType == null)
                {
                    try
                    {
                        objType = CreateImpObject(serviceNamespace, typeName);
                    }
                    catch (Exception ex)
                    {
                        //logger.HandleException(ex, "Configuration Error");
                    }
                }
            }
            catch (Exception ex)
            {
                //logger.HandleException(ex, "Configuration Error");
            }
            obj = objType as T;
            return obj;
        }

        

    }
}
