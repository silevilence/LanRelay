using LanRelay.Core.Network;
using System.Text.Json;

namespace LanRelay.Core.Tests.Network;

public class DiscoveryPacketTests
{
    [Fact]
    public void Serialize_ShouldProduceValidJson()
    {
        // Arrange
        var packet = new DiscoveryPacket
        {
            DeviceId = Guid.Parse("12345678-1234-1234-1234-123456789abc"),
            DeviceName = "TestDevice",
            GroupId = "LanRelay-Default"
        };

        // Act
        var json = packet.Serialize();

        // Assert
        Assert.NotNull(json);
        // JSON uses shortened property names: v, id, name, group
        Assert.Contains("\"id\":", json);
        Assert.Contains("12345678-1234-1234-1234-123456789abc", json);
        Assert.Contains("TestDevice", json);
        Assert.Contains("LanRelay-Default", json);
    }

    [Fact]
    public void Deserialize_ShouldRestorePacket()
    {
        // Arrange
        var original = new DiscoveryPacket
        {
            DeviceId = Guid.Parse("aaaabbbb-cccc-dddd-eeee-ffffffffffff"),
            DeviceName = "MyComputer",
            GroupId = "MyGroup"
        };
        var json = original.Serialize();

        // Act
        var restored = DiscoveryPacket.Deserialize(json);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(original.DeviceId, restored.DeviceId);
        Assert.Equal(original.DeviceName, restored.DeviceName);
        Assert.Equal(original.GroupId, restored.GroupId);
    }

    [Fact]
    public void Deserialize_ShouldReturnNull_ForInvalidJson()
    {
        // Arrange
        var invalidJson = "{ invalid json }}}";

        // Act
        var result = DiscoveryPacket.Deserialize(invalidJson);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Deserialize_ShouldReturnNull_ForEmptyString()
    {
        // Act
        var result = DiscoveryPacket.Deserialize("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Deserialize_ShouldReturnNull_ForNullInput()
    {
        // Act
        var result = DiscoveryPacket.Deserialize(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ToBytes_ShouldReturnUtf8Bytes()
    {
        // Arrange
        var packet = new DiscoveryPacket
        {
            DeviceId = Guid.NewGuid(),
            DeviceName = "ByteTest",
            GroupId = "Group1"
        };

        // Act
        var bytes = packet.ToBytes();

        // Assert
        Assert.NotNull(bytes);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void FromBytes_ShouldRestorePacket()
    {
        // Arrange
        var original = new DiscoveryPacket
        {
            DeviceId = Guid.NewGuid(),
            DeviceName = "FromBytesTest",
            GroupId = "TestGroup"
        };
        var bytes = original.ToBytes();

        // Act
        var restored = DiscoveryPacket.FromBytes(bytes);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(original.DeviceId, restored.DeviceId);
        Assert.Equal(original.DeviceName, restored.DeviceName);
        Assert.Equal(original.GroupId, restored.GroupId);
    }

    [Fact]
    public void DiscoveryPacket_ShouldHaveProtocolVersion()
    {
        // Arrange
        var packet = new DiscoveryPacket
        {
            DeviceId = Guid.NewGuid(),
            DeviceName = "Test",
            GroupId = "Group"
        };

        // Assert
        Assert.Equal(1, packet.ProtocolVersion);
    }
}
