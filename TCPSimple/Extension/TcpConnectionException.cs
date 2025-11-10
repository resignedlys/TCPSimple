using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPSimple.Extension
{
    public class TcpConnectionException : Exception
    {
        public TcpConnectionException(string message) : base(message) { }
        public TcpConnectionException(string message, Exception innerException) : base(message, innerException) { }
    }
}
