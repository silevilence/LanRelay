using System.Collections.Concurrent;
using System.Net;
using LanRelay.Core.Network;

namespace LanRelay.Core.State;

/// <summary>
/// State container for managing discovered devices.
/// Implements the State Container Pattern with C# events for UI notification.
/// </summary>
public class DeviceListState
{
    private readonly ConcurrentDictionary<Guid, DeviceInfo> _devices = new();
    private readonly int _heartbeatTimeoutMs;

    /// <summary>
    /// Event raised when a new device is discovered.
    /// </summary>
    public event Action<DeviceInfo>? OnDeviceFound;

    /// <summary>
    /// Event raised when a device is removed (timeout or explicit removal).
    /// </summary>
    public event Action<DeviceInfo>? OnDeviceLost;

    /// <summary>
    /// Event raised when the device list changes (for UI refresh).
    /// </summary>
    public event Action? OnDevicesChanged;

    /// <summary>
    /// Creates a new DeviceListState with the specified heartbeat timeout.
    /// </summary>
    /// <param name="heartbeatTimeoutMs">Timeout in milliseconds before a device is considered lost. Default: 10000ms (10s)</param>
    public DeviceListState(int heartbeatTimeoutMs = 10000)
    {
        _heartbeatTimeoutMs = heartbeatTimeoutMs;
    }

    /// <summary>
    /// Gets a read-only snapshot of all discovered devices.
    /// </summary>
    public IReadOnlyList<DeviceInfo> Devices => _devices.Values.ToList().AsReadOnly();

    /// <summary>
    /// Adds a new device or updates an existing one.
    /// </summary>
    /// <param name="deviceInfo">The device information to add or update.</param>
    public void AddOrUpdateDevice(DeviceInfo deviceInfo)
    {
        ArgumentNullException.ThrowIfNull(deviceInfo);

        var isNew = !_devices.ContainsKey(deviceInfo.DeviceId);

        // Update the device with fresh LastSeen time
        var updatedDevice = deviceInfo with { LastSeen = DateTime.UtcNow };
        _devices[deviceInfo.DeviceId] = updatedDevice;

        if (isNew)
        {
            OnDeviceFound?.Invoke(updatedDevice);
        }

        OnDevicesChanged?.Invoke();
    }

    /// <summary>
    /// Removes a device by its ID.
    /// </summary>
    /// <param name="deviceId">The device ID to remove.</param>
    public void RemoveDevice(Guid deviceId)
    {
        if (_devices.TryRemove(deviceId, out var removedDevice))
        {
            OnDeviceLost?.Invoke(removedDevice);
            OnDevicesChanged?.Invoke();
        }
    }

    /// <summary>
    /// Processes a discovery packet and updates the device list.
    /// Handles both direct devices and relayed devices (via Gossip protocol).
    /// </summary>
    /// <param name="packet">The received discovery packet.</param>
    /// <param name="senderIp">The IP address of the sender.</param>
    public void ProcessDiscoveryPacket(DiscoveryPacket packet, IPAddress senderIp)
    {
        ArgumentNullException.ThrowIfNull(packet);
        ArgumentNullException.ThrowIfNull(senderIp);

        // 1. Process the sender as a direct connection
        var senderInfo = new DeviceInfo
        {
            DeviceId = packet.DeviceId,
            DeviceName = packet.DeviceName,
            IPAddress = senderIp,
            GroupId = packet.GroupId,
            LastSeen = DateTime.UtcNow,
            IsDirectConnection = true,
            RelayDeviceId = null,
            HopCount = 0
        };

        AddOrUpdateDevice(senderInfo);

        // 2. Process known devices advertised by the sender (Gossip protocol)
        foreach (var known in packet.KnownDevices)
        {
            ProcessKnownDevice(known, packet.DeviceId, senderIp);
        }
    }

    /// <summary>
    /// Processes a known device advertised via Gossip protocol.
    /// </summary>
    private void ProcessKnownDevice(KnownDeviceInfo known, Guid relayDeviceId, IPAddress relayIp)
    {
        // Calculate hop count: advertised hops + 1 (for the relay itself)
        var hopCount = known.HopCount + 1;

        // Check if we already know this device
        if (_devices.TryGetValue(known.DeviceId, out var existing))
        {
            // Prefer direct connection or lower hop count
            if (existing.IsDirectConnection || existing.HopCount <= hopCount)
            {
                return; // Keep existing better route
            }
        }

        // Add/update the relayed device
        var relayedDevice = new DeviceInfo
        {
            DeviceId = known.DeviceId,
            DeviceName = known.DeviceName,
            IPAddress = relayIp, // Use relay's IP for communication
            GroupId = known.GroupId ?? "default",
            LastSeen = DateTime.UtcNow,
            IsDirectConnection = false,
            RelayDeviceId = relayDeviceId,
            HopCount = hopCount
        };

        AddOrUpdateDevice(relayedDevice);
    }

    /// <summary>
    /// Removes all devices that have not sent a heartbeat within the timeout period.
    /// This should be called periodically (e.g., via a timer).
    /// </summary>
    public void CleanupExpiredDevices()
    {
        var now = DateTime.UtcNow;
        var expiredIds = _devices
            .Where(kvp => (now - kvp.Value.LastSeen).TotalMilliseconds > _heartbeatTimeoutMs)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var id in expiredIds)
        {
            RemoveDevice(id);
        }
    }

    /// <summary>
    /// Clears all devices from the list.
    /// </summary>
    public void Clear()
    {
        var allIds = _devices.Keys.ToList();
        foreach (var id in allIds)
        {
            RemoveDevice(id);
        }
    }
}
