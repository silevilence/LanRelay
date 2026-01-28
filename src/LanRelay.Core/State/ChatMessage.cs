namespace LanRelay.Core.State;

/// <summary>
/// Represents a chat message.
/// </summary>
public record ChatMessage
{
    /// <summary>
    /// Unique identifier for this message.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The device ID this message is associated with (sender or receiver).
    /// </summary>
    public required Guid DeviceId { get; init; }

    /// <summary>
    /// The text content of the message.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// When the message was sent/received.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// True if this message was sent by the local user, false if received.
    /// </summary>
    public required bool IsOutgoing { get; init; }

    /// <summary>
    /// Optional sender name for display purposes.
    /// </summary>
    public string? SenderName { get; init; }
}
