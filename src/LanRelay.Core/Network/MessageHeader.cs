using System.Buffers.Binary;

namespace LanRelay.Core.Network;

/// <summary>
/// Fixed-size header for all messages in the LanRelay protocol.
/// Format: [Type: 1 byte] [Reserved: 3 bytes] [BodyLength: 4 bytes] = 8 bytes total
/// </summary>
public record MessageHeader
{
    /// <summary>
    /// Size of the header in bytes.
    /// </summary>
    public const int HeaderSize = 8;

    /// <summary>
    /// The type of message.
    /// </summary>
    public required MessageType Type { get; init; }

    /// <summary>
    /// Length of the message body in bytes.
    /// </summary>
    public required int BodyLength { get; init; }

    /// <summary>
    /// Serializes the header to a byte array.
    /// </summary>
    public byte[] ToBytes()
    {
        var buffer = new byte[HeaderSize];
        buffer[0] = (byte)Type;
        // bytes 1-3 are reserved (set to 0)
        BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(4), BodyLength);
        return buffer;
    }

    /// <summary>
    /// Deserializes a header from a byte array.
    /// </summary>
    /// <param name="data">The byte array containing the header.</param>
    /// <returns>The deserialized header, or null if data is invalid.</returns>
    public static MessageHeader? FromBytes(ReadOnlySpan<byte> data)
    {
        if (data.Length < HeaderSize)
        {
            return null;
        }

        var type = (MessageType)data[0];
        var bodyLength = BinaryPrimitives.ReadInt32BigEndian(data.Slice(4));

        if (bodyLength < 0)
        {
            return null;
        }

        return new MessageHeader
        {
            Type = type,
            BodyLength = bodyLength
        };
    }
}
