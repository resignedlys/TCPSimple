using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPSimple.Common
{
    public static class NetworkStreamExtensions
    {
        // 确保读取指定长度的数据（.NET 5.0 没有内置 ReadExactlyAsync，手动实现）
        public static async Task<int> ReadExactlyAsync(this NetworkStream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                var bytesRead = await stream.ReadAsync(buffer, offset + totalRead, count - totalRead, cancellationToken);
                if (bytesRead == 0) break;
                totalRead += bytesRead;
            }
            return totalRead;
        }
    }
}
