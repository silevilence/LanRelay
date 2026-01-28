using LanRelay.Core.Network;
using System.Net;
using System.Net.Sockets;

namespace LanRelay.Core.Tests.Network;

public class UdpBroadcasterTests
{
    private const int TestPort = 59999; // Use a high port for testing

    [Fact]
    public void Constructor_ShouldBindToSpecificLocalEndpoint()
    {
        // Arrange
        var nicService = new NicService();
        var nics = nicService.GetActiveNicInfos();

        // Skip test if no active NICs (unlikely in normal scenarios)
        if (nics.Count == 0)
        {
            return;
        }

        var nic = nics[0];
        var localEndpoint = new IPEndPoint(nic.IPAddress, TestPort);

        // Act & Assert
        using var broadcaster = new UdpBroadcaster(localEndpoint);
        Assert.NotNull(broadcaster);
        Assert.Equal(localEndpoint, broadcaster.LocalEndpoint);
    }

    [Fact]
    public async Task SendAsync_ShouldNotThrow()
    {
        // Arrange
        var nicService = new NicService();
        var nics = nicService.GetActiveNicInfos();

        if (nics.Count == 0)
        {
            return;
        }

        var nic = nics[0];
        // Bind to local IP, send to broadcast address
        var localEndpoint = new IPEndPoint(nic.IPAddress, 0); // Port 0 = auto-assign
        var broadcastEndpoint = new IPEndPoint(nic.BroadcastAddress, TestPort + 1);

        using var broadcaster = new UdpBroadcaster(localEndpoint);

        var packet = new DiscoveryPacket
        {
            DeviceId = Guid.NewGuid(),
            DeviceName = "TestDevice",
            GroupId = "TestGroup"
        };

        // Act & Assert - Should not throw
        await broadcaster.SendAsync(packet.ToBytes(), broadcastEndpoint);
    }

    [Fact]
    public async Task SendAndReceive_ShouldWorkOnSameSubnet()
    {
        // Arrange
        var nicService = new NicService();
        var nics = nicService.GetActiveNicInfos();

        if (nics.Count == 0)
        {
            return;
        }

        var nic = nics[0];
        var port = TestPort + 2;
        var senderEndpoint = new IPEndPoint(nic.IPAddress, port);
        var receiverEndpoint = new IPEndPoint(nic.IPAddress, port + 1);
        var targetEndpoint = new IPEndPoint(nic.IPAddress, port + 1);

        var originalPacket = new DiscoveryPacket
        {
            DeviceId = Guid.NewGuid(),
            DeviceName = "IntegrationTest",
            GroupId = "TestGroup"
        };

        using var sender = new UdpBroadcaster(senderEndpoint);
        using var receiver = new UdpBroadcaster(receiverEndpoint);

        // Act
        var receiveTask = receiver.ReceiveAsync(CancellationToken.None);

        // Give receiver time to start
        await Task.Delay(50);

        await sender.SendAsync(originalPacket.ToBytes(), targetEndpoint);

        var result = await receiveTask.WaitAsync(TimeSpan.FromSeconds(2));

        // Assert
        Assert.NotNull(result.Data);
        var receivedPacket = DiscoveryPacket.FromBytes(result.Data);
        Assert.NotNull(receivedPacket);
        Assert.Equal(originalPacket.DeviceId, receivedPacket.DeviceId);
        Assert.Equal(originalPacket.DeviceName, receivedPacket.DeviceName);
    }

    [Fact]
    public async Task ReceiveAsync_ShouldBeCancellable()
    {
        // Arrange
        var nicService = new NicService();
        var nics = nicService.GetActiveNicInfos();

        if (nics.Count == 0)
        {
            return;
        }

        var nic = nics[0];
        var localEndpoint = new IPEndPoint(nic.IPAddress, TestPort + 10);

        using var broadcaster = new UdpBroadcaster(localEndpoint);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await broadcaster.ReceiveAsync(cts.Token);
        });
    }

    [Fact]
    public void Dispose_ShouldReleaseResources()
    {
        // Arrange
        var nicService = new NicService();
        var nics = nicService.GetActiveNicInfos();

        if (nics.Count == 0)
        {
            return;
        }

        var nic = nics[0];
        var localEndpoint = new IPEndPoint(nic.IPAddress, TestPort + 20);

        // Act
        var broadcaster = new UdpBroadcaster(localEndpoint);
        broadcaster.Dispose();

        // Assert - Creating a new broadcaster on the same port should work
        using var newBroadcaster = new UdpBroadcaster(localEndpoint);
        Assert.NotNull(newBroadcaster);
    }
}
