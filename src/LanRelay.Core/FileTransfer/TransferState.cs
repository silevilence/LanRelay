using System.Collections.Concurrent;

namespace LanRelay.Core.FileTransfer;

/// <summary>
/// State container for managing active file transfers.
/// Implements the State Container Pattern with C# events for UI notification.
/// </summary>
public class TransferState
{
    private readonly ConcurrentDictionary<Guid, TransferInfo> _transfers = new();
    private readonly object _lock = new();

    /// <summary>
    /// Event raised when a new transfer is added.
    /// </summary>
    public event Action<TransferInfo>? OnTransferAdded;

    /// <summary>
    /// Event raised when a transfer's progress changes.
    /// </summary>
    public event Action<Guid, double>? OnTransferProgressChanged;

    /// <summary>
    /// Event raised when a transfer's status changes.
    /// </summary>
    public event Action<Guid, TransferStatus>? OnTransferStatusChanged;

    /// <summary>
    /// Event raised when a transfer is removed.
    /// </summary>
    public event Action<Guid>? OnTransferRemoved;

    /// <summary>
    /// Gets all active transfers as a read-only dictionary.
    /// </summary>
    public IReadOnlyDictionary<Guid, TransferInfo> ActiveTransfers =>
        _transfers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    /// <summary>
    /// Adds a new transfer to the state.
    /// </summary>
    /// <param name="info">The transfer information.</param>
    public void AddTransfer(TransferInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        _transfers[info.TransferId] = info;
        OnTransferAdded?.Invoke(info);
    }

    /// <summary>
    /// Updates the progress of a transfer.
    /// </summary>
    /// <param name="transferId">The transfer ID.</param>
    /// <param name="progress">The new progress value (0.0 to 1.0).</param>
    public void UpdateProgress(Guid transferId, double progress)
    {
        if (_transfers.TryGetValue(transferId, out var info))
        {
            lock (_lock)
            {
                info.Progress = Math.Clamp(progress, 0.0, 1.0);
            }
            OnTransferProgressChanged?.Invoke(transferId, progress);
        }
    }

    /// <summary>
    /// Updates the status of a transfer.
    /// </summary>
    /// <param name="transferId">The transfer ID.</param>
    /// <param name="status">The new status.</param>
    public void UpdateStatus(Guid transferId, TransferStatus status)
    {
        if (_transfers.TryGetValue(transferId, out var info))
        {
            lock (_lock)
            {
                info.Status = status;
            }
            OnTransferStatusChanged?.Invoke(transferId, status);
        }
    }

    /// <summary>
    /// Removes a transfer from the state.
    /// </summary>
    /// <param name="transferId">The transfer ID to remove.</param>
    public void RemoveTransfer(Guid transferId)
    {
        if (_transfers.TryRemove(transferId, out _))
        {
            OnTransferRemoved?.Invoke(transferId);
        }
    }

    /// <summary>
    /// Gets a transfer by ID.
    /// </summary>
    /// <param name="transferId">The transfer ID.</param>
    /// <returns>The transfer info, or null if not found.</returns>
    public TransferInfo? GetTransfer(Guid transferId)
    {
        return _transfers.TryGetValue(transferId, out var info) ? info : null;
    }

    /// <summary>
    /// Gets all transfers for a specific device.
    /// </summary>
    /// <param name="deviceId">The remote device ID.</param>
    /// <returns>A list of transfers associated with the device.</returns>
    public IReadOnlyList<TransferInfo> GetTransfersForDevice(Guid deviceId)
    {
        return _transfers.Values
            .Where(t => t.RemoteDeviceId == deviceId)
            .OrderByDescending(t => t.CreatedAt)
            .ToList();
    }
}
