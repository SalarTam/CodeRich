using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Common.XML
{
    public interface IMultiNamespaceObject
    {
        /// <summary>
        /// 命名空间别名（key：别名，value：命名空间）
        /// 请标识[XmlIgnore]以防止序列化此属性
        /// </summary>
        Dictionary<string, string> NamespaceAlias
        {
            get;
        }
    }
}
