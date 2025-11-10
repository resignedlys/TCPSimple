# TCPSimple
TCP、SOCKET Server、Client（采用：长度前缀法）, Compatible with.NET 5.0+


# Server
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

# Client
// 配置客户端
var clientOptions = new TcpClientOptions
{
    ServerIp = "127.0.0.1",
    ServerPort = 8888
};

// 实例化客户端（消息接收回调）
var client = new TcpClient(clientOptions, (message) =>
{
    Console.WriteLine($"收到服务端消息: {message}");
});

// 订阅事件
client.Disconnected += () => Console.WriteLine("与服务端断开连接");
client.ErrorOccurred += (ex) => Console.WriteLine($"错误: {JsonConvert.SerializeObject(ex)}");

// 连接并发送消息
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
    client.Disconnect();
}
