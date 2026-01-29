using System.Net;
using LanRelay.Core.Network;
using LanRelay.Core.State;

namespace LanRelay.Core.Tests.State;

/// <summary>
/// Tests for filtering devices by group.
/// </summary>
public class DeviceFilterTests
{
    [Fact]
    public void DeviceListState_ShouldFilterByGroupId()
    {
        // Arrange
        var state = new DeviceListState();

        // Add devices from different groups
        state.ProcessDiscoveryPacket(new DiscoveryPacket
        {
            DeviceId = Guid.NewGuid(),
            DeviceName = "Device-A",
            GroupId = "TeamAlpha"
        }, IPAddress.Parse("192.168.1.10"));

        state.ProcessDiscoveryPacket(new DiscoveryPacket
        {
            DeviceId = Guid.NewGuid(),
            DeviceName = "Device-B",
            GroupId = "TeamBeta"
        }, IPAddress.Parse("192.168.1.11"));

        state.ProcessDiscoveryPacket(new DiscoveryPacket
        {
            DeviceId = Guid.NewGuid(),
            DeviceName = "Device-C",
            GroupId = "TeamAlpha"
        }, IPAddress.Parse("192.168.1.12"));

        // Act
        var alphaDevices = state.GetDevicesByGroup("TeamAlpha");
        var betaDevices = state.GetDevicesByGroup("TeamBeta");

        // Assert
        Assert.Equal(2, alphaDevices.Count);
        Assert.Single(betaDevices);
        Assert.All(alphaDevices, d => Assert.Equal("TeamAlpha", d.GroupId));
    }

    [Fact]
    public void DeviceListState_ShouldReturnEmptyForUnknownGroup()
    {
        // Arrange
        var state = new DeviceListState();
        state.ProcessDiscoveryPacket(new DiscoveryPacket
        {
            DeviceId = Guid.NewGuid(),
            DeviceName = "Device-A",
            GroupId = "ExistingGroup"
        }, IPAddress.Parse("192.168.1.10"));

        // Act
        var devices = state.GetDevicesByGroup("NonExistentGroup");

        // Assert
        Assert.Empty(devices);
    }

    [Fact]
    public void DeviceListState_ShouldFilterCurrentGroupDevices()
    {
        // Arrange
        var deviceState = new DeviceListState();
        var groupState = new GroupState();

        // Add devices
        deviceState.ProcessDiscoveryPacket(new DiscoveryPacket
        {
            DeviceId = Guid.NewGuid(),
            DeviceName = "MyTeam-1",
            GroupId = "MyTeam"
        }, IPAddress.Parse("192.168.1.10"));

        deviceState.ProcessDiscoveryPacket(new DiscoveryPacket
        {
            DeviceId = Guid.NewGuid(),
            DeviceName = "OtherTeam-1",
            GroupId = "OtherTeam"
        }, IPAddress.Parse("192.168.1.11"));

        // Join MyTeam
        groupState.CreateGroup("MyTeam");
        groupState.JoinGroup("MyTeam");

        // Act
        var currentGroupDevices = deviceState.GetDevicesByGroup(groupState.CurrentGroupId);

        // Assert
        Assert.Single(currentGroupDevices);
        Assert.Equal("MyTeam-1", currentGroupDevices[0].DeviceName);
    }
}
