using LanRelay.Core.Network;
using LanRelay.Core.FileTransfer;
using System.Security.Cryptography;
using System.Text;

namespace LanRelay.Core.Tests.FileTransfer;

public class FileTransferProtocolTests
{
    [Fact]
    public void FileTransferRequest_ShouldSerialize()
    {
        // Arrange
        var request = new FileTransferRequest
        {
            TransferId = Guid.NewGuid(),
            FileName = "test.txt",
            FileSize = 1024,
            Md5Hash = "abc123def456"
        };

        // Act
        var json = request.Serialize();

        // Assert
        Assert.NotNull(json);
        Assert.Contains("test.txt", json);
        Assert.Contains("1024", json);
    }

    [Fact]
    public void FileTransferRequest_ShouldDeserialize()
    {
        // Arrange
        var original = new FileTransferRequest
        {
            TransferId = Guid.NewGuid(),
            FileName = "document.pdf",
            FileSize = 2048,
            Md5Hash = "hash123"
        };
        var json = original.Serialize();

        // Act
        var restored = FileTransferRequest.Deserialize(json);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(original.TransferId, restored.TransferId);
        Assert.Equal(original.FileName, restored.FileName);
        Assert.Equal(original.FileSize, restored.FileSize);
        Assert.Equal(original.Md5Hash, restored.Md5Hash);
    }

    [Fact]
    public void FileTransferResponse_ShouldSerialize()
    {
        // Arrange
        var response = new FileTransferResponse
        {
            TransferId = Guid.NewGuid(),
            Accepted = true,
            SavePath = @"C:\Downloads\file.txt"
        };

        // Act
        var json = response.Serialize();

        // Assert
        Assert.NotNull(json);
        Assert.Contains("true", json.ToLower());
    }

    [Fact]
    public void FileTransferResponse_ShouldDeserialize()
    {
        // Arrange
        var original = new FileTransferResponse
        {
            TransferId = Guid.NewGuid(),
            Accepted = false,
            RejectReason = "Insufficient disk space"
        };
        var json = original.Serialize();

        // Act
        var restored = FileTransferResponse.Deserialize(json);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(original.TransferId, restored.TransferId);
        Assert.False(restored.Accepted);
        Assert.Equal("Insufficient disk space", restored.RejectReason);
    }

    [Fact]
    public void Message_ShouldCreateFileRequest()
    {
        // Arrange
        var request = new FileTransferRequest
        {
            TransferId = Guid.NewGuid(),
            FileName = "test.zip",
            FileSize = 10240,
            Md5Hash = "md5hash"
        };

        // Act
        var message = Message.CreateFileTransferRequest(request);

        // Assert
        Assert.Equal(MessageType.FileRequest, message.Header.Type);
        Assert.True(message.Header.BodyLength > 0);
    }
}
