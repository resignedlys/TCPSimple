using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPSimple.Extension
{
    public class TcpProtocolException : Exception
    {
        public TcpProtocolException(string message) : base(message) { }
        public TcpProtocolException(string message, Exception innerException) : base(message, innerException) { }
    }
}
