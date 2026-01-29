using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LanRelay.Core.Network;

/// <summary>
/// Represents a discovery broadcast packet for LAN device discovery.
/// </summary>
public record DiscoveryPacket
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Protocol version for forward compatibility.
    /// </summary>
    [JsonPropertyName("v")]
    public int ProtocolVersion { get; init; } = 1;

    /// <summary>
    /// Unique identifier for this device.
    /// </summary>
    [JsonPropertyName("id")]
    public required Guid DeviceId { get; init; }

    /// <summary>
    /// Human-readable name of the device.
    /// </summary>
    [JsonPropertyName("name")]
    public required string DeviceName { get; init; }

    /// <summary>
    /// Group identifier for device grouping.
    /// </summary>
    [JsonPropertyName("group")]
    public required string GroupId { get; init; }

    /// <summary>
    /// List of known devices to share via Gossip protocol for relay discovery.
    /// Bridge nodes use this to advertise devices from other network segments.
    /// </summary>
    [JsonPropertyName("known")]
    public IReadOnlyList<KnownDeviceInfo> KnownDevices { get; init; } = [];

    /// <summary>
    /// Serializes the packet to a JSON string.
    /// </summary>
    public string Serialize()
    {
        return JsonSerializer.Serialize(this, JsonOptions);
    }

    /// <summary>
    /// Serializes the packet to UTF-8 bytes.
    /// </summary>
    public byte[] ToBytes()
    {
        return Encoding.UTF8.GetBytes(Serialize());
    }

    /// <summary>
    /// Deserializes a JSON string to a DiscoveryPacket.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized packet, or null if deserialization fails.</returns>
    public static DiscoveryPacket? Deserialize(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<DiscoveryPacket>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Deserializes UTF-8 bytes to a DiscoveryPacket.
    /// </summary>
    /// <param name="bytes">The bytes to deserialize.</param>
    /// <returns>The deserialized packet, or null if deserialization fails.</returns>
    public static DiscoveryPacket? FromBytes(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
        {
            return null;
        }

        try
        {
            var json = Encoding.UTF8.GetString(bytes);
            return Deserialize(json);
        }
        catch
        {
            return null;
        }
    }
}
