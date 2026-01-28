using LanRelay.Core.State;
using LanRelay.Core.Network;
using System.Net;

namespace LanRelay.Core.Tests.State;

public class DeviceListStateTests
{
    [Fact]
    public void AddOrUpdateDevice_ShouldAddNewDevice()
    {
        // Arrange
        var state = new DeviceListState();
        var deviceInfo = CreateTestDevice("Device1", "192.168.1.10");

        // Act
        state.AddOrUpdateDevice(deviceInfo);

        // Assert
        Assert.Single(state.Devices);
        Assert.Equal("Device1", state.Devices[0].DeviceName);
    }

    [Fact]
    public void AddOrUpdateDevice_ShouldUpdateExistingDevice()
    {
        // Arrange
        var state = new DeviceListState();
        var deviceId = Guid.NewGuid();
        var device1 = CreateTestDevice("Device1", "192.168.1.10", deviceId);
        var device2 = CreateTestDevice("Device1-Updated", "192.168.1.11", deviceId);

        // Act
        state.AddOrUpdateDevice(device1);
        state.AddOrUpdateDevice(device2);

        // Assert
        Assert.Single(state.Devices);
        Assert.Equal("Device1-Updated", state.Devices[0].DeviceName);
        Assert.Equal(IPAddress.Parse("192.168.1.11"), state.Devices[0].IPAddress);
    }

    [Fact]
    public void AddOrUpdateDevice_ShouldRaiseOnDeviceFoundForNewDevice()
    {
        // Arrange
        var state = new DeviceListState();
        var deviceInfo = CreateTestDevice("Device1", "192.168.1.10");
        DeviceInfo? foundDevice = null;
        state.OnDeviceFound += (device) => foundDevice = device;

        // Act
        state.AddOrUpdateDevice(deviceInfo);

        // Assert
        Assert.NotNull(foundDevice);
        Assert.Equal("Device1", foundDevice.DeviceName);
    }

    [Fact]
    public void AddOrUpdateDevice_ShouldNotRaiseOnDeviceFoundForExistingDevice()
    {
        // Arrange
        var state = new DeviceListState();
        var deviceId = Guid.NewGuid();
        var device1 = CreateTestDevice("Device1", "192.168.1.10", deviceId);
        var device2 = CreateTestDevice("Device1-Updated", "192.168.1.10", deviceId);

        int eventCount = 0;
        state.OnDeviceFound += (_) => eventCount++;

        // Act
        state.AddOrUpdateDevice(device1);
        state.AddOrUpdateDevice(device2);

        // Assert
        Assert.Equal(1, eventCount); // Only raised once for the first add
    }

    [Fact]
    public void RemoveDevice_ShouldRaiseOnDeviceLost()
    {
        // Arrange
        var state = new DeviceListState();
        var deviceId = Guid.NewGuid();
        var device = CreateTestDevice("Device1", "192.168.1.10", deviceId);
        state.AddOrUpdateDevice(device);

        DeviceInfo? lostDevice = null;
        state.OnDeviceLost += (device) => lostDevice = device;

        // Act
        state.RemoveDevice(deviceId);

        // Assert
        Assert.NotNull(lostDevice);
        Assert.Equal("Device1", lostDevice.DeviceName);
        Assert.Empty(state.Devices);
    }

    [Fact]
    public void Devices_ShouldBeReadOnlySnapshot()
    {
        // Arrange
        var state = new DeviceListState();
        var device = CreateTestDevice("Device1", "192.168.1.10");

        // Act
        state.AddOrUpdateDevice(device);
        var snapshot = state.Devices;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<DeviceInfo>>(snapshot);
    }

    [Fact]
    public void AddOrUpdateDevice_ShouldUpdateLastSeenTime()
    {
        // Arrange
        var state = new DeviceListState();
        var deviceId = Guid.NewGuid();
        var device1 = CreateTestDevice("Device1", "192.168.1.10", deviceId);

        // Act
        state.AddOrUpdateDevice(device1);
        var firstSeen = state.Devices[0].LastSeen;

        // Wait a tiny bit and update again
        Thread.Sleep(10);
        state.AddOrUpdateDevice(device1);
        var secondSeen = state.Devices[0].LastSeen;

        // Assert
        Assert.True(secondSeen >= firstSeen);
    }

    [Fact]
    public async Task CleanupExpiredDevices_ShouldRemoveStaleDevices()
    {
        // Arrange
        var state = new DeviceListState(heartbeatTimeoutMs: 100);
        var device = CreateTestDevice("Device1", "192.168.1.10");
        state.AddOrUpdateDevice(device);

        DeviceInfo? lostDevice = null;
        state.OnDeviceLost += (d) => lostDevice = d;

        // Act
        await Task.Delay(150); // Wait for timeout
        state.CleanupExpiredDevices();

        // Assert
        Assert.Empty(state.Devices);
        Assert.NotNull(lostDevice);
    }

    [Fact]
    public void CleanupExpiredDevices_ShouldNotRemoveActiveDevices()
    {
        // Arrange
        var state = new DeviceListState(heartbeatTimeoutMs: 5000);
        var device = CreateTestDevice("Device1", "192.168.1.10");
        state.AddOrUpdateDevice(device);

        // Act
        state.CleanupExpiredDevices();

        // Assert
        Assert.Single(state.Devices);
    }

    [Fact]
    public void ProcessDiscoveryPacket_ShouldAddDeviceFromPacket()
    {
        // Arrange
        var state = new DeviceListState();
        var packet = new DiscoveryPacket
        {
            DeviceId = Guid.NewGuid(),
            DeviceName = "RemoteDevice",
            GroupId = "TestGroup"
        };
        var senderIp = IPAddress.Parse("192.168.1.50");

        // Act
        state.ProcessDiscoveryPacket(packet, senderIp);

        // Assert
        Assert.Single(state.Devices);
        Assert.Equal("RemoteDevice", state.Devices[0].DeviceName);
        Assert.Equal(senderIp, state.Devices[0].IPAddress);
        Assert.Equal("TestGroup", state.Devices[0].GroupId);
    }

    private static DeviceInfo CreateTestDevice(string name, string ip, Guid? id = null)
    {
        return new DeviceInfo
        {
            DeviceId = id ?? Guid.NewGuid(),
            DeviceName = name,
            IPAddress = IPAddress.Parse(ip),
            GroupId = "TestGroup",
            LastSeen = DateTime.UtcNow
        };
    }
}
