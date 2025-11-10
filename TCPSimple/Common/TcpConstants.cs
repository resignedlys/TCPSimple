using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPSimple.Common
{
    public static class TcpConstants
    {
        // 协议常量（长度前缀为4字节int，最大消息1MB）
        public const int LengthHeaderSize = 4;
        public const int MaxMessageSize = 1024 * 1024; // 1MB
    }

}
