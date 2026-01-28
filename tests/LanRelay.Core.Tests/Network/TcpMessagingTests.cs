using LanRelay.Core.Network;
using System.Net;
using System.Text;

namespace LanRelay.Core.Tests.Network;

public class TcpMessagingTests
{
    private const int TestPort = 59800;

    [Fact]
    public async Task TcpServer_ShouldAcceptConnection()
    {
        // Arrange
        var nicService = new NicService();
        var nics = nicService.GetActiveNicInfos();
        if (nics.Count == 0) return;

        var localIp = nics[0].IPAddress;
        var endpoint = new IPEndPoint(localIp, TestPort);

        using var server = new TcpMessageServer(endpoint);
        var connectionReceived = new TaskCompletionSource<bool>();

        server.OnClientConnected += (_) => connectionReceived.SetResult(true);

        // Act
        server.Start();

        using var client = new TcpMessageClient();
        await client.ConnectAsync(endpoint);

        // Assert
        var result = await connectionReceived.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.True(result);
    }

    [Fact]
    public async Task TcpServer_ShouldReceiveMessage()
    {
        // Arrange
        var nicService = new NicService();
        var nics = nicService.GetActiveNicInfos();
        if (nics.Count == 0) return;

        var localIp = nics[0].IPAddress;
        var endpoint = new IPEndPoint(localIp, TestPort + 1);

        using var server = new TcpMessageServer(endpoint);
        Message? receivedMessage = null;
        var messageReceived = new TaskCompletionSource<bool>();

        server.OnMessageReceived += (_, msg) =>
        {
            receivedMessage = msg;
            messageReceived.SetResult(true);
        };

        server.Start();

        // Act
        using var client = new TcpMessageClient();
        await client.ConnectAsync(endpoint);
        await client.SendAsync(Message.CreateText("Hello, Server!"));

        // Assert
        await messageReceived.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.NotNull(receivedMessage);
        Assert.Equal(MessageType.Text, receivedMessage.Header.Type);
        Assert.Equal("Hello, Server!", Encoding.UTF8.GetString(receivedMessage.Body));
    }

    [Fact]
    public async Task TcpClient_ShouldReceiveMessage()
    {
        // Arrange
        var nicService = new NicService();
        var nics = nicService.GetActiveNicInfos();
        if (nics.Count == 0) return;

        var localIp = nics[0].IPAddress;
        var endpoint = new IPEndPoint(localIp, TestPort + 2);

        using var server = new TcpMessageServer(endpoint);
        TcpClientConnection? serverConnection = null;

        server.OnClientConnected += (conn) => serverConnection = conn;
        server.Start();

        using var client = new TcpMessageClient();
        Message? receivedMessage = null;
        var messageReceived = new TaskCompletionSource<bool>();

        client.OnMessageReceived += (msg) =>
        {
            receivedMessage = msg;
            messageReceived.SetResult(true);
        };

        await client.ConnectAsync(endpoint);

        // Wait for server to get the connection
        await Task.Delay(100);

        // Act - Server sends message to client
        Assert.NotNull(serverConnection);
        await serverConnection.SendAsync(Message.CreateText("Hello, Client!"));

        // Assert
        await messageReceived.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.NotNull(receivedMessage);
        Assert.Equal("Hello, Client!", Encoding.UTF8.GetString(receivedMessage.Body));
    }

    [Fact]
    public async Task TcpMessaging_ShouldHandleMultipleMessages()
    {
        // Arrange
        var nicService = new NicService();
        var nics = nicService.GetActiveNicInfos();
        if (nics.Count == 0) return;

        var localIp = nics[0].IPAddress;
        var endpoint = new IPEndPoint(localIp, TestPort + 3);

        using var server = new TcpMessageServer(endpoint);
        var receivedMessages = new List<string>();
        var allReceived = new TaskCompletionSource<bool>();

        server.OnMessageReceived += (_, msg) =>
        {
            if (msg.Header.Type == MessageType.Text)
            {
                receivedMessages.Add(Encoding.UTF8.GetString(msg.Body));
                if (receivedMessages.Count == 3)
                {
                    allReceived.SetResult(true);
                }
            }
        };

        server.Start();

        // Act
        using var client = new TcpMessageClient();
        await client.ConnectAsync(endpoint);

        await client.SendAsync(Message.CreateText("Message 1"));
        await client.SendAsync(Message.CreateText("Message 2"));
        await client.SendAsync(Message.CreateText("Message 3"));

        // Assert
        await allReceived.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal(3, receivedMessages.Count);
        Assert.Contains("Message 1", receivedMessages);
        Assert.Contains("Message 2", receivedMessages);
        Assert.Contains("Message 3", receivedMessages);
    }

    [Fact]
    public async Task TcpClient_ShouldDetectDisconnection()
    {
        // Arrange
        var nicService = new NicService();
        var nics = nicService.GetActiveNicInfos();
        if (nics.Count == 0) return;

        var localIp = nics[0].IPAddress;
        var endpoint = new IPEndPoint(localIp, TestPort + 4);

        using var server = new TcpMessageServer(endpoint);
        server.Start();

        using var client = new TcpMessageClient();
        var disconnected = new TaskCompletionSource<bool>();

        client.OnDisconnected += () => disconnected.SetResult(true);

        await client.ConnectAsync(endpoint);

        // Act - Stop server (closes connection)
        server.Dispose();

        // Assert
        var result = await disconnected.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.True(result);
    }
}
