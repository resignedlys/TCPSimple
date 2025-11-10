using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPSimple.Client
{
    public class TcpClientOptions
    {
        public string ServerIp { get; set; } = "127.0.0.1";
        public int ServerPort { get; set; } = 8888;
        public int ConnectTimeout { get; set; } = 5000; // 连接超时（毫秒）
        public int ReceiveTimeout { get; set; } = 30000; // 接收超时（毫秒）
    }
}
