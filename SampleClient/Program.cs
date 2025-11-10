// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using TCPSimple.Client;
using TCPSimple.Extension;

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