using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCPSimple.Common;
using TCPSimple.Extension;

namespace TCPSimple.Server
{
    public class TcpServer : IDisposable
    {
        private readonly TcpListener _server;
        private readonly TcpServerOptions _options;
        private readonly Func<TcpServer, string, string, Task> _messageHandler; // 消息处理委托
        private readonly Dictionary<string, TcpClient> _clients = new();
        private readonly object _clientLock = new();
        private bool _isRunning;
        private bool _disposed;

        public bool IsRunning => _isRunning;
        public int ConnectedClientsCount => _clients.Count;

        public TcpServer(TcpServerOptions options, Func<TcpServer, string, string, Task> messageHandler)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            _server = new TcpListener(options.IpAddress, options.Port);
        }

        // 启动服务器
        public void Start()
        {
            if (_isRunning) return;
            _server.Start();
            _isRunning = true;
            _ = AcceptClientsAsync(); // 异步接受连接，不阻塞
        }

        // 异步接受客户端连接
        private async Task AcceptClientsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    // 检查连接上限
                    if (ConnectedClientsCount >= _options.MaxConnections)
                    {
                        await Task.Delay(100); // 延迟重试
                        continue;
                    }

                    var client = await _server.AcceptTcpClientAsync();
                    client.ReceiveTimeout = _options.ReceiveTimeout;
                    var remoteEndPoint = client.Client.RemoteEndPoint?.ToString() ?? Guid.NewGuid().ToString();

                    // 线程安全添加客户端
                    lock (_clientLock)
                    {
                        _clients[remoteEndPoint] = client;
                    }

                    // 处理客户端消息
                    _ = HandleClientAsync(client, remoteEndPoint);
                }
                catch (SocketException ex) when (!_isRunning)
                {
                    // 服务器停止时的异常，忽略
                    break;
                }
                catch (Exception ex)
                {
                    // 触发错误事件（可选）
                    OnErrorOccurred(ex);
                }
            }
        }

        // 处理客户端消息（协议解析）
        private async Task HandleClientAsync(TcpClient client, string remoteEndPoint)
        {
            try
            {
                var stream = client.GetStream();
                var lengthBuffer = new byte[TcpConstants.LengthHeaderSize];

                while (_isRunning && client.Connected)
                {
                    // 读取长度头
                    var lengthRead = await stream.ReadExactlyAsync(lengthBuffer, 0, TcpConstants.LengthHeaderSize);
                    if (lengthRead < TcpConstants.LengthHeaderSize) break; // 连接断开

                    // 解析长度并校验
                    var dataLength = ProtocolHelper.DecodeLength(lengthBuffer);
                    if (dataLength <= 0 || dataLength > TcpConstants.MaxMessageSize)
                    {
                        OnErrorOccurred(new TcpProtocolException($"无效消息长度: {dataLength}"));

                        await Task.Delay(500);
                        continue;
                    }

                    // 读取实际数据
                    var dataBuffer = new byte[dataLength];
                    var dataRead = await stream.ReadExactlyAsync(dataBuffer, 0, dataLength);
                    if (dataRead < dataLength) break; // 数据不完整

                    // 调用消息处理委托
                    var message = Encoding.UTF8.GetString(dataBuffer);
                    await _messageHandler(this, remoteEndPoint, message);
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
            finally
            {
                // 移除客户端
                lock (_clientLock)
                {
                    _clients.Remove(remoteEndPoint);
                }
                client.Close();
                OnClientDisconnected(remoteEndPoint);
            }
        }


        // 发送消息给指定客户端
        public async Task SendToClientAsync(string remoteEndPoint, string message)
        {
            if (!_isRunning || string.IsNullOrEmpty(remoteEndPoint)) return;

            TcpClient? client;
            lock (_clientLock)
            {
                _clients.TryGetValue(remoteEndPoint, out client);
            }

            if (client == null || !client.Connected) return;

            try
            {
                var stream = client.GetStream();
                var data = ProtocolHelper.EncodeMessage(message);
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
        }

        // 广播消息给所有客户端
        public async Task BroadcastAsync(string message)
        {
            if (!_isRunning) return;

            List<TcpClient> clientsCopy;
            lock (_clientLock)
            {
                clientsCopy = _clients.Values.ToList();
            }

            foreach (var client in clientsCopy)
            {
                if (client.Connected)
                {
                    try
                    {
                        var stream = client.GetStream();
                        var data = ProtocolHelper.EncodeMessage(message);
                        await stream.WriteAsync(data, 0, data.Length);
                    }
                    catch { /* 忽略单个客户端发送失败 */ }
                }
            }
        }

        // 停止服务器
        public void Stop()
        {
            if (!_isRunning) return;
            _isRunning = false;
            _server.Stop();

            lock (_clientLock)
            {
                foreach (var client in _clients.Values)
                {
                    client.GetStream().Dispose();
                    client.Close();
                }
                _clients.Clear();
            }
        }

        // 事件（供外部订阅）
        public event Action<string> ClientDisconnected;
        public event Action<Exception> ErrorOccurred;

        protected virtual void OnClientDisconnected(string remoteEndPoint)
        {
            ClientDisconnected?.Invoke(remoteEndPoint);
        }

        protected virtual void OnErrorOccurred(Exception ex)
        {
            ErrorOccurred?.Invoke(ex);
        }

        // 释放资源
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                Stop();
                _server?.Server.Dispose();
            }
            _disposed = true;
        }
    }
}