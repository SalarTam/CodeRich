using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Code.Common
{
    internal class AssembliesProvider
    {
        private static readonly AssembliesProvider instance = new AssembliesProvider();

        private Assembly[] Assemblies;

        public static AssembliesProvider Instance
        {
            get
            {
                return AssembliesProvider.instance;
            }
        }

        private AssembliesProvider()
        {
        }

        public Assembly[] GetCurrentAssemblyList()
        {
            if (this.Assemblies != null && this.Assemblies.Length > 0)
            {
                return this.Assemblies;
            }
            this.Assemblies = this.GetAssemblies();
            return this.Assemblies;
        }

        private Assembly[] GetAssemblies()
        {
            string path = Assembly.GetExecutingAssembly().CodeBase;
            path = Path.GetDirectoryName(path).Replace("file:\\", "");
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] files = directoryInfo.GetFiles("*.dll");
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo fileInfo = files[i];
                string assemblyString = fileInfo.Name.Replace(fileInfo.Extension, "");
                AppDomain.CurrentDomain.Load(assemblyString);
            }
            return AppDomain.CurrentDomain.GetAssemblies();
        }
    }
}
