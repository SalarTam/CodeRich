using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject1.Config
{
    public class CampusConnectionUtility
    {
        private static string GetRemoteCampusDbConnectionConfig(string key)
        {
            return CampusConnectionCollection.Instance[key];
        }
        public static string Evareg
        {
            get
            {
                return GetRemoteCampusDbConnectionConfig("Evareg");

            }
        }
    }
}
