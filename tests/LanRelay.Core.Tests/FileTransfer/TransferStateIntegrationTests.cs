using LanRelay.Core.FileTransfer;

namespace LanRelay.Core.Tests.FileTransfer;

public class TransferStateIntegrationTests
{
    [Fact]
    public void TransferState_ShouldSupportPendingIncomingTransfer()
    {
        // Arrange - simulating a file receive request
        var state = new TransferState();
        var transferId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        
        var info = new TransferInfo
        {
            TransferId = transferId,
            FileName = "document.pdf",
            FileSize = 1024 * 1024 * 5, // 5 MB
            IsOutgoing = false,
            Status = TransferStatus.Pending,
            RemoteDeviceId = deviceId
        };

        // Act
        state.AddTransfer(info);

        // Assert
        var transfer = state.GetTransfer(transferId);
        Assert.NotNull(transfer);
        Assert.Equal("document.pdf", transfer.FileName);
        Assert.Equal(TransferStatus.Pending, transfer.Status);
        Assert.False(transfer.IsOutgoing);
    }

    [Fact]
    public void AcceptTransfer_ShouldChangeStatusToInProgress()
    {
        // Arrange
        var state = new TransferState();
        var transferId = Guid.NewGuid();
        var info = new TransferInfo
        {
            TransferId = transferId,
            FileName = "photo.jpg",
            FileSize = 2048,
            IsOutgoing = false,
            Status = TransferStatus.Pending,
            RemoteDeviceId = Guid.NewGuid()
        };
        state.AddTransfer(info);

        TransferStatus? receivedStatus = null;
        state.OnTransferStatusChanged += (id, status) => receivedStatus = status;

        // Act - User accepts the transfer
        state.UpdateStatus(transferId, TransferStatus.InProgress);

        // Assert
        Assert.Equal(TransferStatus.InProgress, state.GetTransfer(transferId)?.Status);
        Assert.Equal(TransferStatus.InProgress, receivedStatus);
    }

    [Fact]
    public void RejectTransfer_ShouldChangeStatusToRejected()
    {
        // Arrange
        var state = new TransferState();
        var transferId = Guid.NewGuid();
        var info = new TransferInfo
        {
            TransferId = transferId,
            FileName = "virus.exe",
            FileSize = 4096,
            IsOutgoing = false,
            Status = TransferStatus.Pending,
            RemoteDeviceId = Guid.NewGuid()
        };
        state.AddTransfer(info);

        // Act - User rejects the transfer
        state.UpdateStatus(transferId, TransferStatus.Rejected);

        // Assert
        Assert.Equal(TransferStatus.Rejected, state.GetTransfer(transferId)?.Status);
    }

    [Fact]
    public void ProgressUpdates_ShouldTrackMultipleTransfers()
    {
        // Arrange
        var state = new TransferState();
        var transfer1Id = Guid.NewGuid();
        var transfer2Id = Guid.NewGuid();

        state.AddTransfer(new TransferInfo
        {
            TransferId = transfer1Id,
            FileName = "file1.zip",
            FileSize = 1000,
            IsOutgoing = true,
            Status = TransferStatus.InProgress
        });

        state.AddTransfer(new TransferInfo
        {
            TransferId = transfer2Id,
            FileName = "file2.zip",
            FileSize = 2000,
            IsOutgoing = false,
            Status = TransferStatus.InProgress
        });

        // Act
        state.UpdateProgress(transfer1Id, 0.25);
        state.UpdateProgress(transfer2Id, 0.75);

        // Assert
        Assert.Equal(0.25, state.GetTransfer(transfer1Id)?.Progress);
        Assert.Equal(0.75, state.GetTransfer(transfer2Id)?.Progress);
    }

    [Fact]
    public void GetTransfersForDevice_ShouldFilterByDeviceId()
    {
        // Arrange
        var state = new TransferState();
        var deviceA = Guid.NewGuid();
        var deviceB = Guid.NewGuid();

        state.AddTransfer(new TransferInfo
        {
            TransferId = Guid.NewGuid(),
            FileName = "fileA1.txt",
            FileSize = 100,
            IsOutgoing = true,
            Status = TransferStatus.Completed,
            RemoteDeviceId = deviceA
        });

        state.AddTransfer(new TransferInfo
        {
            TransferId = Guid.NewGuid(),
            FileName = "fileA2.txt",
            FileSize = 200,
            IsOutgoing = false,
            Status = TransferStatus.InProgress,
            RemoteDeviceId = deviceA
        });

        state.AddTransfer(new TransferInfo
        {
            TransferId = Guid.NewGuid(),
            FileName = "fileB1.txt",
            FileSize = 300,
            IsOutgoing = true,
            Status = TransferStatus.Pending,
            RemoteDeviceId = deviceB
        });

        // Act
        var deviceATransfers = state.GetTransfersForDevice(deviceA);
        var deviceBTransfers = state.GetTransfersForDevice(deviceB);

        // Assert
        Assert.Equal(2, deviceATransfers.Count);
        Assert.Single(deviceBTransfers);
        Assert.All(deviceATransfers, t => Assert.Equal(deviceA, t.RemoteDeviceId));
    }

    [Fact]
    public void CompletedTransfer_ShouldHaveFullProgress()
    {
        // Arrange
        var state = new TransferState();
        var transferId = Guid.NewGuid();
        state.AddTransfer(new TransferInfo
        {
            TransferId = transferId,
            FileName = "complete.zip",
            FileSize = 5000,
            IsOutgoing = true,
            Status = TransferStatus.InProgress
        });

        // Act - Simulate completion
        state.UpdateProgress(transferId, 1.0);
        state.UpdateStatus(transferId, TransferStatus.Completed);

        // Assert
        var transfer = state.GetTransfer(transferId);
        Assert.Equal(1.0, transfer?.Progress);
        Assert.Equal(TransferStatus.Completed, transfer?.Status);
    }

    [Fact]
    public void CancelTransfer_ShouldSetCancelledStatus()
    {
        // Arrange
        var state = new TransferState();
        var transferId = Guid.NewGuid();
        state.AddTransfer(new TransferInfo
        {
            TransferId = transferId,
            FileName = "large-file.iso",
            FileSize = 1024 * 1024 * 100,
            IsOutgoing = true,
            Status = TransferStatus.InProgress
        });
        state.UpdateProgress(transferId, 0.3);

        // Act - User cancels
        state.UpdateStatus(transferId, TransferStatus.Cancelled);

        // Assert
        var transfer = state.GetTransfer(transferId);
        Assert.Equal(TransferStatus.Cancelled, transfer?.Status);
        Assert.Equal(0.3, transfer?.Progress); // Progress remains where it was
    }
}
