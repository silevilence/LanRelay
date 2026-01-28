using System.Text;

namespace LanRelay.Core.Network;

/// <summary>
/// Represents a complete message with header and body.
/// </summary>
public record Message
{
    /// <summary>
    /// The message header containing type and length.
    /// </summary>
    public required MessageHeader Header { get; init; }

    /// <summary>
    /// The message body (may be empty for heartbeats).
    /// </summary>
    public required byte[] Body { get; init; }

    /// <summary>
    /// Creates a text message.
    /// </summary>
    /// <param name="text">The text content.</param>
    /// <returns>A new text message.</returns>
    public static Message CreateText(string text)
    {
        var body = Encoding.UTF8.GetBytes(text);
        return new Message
        {
            Header = new MessageHeader
            {
                Type = MessageType.Text,
                BodyLength = body.Length
            },
            Body = body
        };
    }

    /// <summary>
    /// Creates a heartbeat message.
    /// </summary>
    /// <returns>A new heartbeat message.</returns>
    public static Message CreateHeartbeat()
    {
        return new Message
        {
            Header = new MessageHeader
            {
                Type = MessageType.Heartbeat,
                BodyLength = 0
            },
            Body = []
        };
    }

    /// <summary>
    /// Creates a file request message with metadata.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="fileSize">The file size in bytes.</param>
    /// <returns>A new file request message.</returns>
    public static Message CreateFileRequest(string fileName, long fileSize)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new { fileName, fileSize });
        var body = Encoding.UTF8.GetBytes(json);
        return new Message
        {
            Header = new MessageHeader
            {
                Type = MessageType.FileRequest,
                BodyLength = body.Length
            },
            Body = body
        };
    }

    /// <summary>
    /// Serializes the message to a byte array.
    /// </summary>
    /// <returns>The serialized message.</returns>
    public byte[] ToBytes()
    {
        var headerBytes = Header.ToBytes();
        var result = new byte[headerBytes.Length + Body.Length];
        headerBytes.CopyTo(result, 0);
        Body.CopyTo(result, headerBytes.Length);
        return result;
    }

    /// <summary>
    /// Deserializes a message from a byte array.
    /// </summary>
    /// <param name="data">The byte array containing the message.</param>
    /// <returns>The deserialized message, or null if data is invalid.</returns>
    public static Message? FromBytes(ReadOnlySpan<byte> data)
    {
        var header = MessageHeader.FromBytes(data);
        if (header is null)
        {
            return null;
        }

        if (data.Length < MessageHeader.HeaderSize + header.BodyLength)
        {
            return null;
        }

        var body = data.Slice(MessageHeader.HeaderSize, header.BodyLength).ToArray();

        return new Message
        {
            Header = header,
            Body = body
        };
    }
}
