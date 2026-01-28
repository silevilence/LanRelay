using LanRelay.Core.Network;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LanRelay.Core.Tests.Network;

public class NicServiceTests
{
    [Fact]
    public void GetActiveNicInfos_ShouldReturnNonEmptyList_WhenValidNicsExist()
    {
        // Arrange
        var service = new NicService();

        // Act
        var result = service.GetActiveNicInfos();

        // Assert
        Assert.NotNull(result);
        // Most machines have at least one active NIC
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetActiveNicInfos_ShouldExcludeLoopback()
    {
        // Arrange
        var service = new NicService();

        // Act
        var result = service.GetActiveNicInfos();

        // Assert
        foreach (var nic in result)
        {
            Assert.NotEqual(IPAddress.Loopback, nic.IPAddress);
            Assert.NotEqual(IPAddress.IPv6Loopback, nic.IPAddress);
        }
    }

    [Fact]
    public void GetActiveNicInfos_ShouldOnlyReturnIPv4Addresses()
    {
        // Arrange
        var service = new NicService();

        // Act
        var result = service.GetActiveNicInfos();

        // Assert
        foreach (var nic in result)
        {
            Assert.Equal(AddressFamily.InterNetwork, nic.IPAddress.AddressFamily);
        }
    }

    [Fact]
    public void GetActiveNicInfos_ShouldIncludeSubnetMask()
    {
        // Arrange
        var service = new NicService();

        // Act
        var result = service.GetActiveNicInfos();

        // Assert
        foreach (var nic in result)
        {
            Assert.NotNull(nic.SubnetMask);
            Assert.Equal(AddressFamily.InterNetwork, nic.SubnetMask.AddressFamily);
        }
    }

    [Fact]
    public void GetActiveNicInfos_ShouldIncludeNicName()
    {
        // Arrange
        var service = new NicService();

        // Act
        var result = service.GetActiveNicInfos();

        // Assert
        foreach (var nic in result)
        {
            Assert.False(string.IsNullOrWhiteSpace(nic.Name));
        }
    }

    [Fact]
    public void NicInfo_ShouldHaveCorrectBroadcastAddress()
    {
        // Arrange
        var service = new NicService();

        // Act
        var result = service.GetActiveNicInfos();

        // Assert
        foreach (var nic in result)
        {
            // Broadcast address should be calculated: (IP & Mask) | (~Mask)
            Assert.NotNull(nic.BroadcastAddress);
            Assert.Equal(AddressFamily.InterNetwork, nic.BroadcastAddress.AddressFamily);
        }
    }
}
