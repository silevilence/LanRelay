using LanRelay.Core.Network;
using System.Text;

namespace LanRelay.Core.Tests.Network;

public class MessageProtocolTests
{
    [Fact]
    public void MessageType_ShouldHaveExpectedValues()
    {
        // Assert - Verify message types exist
        Assert.Equal(0, (int)MessageType.Heartbeat);
        Assert.Equal(1, (int)MessageType.Text);
        Assert.Equal(2, (int)MessageType.FileRequest);
        Assert.Equal(3, (int)MessageType.FileData);
        Assert.Equal(4, (int)MessageType.FileAck);
    }

    [Fact]
    public void MessageHeader_ShouldSerializeCorrectly()
    {
        // Arrange
        var header = new MessageHeader
        {
            Type = MessageType.Text,
            BodyLength = 100
        };

        // Act
        var bytes = header.ToBytes();

        // Assert
        Assert.Equal(MessageHeader.HeaderSize, bytes.Length);
    }

    [Fact]
    public void MessageHeader_ShouldDeserializeCorrectly()
    {
        // Arrange
        var original = new MessageHeader
        {
            Type = MessageType.FileRequest,
            BodyLength = 1024
        };
        var bytes = original.ToBytes();

        // Act
        var restored = MessageHeader.FromBytes(bytes);

        // Assert
        Assert.Equal(original.Type, restored.Type);
        Assert.Equal(original.BodyLength, restored.BodyLength);
    }

    [Fact]
    public void Message_ShouldCreateTextMessage()
    {
        // Arrange
        var text = "Hello, World!";

        // Act
        var message = Message.CreateText(text);

        // Assert
        Assert.Equal(MessageType.Text, message.Header.Type);
        Assert.Equal(Encoding.UTF8.GetByteCount(text), message.Header.BodyLength);
        Assert.Equal(text, Encoding.UTF8.GetString(message.Body));
    }

    [Fact]
    public void Message_ShouldCreateHeartbeat()
    {
        // Act
        var message = Message.CreateHeartbeat();

        // Assert
        Assert.Equal(MessageType.Heartbeat, message.Header.Type);
        Assert.Equal(0, message.Header.BodyLength);
        Assert.Empty(message.Body);
    }

    [Fact]
    public void Message_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var originalText = "测试中文消息";
        var original = Message.CreateText(originalText);

        // Act
        var bytes = original.ToBytes();
        var restored = Message.FromBytes(bytes);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(original.Header.Type, restored.Header.Type);
        Assert.Equal(originalText, Encoding.UTF8.GetString(restored.Body));
    }

    [Fact]
    public void MessageHeader_FromBytes_ShouldReturnNull_ForInvalidData()
    {
        // Arrange
        var invalidBytes = new byte[3]; // Too short

        // Act
        var result = MessageHeader.FromBytes(invalidBytes);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Message_FromBytes_ShouldReturnNull_ForInvalidData()
    {
        // Arrange
        var invalidBytes = new byte[2]; // Too short

        // Act
        var result = Message.FromBytes(invalidBytes);

        // Assert
        Assert.Null(result);
    }
}
