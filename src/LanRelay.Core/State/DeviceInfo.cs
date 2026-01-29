using System.Net;

namespace LanRelay.Core.State;

/// <summary>
/// Represents information about a discovered device on the network.
/// </summary>
public record DeviceInfo
{
    /// <summary>
    /// Unique identifier for the device.
    /// </summary>
    public required Guid DeviceId { get; init; }

    /// <summary>
    /// Human-readable name of the device.
    /// </summary>
    public required string DeviceName { get; init; }

    /// <summary>
    /// The IP address from which we received the discovery broadcast.
    /// </summary>
    public required IPAddress IPAddress { get; init; }

    /// <summary>
    /// The group this device belongs to.
    /// </summary>
    public required string GroupId { get; init; }

    /// <summary>
    /// Timestamp of the last received heartbeat/discovery packet.
    /// </summary>
    public required DateTime LastSeen { get; init; }

    /// <summary>
    /// Indicates whether this device is directly reachable or requires relay.
    /// </summary>
    public bool IsDirectConnection { get; init; } = true;

    /// <summary>
    /// If relayed, the relay device ID. Null if direct connection.
    /// </summary>
    public Guid? RelayDeviceId { get; init; }

    /// <summary>
    /// Number of network hops to reach this device.
    /// Direct connection = 0, via one relay = 1, etc.
    /// </summary>
    public int HopCount { get; init; } = 0;
}
