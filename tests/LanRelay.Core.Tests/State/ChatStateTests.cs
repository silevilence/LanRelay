using LanRelay.Core.State;
using System.Net;

namespace LanRelay.Core.Tests.State;

public class ChatStateTests
{
    [Fact]
    public void AddMessage_ShouldAddToChatHistory()
    {
        // Arrange
        var state = new ChatState();
        var deviceId = Guid.NewGuid();
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            Content = "Hello!",
            Timestamp = DateTime.UtcNow,
            IsOutgoing = false
        };

        // Act
        state.AddMessage(deviceId, message);

        // Assert
        var history = state.GetChatHistory(deviceId);
        Assert.Single(history);
        Assert.Equal("Hello!", history[0].Content);
    }

    [Fact]
    public void AddMessage_ShouldRaiseOnMessageReceived()
    {
        // Arrange
        var state = new ChatState();
        var deviceId = Guid.NewGuid();
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            Content = "Test",
            Timestamp = DateTime.UtcNow,
            IsOutgoing = false
        };

        ChatMessage? receivedMessage = null;
        state.OnMessageReceived += (id, msg) => receivedMessage = msg;

        // Act
        state.AddMessage(deviceId, message);

        // Assert
        Assert.NotNull(receivedMessage);
        Assert.Equal("Test", receivedMessage.Content);
    }

    [Fact]
    public void GetChatHistory_ShouldReturnEmptyForUnknownDevice()
    {
        // Arrange
        var state = new ChatState();

        // Act
        var history = state.GetChatHistory(Guid.NewGuid());

        // Assert
        Assert.Empty(history);
    }

    [Fact]
    public void SelectDevice_ShouldUpdateSelectedDeviceId()
    {
        // Arrange
        var state = new ChatState();
        var deviceId = Guid.NewGuid();

        // Act
        state.SelectDevice(deviceId);

        // Assert
        Assert.Equal(deviceId, state.SelectedDeviceId);
    }

    [Fact]
    public void SelectDevice_ShouldRaiseOnSelectedDeviceChanged()
    {
        // Arrange
        var state = new ChatState();
        var deviceId = Guid.NewGuid();
        Guid? changedDeviceId = null;

        state.OnSelectedDeviceChanged += (id) => changedDeviceId = id;

        // Act
        state.SelectDevice(deviceId);

        // Assert
        Assert.Equal(deviceId, changedDeviceId);
    }

    [Fact]
    public void ClearChat_ShouldRemoveAllMessagesForDevice()
    {
        // Arrange
        var state = new ChatState();
        var deviceId = Guid.NewGuid();
        state.AddMessage(deviceId, CreateMessage(deviceId, "Msg 1"));
        state.AddMessage(deviceId, CreateMessage(deviceId, "Msg 2"));

        // Act
        state.ClearChat(deviceId);

        // Assert
        Assert.Empty(state.GetChatHistory(deviceId));
    }

    [Fact]
    public void SelectedChatHistory_ShouldReturnHistoryForSelectedDevice()
    {
        // Arrange
        var state = new ChatState();
        var deviceId = Guid.NewGuid();
        state.AddMessage(deviceId, CreateMessage(deviceId, "Hello"));
        state.SelectDevice(deviceId);

        // Act
        var history = state.SelectedChatHistory;

        // Assert
        Assert.Single(history);
    }

    [Fact]
    public void MessageHistory_ShouldMaintainOrder()
    {
        // Arrange
        var state = new ChatState();
        var deviceId = Guid.NewGuid();

        // Act
        state.AddMessage(deviceId, CreateMessage(deviceId, "First"));
        state.AddMessage(deviceId, CreateMessage(deviceId, "Second"));
        state.AddMessage(deviceId, CreateMessage(deviceId, "Third"));

        // Assert
        var history = state.GetChatHistory(deviceId);
        Assert.Equal(3, history.Count);
        Assert.Equal("First", history[0].Content);
        Assert.Equal("Second", history[1].Content);
        Assert.Equal("Third", history[2].Content);
    }

    private static ChatMessage CreateMessage(Guid deviceId, string content)
    {
        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            Content = content,
            Timestamp = DateTime.UtcNow,
            IsOutgoing = false
        };
    }
}
