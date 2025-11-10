# TCPSimple

> 基于长度前缀法的 TCP 通信库，支持服务端与客户端双向通信，兼容 .NET 5.0+

![版本](https://img.shields.io/github/v/release/your-username/TCPSimple)
![下载量](https://img.shields.io/github/downloads/your-username/TCPSimple/total)
![许可证](https://img.shields.io/github/license/your-username/TCPSimple)


## 🌟 核心特性
- ✅ 自动处理 TCP 粘包/半包问题（基于**长度前缀法**）
- ✅ 支持多客户端连接管理与并发通信
- ✅ 异步非阻塞设计，性能高效
- ✅ 完善的异常处理与事件通知
- ✅ 兼容 .NET 5.0 及以上所有版本


## 🚀 快速上手

### 客户端示例
```csharp
using TCPSimple.Client;
using TCPSimple.Exceptions;
using Newtonsoft.Json;

// 1. 配置客户端
var clientOptions = new TcpClientOptions
{
    ServerIp = "127.0.0.1",       // 服务端 IP
    ServerPort = 8888,            // 服务端端口
    ConnectTimeout = 5000,       // 连接超时（毫秒）
    ReceiveTimeout = 30000       // 接收超时（毫秒）
};

// 2. 实例化客户端并注册消息接收回调
var client = new TcpClient(clientOptions, message =>
    Console.WriteLine($"收到服务端消息: {message}")
);

// 3. 订阅事件（可选）
client.Disconnected += () => 
    Console.WriteLine("与服务端的连接已断开");
client.ErrorOccurred += ex => 
    Console.WriteLine($"发生异常: {JsonConvert.SerializeObject(ex)}");

// 4. 连接并交互
try
{
    await client.ConnectAsync();
    Console.WriteLine("连接成功，输入消息发送（空行退出）:");
    
    string? input;
    while ((input = Console.ReadLine()) != null)
    {
        if (string.IsNullOrEmpty(input)) break;
        await client.SendAsync(input);
    }
}
catch (TcpConnectionException ex)
{
    Console.WriteLine($"连接失败: {ex.Message}");
}
finally
{
    client.Disconnect(); // 主动断开连接
}


## 📦 安装

通过 NuGet 安装（推荐）：
```bash
dotnet add package TCPSimple

### 服务端示例
```csharp
using TCPSimple.Server;
using TCPSimple.Common;
using System.Net;

// 1. 配置服务端
var serverOptions = new TcpServerOptions
{
    IpAddress = IPAddress.Any,    // 监听所有网卡
    Port = 8888,                  // 监听端口
    MaxConnections = 50,          // 最大连接数
    ReceiveTimeout = 30000        // 接收超时（毫秒）
};

// 2. 实例化服务端并注册消息处理逻辑
var server = new TcpServer(serverOptions, async (server, clientId, message) =>
    await HandleMessage(server, clientId, message)
);

// 3. 订阅事件（可选）
server.ClientDisconnected += clientId => 
    Console.WriteLine($"客户端 [{clientId}] 已断开连接");
server.ErrorOccurred += ex => 
    Console.WriteLine($"发生异常: {ex.Message}");

// 4. 启动服务
server.Start();
Console.WriteLine("服务端已启动，按任意键停止...");
Console.ReadKey();

// 5. 停止服务（释放资源）
server.Stop();


// 消息处理方法
async Task HandleMessage(TcpServer server, string clientId, string message)
{
    Console.WriteLine($"收到客户端 [{clientId}] 的消息: {message}");
    // 回复客户端
    await server.SendToClientAsync(clientId, $"服务端已收到: {message}");
    // 广播消息给所有客户端（可选）
    await server.BroadcastAsync($"通知：客户端 [{clientId}] 发送了消息");
}

