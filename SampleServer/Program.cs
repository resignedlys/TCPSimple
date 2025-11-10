// See https://aka.ms/new-console-template for more information
using System.Net;
using TCPSimple.Server;
using TCPSimple.Extension;

// 配置服务端
var serverOptions = new TcpServerOptions
{
    IpAddress = IPAddress.Any,
    Port = 8888,
    MaxConnections = 50
};

// 实例化服务端（消息处理委托）
var server = new TcpServer(serverOptions, async (server, clientId, message) => await Received(server, clientId, message));

// 订阅事件
server.ClientDisconnected += (clientId) => Console.WriteLine($"客户端 {clientId} 断开");
server.ErrorOccurred += (ex) => Console.WriteLine($"错误: {ex.Message}");

// 启动服务
server.Start();
Console.WriteLine("服务端启动，按任意键停止...");
Console.ReadKey();
server.Stop();


async Task Received(TcpServer server, string clientId, string message)
{
    Console.WriteLine($"收到客户端 {clientId} 的消息: {message}");
    // 此时引用 server 是安全的
    await server.SendToClientAsync(clientId, $"已收到: {message}");
}
