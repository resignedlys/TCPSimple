using System;
using System.Net;
using System.Text;

namespace TCPSimple.Common
{
    public static class ProtocolHelper
    {
        // 编码消息（长度前缀+数据）
        public static byte[] EncodeMessage(string data)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            // 长度头转换为网络字节序（大端序）
            var length = IPAddress.HostToNetworkOrder(dataBytes.Length);
            var lengthBytes = BitConverter.GetBytes(length);

            // 合并长度头和数据
            var buffer = new byte[TcpConstants.LengthHeaderSize + dataBytes.Length];
            Buffer.BlockCopy(lengthBytes, 0, buffer, 0, TcpConstants.LengthHeaderSize);
            Buffer.BlockCopy(dataBytes, 0, buffer, TcpConstants.LengthHeaderSize, dataBytes.Length);
            return buffer;
        }

        // 解码长度头（从网络字节序转主机字节序）
        public static int DecodeLength(byte[] lengthBytes)
        {
            if (lengthBytes.Length != TcpConstants.LengthHeaderSize)
                throw new ArgumentException("长度头必须为4字节", nameof(lengthBytes));

            var length = BitConverter.ToInt32(lengthBytes, 0);
            return IPAddress.NetworkToHostOrder(length);
        }
    }
}
