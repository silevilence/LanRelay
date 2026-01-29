using System.Net;
using LanRelay.Core.Network;
using LanRelay.Core.State;

namespace LanRelay.Core.Tests.Network;

/// <summary>
/// Tests for relay discovery logic (Gossip protocol).
/// Simulates a three-node topology: A (Subnet1) -- B (Bridge) -- C (Subnet2)
/// </summary>
public class RelayDiscoveryTests
{
    private static readonly Guid DeviceA_Id = Guid.NewGuid();
    private static readonly Guid DeviceB_Id = Guid.NewGuid();
    private static readonly Guid DeviceC_Id = Guid.NewGuid();

    private static readonly IPAddress SubnetA_IP = IPAddress.Parse("192.168.1.10");
    private static readonly IPAddress BridgeB_SubnetA_IP = IPAddress.Parse("192.168.1.1");
    private static readonly IPAddress BridgeB_SubnetC_IP = IPAddress.Parse("10.0.0.1");
    private static readonly IPAddress SubnetC_IP = IPAddress.Parse("10.0.0.10");

    [Fact]
    public void DiscoveryPacket_ShouldSupportKnownDevicesList()
    {
        // Arrange - B knows about A and wants to share this info
        var knownDevice = new KnownDeviceInfo
        {
            DeviceId = DeviceA_Id,
            DeviceName = "Device-A",
            OriginIP = SubnetA_IP.ToString(),
            HopCount = 1
        };

        var packet = new DiscoveryPacket
        {
            DeviceId = DeviceB_Id,
            DeviceName = "Bridge-B",
            GroupId = "default",
            KnownDevices = [knownDevice]
        };

        // Act
        var json = packet.Serialize();
        var deserialized = DiscoveryPacket.Deserialize(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Single(deserialized.KnownDevices);
        Assert.Equal(DeviceA_Id, deserialized.KnownDevices[0].DeviceId);
        Assert.Equal("Device-A", deserialized.KnownDevices[0].DeviceName);
        Assert.Equal(1, deserialized.KnownDevices[0].HopCount);
    }

    [Fact]
    public void DeviceListState_ShouldAddRelayedDevice()
    {
        // Arrange - This simulates C's state (C only knows what it receives)
        var stateC = new DeviceListState();

        // B advertises A's info to C (Gossip protocol)
        var knownA = new KnownDeviceInfo
        {
            DeviceId = DeviceA_Id,
            DeviceName = "Device-A",
            OriginIP = SubnetA_IP.ToString(),
            HopCount = 1 // B is 1 hop from A (direct)
        };

        var packetFromB = new DiscoveryPacket
        {
            DeviceId = DeviceB_Id,
            DeviceName = "Bridge-B",
            GroupId = "default",
            KnownDevices = [knownA]
        };

        // Act - C processes B's packet (which includes A's info)
        stateC.ProcessDiscoveryPacket(packetFromB, BridgeB_SubnetC_IP);

        // Assert - C should now know about both A (relayed) and B (direct)
        var devices = stateC.Devices;
        Assert.Equal(2, devices.Count);

        // Check B is direct
        var deviceB = devices.FirstOrDefault(d => d.DeviceId == DeviceB_Id);
        Assert.NotNull(deviceB);
        Assert.True(deviceB.IsDirectConnection);

        // Check A is relayed via B
        var deviceA = devices.FirstOrDefault(d => d.DeviceId == DeviceA_Id);
        Assert.NotNull(deviceA);
        Assert.False(deviceA.IsDirectConnection); // A is via relay
        Assert.Equal(DeviceB_Id, deviceA.RelayDeviceId); // Relayed through B
    }

    [Fact]
    public void DeviceListState_ShouldPreferDirectConnection()
    {
        // Arrange
        var state = new DeviceListState();

        // First: C learns about A via B (relayed, 2 hops)
        var knownA = new KnownDeviceInfo
        {
            DeviceId = DeviceA_Id,
            DeviceName = "Device-A",
            OriginIP = SubnetA_IP.ToString(),
            HopCount = 2
        };

        var packetFromB = new DiscoveryPacket
        {
            DeviceId = DeviceB_Id,
            DeviceName = "Bridge-B",
            GroupId = "default",
            KnownDevices = [knownA]
        };
        state.ProcessDiscoveryPacket(packetFromB, BridgeB_SubnetC_IP);

        // Then: A somehow becomes directly reachable (network change)
        var packetFromA = new DiscoveryPacket
        {
            DeviceId = DeviceA_Id,
            DeviceName = "Device-A",
            GroupId = "default"
        };

        // Act
        state.ProcessDiscoveryPacket(packetFromA, SubnetA_IP);

        // Assert - Direct connection should be preferred
        var deviceA = state.Devices.FirstOrDefault(d => d.DeviceId == DeviceA_Id);
        Assert.NotNull(deviceA);
        Assert.True(deviceA.IsDirectConnection);
        Assert.Null(deviceA.RelayDeviceId);
    }

    [Fact]
    public void DeviceListState_ShouldTrackHopCount()
    {
        // Arrange
        var state = new DeviceListState();

        // B advertises A with HopCount = 1
        var knownA = new KnownDeviceInfo
        {
            DeviceId = DeviceA_Id,
            DeviceName = "Device-A",
            OriginIP = SubnetA_IP.ToString(),
            HopCount = 1
        };

        var packetFromB = new DiscoveryPacket
        {
            DeviceId = DeviceB_Id,
            DeviceName = "Bridge-B",
            GroupId = "default",
            KnownDevices = [knownA]
        };

        // Act
        state.ProcessDiscoveryPacket(packetFromB, BridgeB_SubnetC_IP);

        // Assert - C should record A with HopCount = 2 (1 + 1)
        var deviceA = state.Devices.FirstOrDefault(d => d.DeviceId == DeviceA_Id);
        Assert.NotNull(deviceA);
        Assert.Equal(2, deviceA.HopCount);
    }

    [Fact]
    public void DeviceListState_ShouldPreferLowerHopCount()
    {
        // Arrange
        var state = new DeviceListState();

        // First path: A via B via D (3 hops)
        var knownA_3hops = new KnownDeviceInfo
        {
            DeviceId = DeviceA_Id,
            DeviceName = "Device-A",
            OriginIP = SubnetA_IP.ToString(),
            HopCount = 3
        };

        var packetFromD = new DiscoveryPacket
        {
            DeviceId = Guid.NewGuid(), // Device D
            DeviceName = "Device-D",
            GroupId = "default",
            KnownDevices = [knownA_3hops]
        };
        state.ProcessDiscoveryPacket(packetFromD, IPAddress.Parse("172.16.0.1"));

        // Second path: A via B (2 hops) - shorter!
        var knownA_1hop = new KnownDeviceInfo
        {
            DeviceId = DeviceA_Id,
            DeviceName = "Device-A",
            OriginIP = SubnetA_IP.ToString(),
            HopCount = 1
        };

        var packetFromB = new DiscoveryPacket
        {
            DeviceId = DeviceB_Id,
            DeviceName = "Bridge-B",
            GroupId = "default",
            KnownDevices = [knownA_1hop]
        };

        // Act
        state.ProcessDiscoveryPacket(packetFromB, BridgeB_SubnetC_IP);

        // Assert - Should use the shorter path (2 hops via B)
        var deviceA = state.Devices.FirstOrDefault(d => d.DeviceId == DeviceA_Id);
        Assert.NotNull(deviceA);
        Assert.Equal(2, deviceA.HopCount); // 1 + 1 = 2
        Assert.Equal(DeviceB_Id, deviceA.RelayDeviceId);
    }

    [Fact]
    public void NicService_ShouldIdentifyBridgeNode()
    {
        // Arrange
        var nicService = new NicService();

        // Act
        var nics = nicService.GetActiveNics();
        var isBridge = nics.Count >= 2;

        // Assert - This test just verifies the API exists
        // In real scenario, a bridge has 2+ NICs on different subnets
        Assert.NotNull(nics);
    }

    [Fact]
    public void DiscoveryPacket_ShouldIncludeMultipleKnownDevices()
    {
        // Arrange - B knows about both A and C
        var knownA = new KnownDeviceInfo
        {
            DeviceId = DeviceA_Id,
            DeviceName = "Device-A",
            OriginIP = SubnetA_IP.ToString(),
            HopCount = 1
        };

        var knownC = new KnownDeviceInfo
        {
            DeviceId = DeviceC_Id,
            DeviceName = "Device-C",
            OriginIP = SubnetC_IP.ToString(),
            HopCount = 1
        };

        var packet = new DiscoveryPacket
        {
            DeviceId = DeviceB_Id,
            DeviceName = "Bridge-B",
            GroupId = "default",
            KnownDevices = [knownA, knownC]
        };

        // Act
        var json = packet.Serialize();
        var deserialized = DiscoveryPacket.Deserialize(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.KnownDevices.Count);
    }
}
