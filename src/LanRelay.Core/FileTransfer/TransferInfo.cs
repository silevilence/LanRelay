namespace LanRelay.Core.FileTransfer;

/// <summary>
/// Information about an active file transfer.
/// </summary>
public class TransferInfo
{
    /// <summary>
    /// Unique identifier for this transfer session.
    /// </summary>
    public required Guid TransferId { get; init; }

    /// <summary>
    /// Name of the file being transferred.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Size of the file in bytes.
    /// </summary>
    public required long FileSize { get; init; }

    /// <summary>
    /// True if this is an outgoing transfer (sending), false if receiving.
    /// </summary>
    public required bool IsOutgoing { get; init; }

    /// <summary>
    /// Current status of the transfer.
    /// </summary>
    public TransferStatus Status { get; set; }

    /// <summary>
    /// Current progress of the transfer (0.0 to 1.0).
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// Device ID of the remote peer.
    /// </summary>
    public Guid? RemoteDeviceId { get; init; }

    /// <summary>
    /// Local file path for saving (incoming) or reading (outgoing).
    /// </summary>
    public string? LocalFilePath { get; set; }

    /// <summary>
    /// Error message if the transfer failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Time when the transfer was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
