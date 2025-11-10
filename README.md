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
```

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
```

## 📦 安装

通过 NuGet 安装（推荐）：
```bash
dotnet add package TCPSimple
```
或直接克隆源码编译：
```bash
git clone https://github.com/your-username/TCPSimple.git
cd TCPSimple
dotnet build -c Release
```

## 📚 协议说明（长度前缀法）

消息格式为 **4字节长度头（网络字节序） + 实际数据（UTF-8 编码）**：

- 长度头：`int` 类型，标识后续数据的字节数（需用 `IPAddress.HostToNetworkOrder` 转换为网络字节序）
- 实际数据：业务消息的 UTF-8 字节流


### 示例
发送消息 `"Hello"` 时，实际传输的字节流为：
```plaintext
[0x00, 0x00, 0x00, 0x05]  // 长度头（5字节）
[0x48, 0x65, 0x6c, 0x6c, 0x6f]  // "Hello" 的 UTF-8 字节
```

## ❓ 常见问题

### Q：如何处理大文件传输？
A：可调整 `TcpConstants.MaxMessageSize`（默认 1MB），或**分片传输大文件**（将文件拆分为多个消息，服务端重组）。


### Q：客户端重连逻辑如何实现？
A：可在 `Disconnected` 事件中添加重连逻辑：
```csharp
client.Disconnected += async () =>
{
    Console.WriteLine("尝试重连...");
    await Task.Delay(3000);
    try { await client.ConnectAsync(); }
    catch { /* 重连失败处理 */ }
};
```
### Q：支持跨平台吗？
A：完全支持，可在 Windows、Linux、macOS 上运行（需安装对应平台的 .NET SDK）。


## 🛠️ 贡献指南

1. Fork 本仓库
2. 新建分支 `git checkout -b feature/your-feature`
3. 提交代码并发起 Pull Request
4. 等待代码审查与合并


## 📄 许可证

MIT © [your-username]

将文档中的 `your-username` 替换为你的 GitHub 用户名后，即可直接用于仓库的 `README.md`，整体结构和格式完全统一。
