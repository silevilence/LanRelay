using LanRelay.Core.FileTransfer;

namespace LanRelay.Core.Tests.FileTransfer;

public class TransferStateTests
{
    [Fact]
    public void AddTransfer_ShouldAddToActiveTransfers()
    {
        // Arrange
        var state = new TransferState();
        var transferId = Guid.NewGuid();
        var info = new TransferInfo
        {
            TransferId = transferId,
            FileName = "test.txt",
            FileSize = 1024,
            IsOutgoing = true,
            Status = TransferStatus.Pending
        };

        // Act
        state.AddTransfer(info);

        // Assert
        var transfers = state.ActiveTransfers;
        Assert.Single(transfers);
        Assert.Contains(transferId, transfers.Keys);
        Assert.Equal("test.txt", transfers[transferId].FileName);
    }

    [Fact]
    public void AddTransfer_ShouldRaiseOnTransferAdded()
    {
        // Arrange
        var state = new TransferState();
        var transferId = Guid.NewGuid();
        var info = new TransferInfo
        {
            TransferId = transferId,
            FileName = "test.txt",
            FileSize = 1024,
            IsOutgoing = true,
            Status = TransferStatus.Pending
        };

        TransferInfo? receivedInfo = null;
        state.OnTransferAdded += i => receivedInfo = i;

        // Act
        state.AddTransfer(info);

        // Assert
        Assert.NotNull(receivedInfo);
        Assert.Equal(transferId, receivedInfo.TransferId);
    }

    [Fact]
    public void UpdateProgress_ShouldUpdateTransferProgress()
    {
        // Arrange
        var state = new TransferState();
        var transferId = Guid.NewGuid();
        var info = new TransferInfo
        {
            TransferId = transferId,
            FileName = "test.txt",
            FileSize = 1024,
            IsOutgoing = true,
            Status = TransferStatus.Pending
        };
        state.AddTransfer(info);

        // Act
        state.UpdateProgress(transferId, 0.5);

        // Assert
        Assert.Equal(0.5, state.ActiveTransfers[transferId].Progress);
    }

    [Fact]
    public void UpdateProgress_ShouldRaiseOnTransferProgressChanged()
    {
        // Arrange
        var state = new TransferState();
        var transferId = Guid.NewGuid();
        var info = new TransferInfo
        {
            TransferId = transferId,
            FileName = "test.txt",
            FileSize = 1024,
            IsOutgoing = true,
            Status = TransferStatus.Pending
        };
        state.AddTransfer(info);

        Guid? receivedId = null;
        double receivedProgress = 0;
        state.OnTransferProgressChanged += (id, progress) =>
        {
            receivedId = id;
            receivedProgress = progress;
        };

        // Act
        state.UpdateProgress(transferId, 0.75);

        // Assert
        Assert.Equal(transferId, receivedId);
        Assert.Equal(0.75, receivedProgress);
    }

    [Fact]
    public void UpdateStatus_ShouldUpdateTransferStatus()
    {
        // Arrange
        var state = new TransferState();
        var transferId = Guid.NewGuid();
        var info = new TransferInfo
        {
            TransferId = transferId,
            FileName = "test.txt",
            FileSize = 1024,
            IsOutgoing = true,
            Status = TransferStatus.Pending
        };
        state.AddTransfer(info);

        // Act
        state.UpdateStatus(transferId, TransferStatus.InProgress);

        // Assert
        Assert.Equal(TransferStatus.InProgress, state.ActiveTransfers[transferId].Status);
    }

    [Fact]
    public void UpdateStatus_ShouldRaiseOnTransferStatusChanged()
    {
        // Arrange
        var state = new TransferState();
        var transferId = Guid.NewGuid();
        var info = new TransferInfo
        {
            TransferId = transferId,
            FileName = "test.txt",
            FileSize = 1024,
            IsOutgoing = true,
            Status = TransferStatus.Pending
        };
        state.AddTransfer(info);

        Guid? receivedId = null;
        TransferStatus receivedStatus = TransferStatus.Pending;
        state.OnTransferStatusChanged += (id, status) =>
        {
            receivedId = id;
            receivedStatus = status;
        };

        // Act
        state.UpdateStatus(transferId, TransferStatus.Completed);

        // Assert
        Assert.Equal(transferId, receivedId);
        Assert.Equal(TransferStatus.Completed, receivedStatus);
    }

    [Fact]
    public void RemoveTransfer_ShouldRemoveFromActiveTransfers()
    {
        // Arrange
        var state = new TransferState();
        var transferId = Guid.NewGuid();
        var info = new TransferInfo
        {
            TransferId = transferId,
            FileName = "test.txt",
            FileSize = 1024,
            IsOutgoing = true,
            Status = TransferStatus.Pending
        };
        state.AddTransfer(info);

        // Act
        state.RemoveTransfer(transferId);

        // Assert
        Assert.Empty(state.ActiveTransfers);
    }

    [Fact]
    public void RemoveTransfer_ShouldRaiseOnTransferRemoved()
    {
        // Arrange
        var state = new TransferState();
        var transferId = Guid.NewGuid();
        var info = new TransferInfo
        {
            TransferId = transferId,
            FileName = "test.txt",
            FileSize = 1024,
            IsOutgoing = true,
            Status = TransferStatus.Pending
        };
        state.AddTransfer(info);

        Guid? receivedId = null;
        state.OnTransferRemoved += id => receivedId = id;

        // Act
        state.RemoveTransfer(transferId);

        // Assert
        Assert.Equal(transferId, receivedId);
    }

    [Fact]
    public void GetTransfer_ShouldReturnTransferInfo()
    {
        // Arrange
        var state = new TransferState();
        var transferId = Guid.NewGuid();
        var info = new TransferInfo
        {
            TransferId = transferId,
            FileName = "test.txt",
            FileSize = 1024,
            IsOutgoing = true,
            Status = TransferStatus.Pending
        };
        state.AddTransfer(info);

        // Act
        var result = state.GetTransfer(transferId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test.txt", result.FileName);
    }

    [Fact]
    public void GetTransfer_ShouldReturnNullForUnknownId()
    {
        // Arrange
        var state = new TransferState();

        // Act
        var result = state.GetTransfer(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }
}
