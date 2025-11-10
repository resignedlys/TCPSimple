using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TCPSimple.Server
{
    public class TcpServerOptions
    {
        public IPAddress IpAddress { get; set; } = IPAddress.Any;
        public int Port { get; set; } = 8888;
        public int MaxConnections { get; set; } = 100; // 默认最大连接数
        public int ReceiveTimeout { get; set; } = 30000; // 接收超时（毫秒）
    }

}
