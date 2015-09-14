using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Framework.Serializer
{
    public interface ISerialize<T>
    {
        string Serialize(T obj);
        T Deserialize(string stream);
    }
}
