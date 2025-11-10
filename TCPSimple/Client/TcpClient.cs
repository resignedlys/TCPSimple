using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCPSimple.Common;
using TCPSimple.Extension;

namespace TCPSimple.Client
{
    public class TcpClient : IDisposable
    {
        private readonly System.Net.Sockets.TcpClient _client = new();
        private readonly TcpClientOptions _options;
        private readonly Action<string> _messageReceived; // 消息接收回调
        private bool _isConnected;
        private bool _disposed;
        private NetworkStream _stream;

        public bool IsConnected => _isConnected;
        public string? RemoteEndPoint => _client.Client?.RemoteEndPoint?.ToString();

        public TcpClient(TcpClientOptions options, Action<string> messageReceived)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _messageReceived = messageReceived ?? throw new ArgumentNullException(nameof(messageReceived));
        }

        // 连接服务器
        public async Task ConnectAsync()
        {
            if (_isConnected) return;

            try
            {
                // 连接超时处理（使用 CancellationToken）
                using var cts = new CancellationTokenSource(_options.ConnectTimeout);
                await _client.ConnectAsync(_options.ServerIp, _options.ServerPort, cts.Token);
                _client.ReceiveTimeout = _options.ReceiveTimeout;
                _stream = _client.GetStream();
                _isConnected = true;

                // 启动消息接收循环
                _ = ReceiveMessagesAsync();
            }
            catch (OperationCanceledException)
            {
                throw new TcpConnectionException("连接超时");
            }
            catch (Exception ex)
            {
                throw new TcpConnectionException("连接失败", ex);
            }
        }

        // 异步接收消息（协议解析）
        private async Task ReceiveMessagesAsync()
        {
            try
            {
                var lengthBuffer = new byte[TcpConstants.LengthHeaderSize];

                while (_isConnected && _client.Connected)
                {
                    // 读取长度头
                    var lengthRead = await _stream.ReadExactlyAsync(lengthBuffer, 0, TcpConstants.LengthHeaderSize);
                    if (lengthRead < TcpConstants.LengthHeaderSize) break;

                    // 解析长度并校验
                    var dataLength = ProtocolHelper.DecodeLength(lengthBuffer);
                    if (dataLength <= 0 || dataLength > TcpConstants.MaxMessageSize)
                        throw new TcpProtocolException($"无效消息长度: {dataLength}");

                    // 读取实际数据
                    var dataBuffer = new byte[dataLength];
                    var dataRead = await _stream.ReadExactlyAsync(dataBuffer, 0, dataLength);
                    if (dataRead < dataLength) break;

                    // 触发消息接收事件
                    var message = Encoding.UTF8.GetString(dataBuffer);
                    _messageReceived(message);
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
            finally
            {
                Disconnect();
            }
        }

        // 发送消息到服务器
        public async Task SendAsync(string message)
        {
            if (!_isConnected || !_client.Connected)
                throw new TcpConnectionException("未连接到服务器");

            try
            {
                var data = ProtocolHelper.EncodeMessage(message);
                await _stream.WriteAsync(data, 0, data.Length);
                await _stream.FlushAsync();
            }
            catch (Exception ex)
            {
                throw new TcpConnectionException("发送消息失败", ex);
            }
        }

        // 断开连接
        public void Disconnect()
        {
            if (!_isConnected) return;
            _isConnected = false;
            _stream?.Dispose();
            _client?.Close();
            OnDisconnected();
        }

        // 事件（供外部订阅）
        public event Action Disconnected;
        public event Action<Exception> ErrorOccurred;

        protected virtual void OnDisconnected()
        {
            Disconnected?.Invoke();
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
                Disconnect();
                _stream?.Dispose();
                _client?.Dispose();
            }
            _disposed = true;
        }
    }
}
