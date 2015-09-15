using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UnitTestProject1
{
    [XmlRoot("MemCachedConfigInfo")]
    public class MemCachedCollection
    {
        [XmlElement("Item")]
        public MemCachedEntity Items { get; set; }

    }

    public class MemCachedEntity
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
        /// <summary>
        /// 缓存服务器IP和端口号（多台请用","隔开）
        /// </summary>
        [XmlElement("ServerList")]
        public string ServerList { get; set; }
        /// <summary>
        /// 线程池名称
        /// </summary>

        [XmlElement("PoolName")]
        public string PoolName { get; set; }
        /// <summary>
        /// 初始连接数
        /// </summary>
        [XmlElement("IntConnections")]
        public int IntConnections { get; set; }
        /// <summary>
        /// 最小连接数
        /// </summary>
        [XmlElement("MinConnections")]
        public int MinConnections { get; set; }
        /// <summary>
        /// 最大连接数
        /// </summary>
        [XmlElement("MaxConnections")]
        public int MaxConnections { get; set; }
        /// <summary>
        /// Socket链接超时
        /// </summary>
        [XmlElement("SocketConnectTimeout")]
        public int SocketConnectTimeout { get; set; }
        /// <summary>
        /// Socket通讯超时
        /// </summary>
        [XmlElement("SocketTimeout")]
        public int SocketTimeout { get; set; }
        /// <summary>
        /// 维护线程的间隔激活时间
        /// </summary>
        [XmlElement("MaintenanceSleep")]
        public long MaintenanceSleep { get; set; }
        /// <summary>
        /// 缓存路由开启
        /// </summary>
        [XmlElement("FailOver")]
        public bool FailOver { get; set; }
        /// <summary>
        /// 关闭套接字的缓存
        /// </summary>
        [XmlElement("Nagle")]
        public bool Nagle { get; set; }
    }
}
