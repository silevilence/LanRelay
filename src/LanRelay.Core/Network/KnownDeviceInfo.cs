using System.Net;
using System.Text.Json.Serialization;

namespace LanRelay.Core.Network;

/// <summary>
/// Information about a known device shared via Gossip protocol.
/// Used for relay discovery across different network segments.
/// </summary>
public record KnownDeviceInfo
{
    /// <summary>
    /// Unique identifier of the known device.
    /// </summary>
    [JsonPropertyName("id")]
    public required Guid DeviceId { get; init; }

    /// <summary>
    /// Human-readable name of the device.
    /// </summary>
    [JsonPropertyName("name")]
    public required string DeviceName { get; init; }

    /// <summary>
    /// Original IP address of the device (as string for JSON serialization).
    /// </summary>
    [JsonPropertyName("ip")]
    public required string OriginIP { get; init; }

    /// <summary>
    /// Number of hops from the advertising node to this device.
    /// Direct connection = 0, via one relay = 1, etc.
    /// </summary>
    [JsonPropertyName("hops")]
    public int HopCount { get; init; }

    /// <summary>
    /// Group ID the device belongs to.
    /// </summary>
    [JsonPropertyName("group")]
    public string? GroupId { get; init; }
}
