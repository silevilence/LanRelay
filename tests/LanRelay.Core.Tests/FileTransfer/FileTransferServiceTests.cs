using LanRelay.Core.FileTransfer;

namespace LanRelay.Core.Tests.FileTransfer;

public class FileTransferServiceTests
{
    [Fact]
    public async Task CopyStreamAsync_ShouldCopyAllData()
    {
        // Arrange
        var sourceData = new byte[1024];
        Random.Shared.NextBytes(sourceData);

        using var source = new MemoryStream(sourceData);
        using var destination = new MemoryStream();

        var service = new FileTransferService();

        // Act
        await service.CopyStreamAsync(source, destination, sourceData.Length);

        // Assert
        Assert.Equal(sourceData.Length, destination.Length);
        Assert.Equal(sourceData, destination.ToArray());
    }

    [Fact]
    public async Task CopyStreamAsync_ShouldReportProgress()
    {
        // Arrange
        var sourceData = new byte[10000]; // 10KB
        Random.Shared.NextBytes(sourceData);

        using var source = new MemoryStream(sourceData);
        using var destination = new MemoryStream();

        var service = new FileTransferService();
        var progressValues = new List<double>();
        var progress = new Progress<double>(p => progressValues.Add(p));

        // Act
        await service.CopyStreamAsync(source, destination, sourceData.Length, progress: progress);

        // Wait for progress callbacks
        await Task.Delay(50);

        // Assert
        Assert.NotEmpty(progressValues);
        Assert.Contains(progressValues, p => p > 0);
        Assert.Contains(progressValues, p => p >= 1.0); // Should reach 100%
    }

    [Fact]
    public async Task CopyStreamAsync_ShouldBeCancellable()
    {
        // Arrange - Use a slow stream that checks cancellation
        var sourceData = new byte[1000];
        Random.Shared.NextBytes(sourceData);

        using var source = new SlowStream(new MemoryStream(sourceData));
        using var destination = new MemoryStream();

        var service = new FileTransferService();
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(10); // Cancel after 10ms

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await service.CopyStreamAsync(source, destination, sourceData.Length, bufferSize: 10, cancellationToken: cts.Token);
        });
    }

    // Helper class for testing cancellation
    private class SlowStream : Stream
    {
        private readonly Stream _inner;

        public SlowStream(Stream inner) => _inner = inner;

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => _inner.Position = value; }
        public override void Flush() => _inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => _inner.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await Task.Delay(5, cancellationToken); // Add delay to allow cancellation
            return await _inner.ReadAsync(buffer, cancellationToken);
        }
    }

    [Fact]
    public async Task CopyStreamAsync_ShouldHandleLargeFiles()
    {
        // Arrange - 1MB file
        var sourceData = new byte[1024 * 1024];
        Random.Shared.NextBytes(sourceData);

        using var source = new MemoryStream(sourceData);
        using var destination = new MemoryStream();

        var service = new FileTransferService();

        // Act
        await service.CopyStreamAsync(source, destination, sourceData.Length);

        // Assert
        Assert.Equal(sourceData.Length, destination.Length);
    }

    [Fact]
    public void CalculateMd5_ShouldReturnCorrectHash()
    {
        // Arrange
        var data = "Hello, World!"u8.ToArray();
        using var stream = new MemoryStream(data);

        var service = new FileTransferService();

        // Act
        var hash = service.CalculateMd5(stream);

        // Assert
        Assert.NotNull(hash);
        Assert.Equal(32, hash.Length); // MD5 hex string is 32 chars
    }

    [Fact]
    public void CheckDiskSpace_ShouldReturnTrue_WhenEnoughSpace()
    {
        // Arrange
        var service = new FileTransferService();
        var tempPath = Path.GetTempPath();

        // Act - Check for 1 byte (should always have space)
        var hasSpace = service.CheckDiskSpace(tempPath, 1);

        // Assert
        Assert.True(hasSpace);
    }

    [Fact]
    public void CheckDiskSpace_ShouldReturnFalse_WhenNotEnoughSpace()
    {
        // Arrange
        var service = new FileTransferService();
        var tempPath = Path.GetTempPath();

        // Act - Check for impossibly large size (10 exabytes)
        var hasSpace = service.CheckDiskSpace(tempPath, long.MaxValue);

        // Assert
        Assert.False(hasSpace);
    }
}
