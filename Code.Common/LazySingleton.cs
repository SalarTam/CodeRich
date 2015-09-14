using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Common
{
    public class LazySingleton<T> where T : class, new()
    {
        private static readonly Lazy<T> instance = new Lazy<T>(Activator.CreateInstance<T>, true);
        public static T Instance
        {
            get
            {
                return instance.IsValueCreated ? instance.Value : new T();
            }
        }
    }
}
