namespace LanRelay.Core.FileTransfer;

/// <summary>
/// Status of a file transfer operation.
/// </summary>
public enum TransferStatus
{
    /// <summary>
    /// Transfer is pending user acceptance.
    /// </summary>
    Pending,

    /// <summary>
    /// Transfer is in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// Transfer completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Transfer was cancelled by user.
    /// </summary>
    Cancelled,

    /// <summary>
    /// Transfer failed due to an error.
    /// </summary>
    Failed,

    /// <summary>
    /// Transfer was rejected by receiver.
    /// </summary>
    Rejected
}
