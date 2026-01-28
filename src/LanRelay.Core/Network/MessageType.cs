namespace LanRelay.Core.Network;

/// <summary>
/// Types of messages in the LanRelay protocol.
/// </summary>
public enum MessageType : byte
{
    /// <summary>
    /// Heartbeat/keep-alive message.
    /// </summary>
    Heartbeat = 0,

    /// <summary>
    /// Text chat message.
    /// </summary>
    Text = 1,

    /// <summary>
    /// File transfer request (metadata).
    /// </summary>
    FileRequest = 2,

    /// <summary>
    /// File data chunk.
    /// </summary>
    FileData = 3,

    /// <summary>
    /// File transfer acknowledgment.
    /// </summary>
    FileAck = 4
}
