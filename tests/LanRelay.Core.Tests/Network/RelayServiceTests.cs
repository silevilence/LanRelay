using System.Net;
using System.Net.Sockets;
using LanRelay.Core.Network;

namespace LanRelay.Core.Tests.Network;

/// <summary>
/// Tests for relay data forwarding service.
/// Simulates A -> B (relay) -> C topology.
/// </summary>
public class RelayServiceTests : IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    [Fact]
    public async Task RelayService_ShouldForwardDataBetweenStreams()
    {
        // Arrange
        var sourceData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        using var sourceStream = new MemoryStream(sourceData);
        using var targetStream = new MemoryStream();

        var relayService = new RelayService();

        // Act
        await relayService.RelayAsync(sourceStream, targetStream, _cts.Token);

        // Assert
        Assert.Equal(sourceData.Length, targetStream.Length);
        Assert.Equal(sourceData, targetStream.ToArray());
    }

    [Fact]
    public async Task RelayService_ShouldReportProgress()
    {
        // Arrange
        var sourceData = new byte[1024 * 10]; // 10KB
        Random.Shared.NextBytes(sourceData);
        using var sourceStream = new MemoryStream(sourceData);
        using var targetStream = new MemoryStream();

        var relayService = new RelayService();
        long lastProgress = 0;
        var progressReporter = new SynchronousProgress<long>(bytes => lastProgress = bytes);

        // Act
        await relayService.RelayAsync(
            sourceStream,
            targetStream,
            _cts.Token,
            progressReporter);

        // Assert
        Assert.Equal(sourceData.Length, lastProgress);
    }

    /// <summary>
    /// Synchronous progress reporter for testing.
    /// </summary>
    private class SynchronousProgress<T> : IProgress<T>
    {
        private readonly Action<T> _handler;
        public SynchronousProgress(Action<T> handler) => _handler = handler;
        public void Report(T value) => _handler(value);
    }

    [Fact]
    public async Task RelayService_ShouldRespectCancellation()
    {
        // Arrange
        using var sourceStream = new NeverEndingStream();
        using var targetStream = new MemoryStream();

        var relayService = new RelayService();
        var localCts = new CancellationTokenSource();

        // Act - Cancel after 50ms
        var relayTask = relayService.RelayAsync(sourceStream, targetStream, localCts.Token);
        await Task.Delay(50);
        localCts.Cancel();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => relayTask);
    }

    [Fact]
    public async Task RelayService_ShouldHandleLargeDataWithoutMemorySpike()
    {
        // Arrange - 1MB of data
        var dataSize = 1024 * 1024;
        using var sourceStream = new LimitedRandomStream(dataSize);
        using var targetStream = new CountingStream();

        var relayService = new RelayService(bufferSize: 4096); // 4KB buffer

        // Act
        var beforeMemory = GC.GetTotalMemory(true);
        await relayService.RelayAsync(sourceStream, targetStream, _cts.Token);
        var afterMemory = GC.GetTotalMemory(true);

        // Assert
        Assert.Equal(dataSize, targetStream.BytesWritten);
        // Memory increase should be reasonable (buffer + overhead), not the full data size
        var memoryIncrease = afterMemory - beforeMemory;
        Assert.True(memoryIncrease < dataSize / 2, $"Memory increased by {memoryIncrease} bytes, expected less than {dataSize / 2}");
    }

    [Fact]
    public async Task RelayService_ShouldHandleBackpressure()
    {
        // Arrange
        var dataSize = 1024 * 100; // 100KB
        using var sourceStream = new LimitedRandomStream(dataSize);
        using var targetStream = new SlowWriteStream(delayMs: 1); // Simulates slow target

        var relayService = new RelayService(bufferSize: 1024);

        // Act - Should complete without issues despite slow target
        await relayService.RelayAsync(sourceStream, targetStream, _cts.Token);

        // Assert
        Assert.Equal(dataSize, targetStream.BytesWritten);
    }

    [Fact]
    public void RelayService_ShouldHaveConfigurableBufferSize()
    {
        // Arrange & Act
        var defaultService = new RelayService();
        var customService = new RelayService(bufferSize: 16384);

        // Assert - Just verify construction works
        Assert.NotNull(defaultService);
        Assert.NotNull(customService);
    }

    /// <summary>
    /// Stream that never ends (for cancellation testing).
    /// </summary>
    private class NeverEndingStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Thread.Sleep(10);
            Array.Fill(buffer, (byte)0, offset, count);
            return count;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await Task.Delay(10, cancellationToken);
            buffer.Span.Fill(0);
            return buffer.Length;
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    /// <summary>
    /// Stream that provides limited random data.
    /// </summary>
    private class LimitedRandomStream : Stream
    {
        private readonly long _totalBytes;
        private long _bytesRead;

        public LimitedRandomStream(long totalBytes) => _totalBytes = totalBytes;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _totalBytes;
        public override long Position { get => _bytesRead; set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var remaining = _totalBytes - _bytesRead;
            if (remaining <= 0) return 0;

            var toRead = (int)Math.Min(count, remaining);
            Random.Shared.NextBytes(buffer.AsSpan(offset, toRead));
            _bytesRead += toRead;
            return toRead;
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    /// <summary>
    /// Stream that counts bytes written.
    /// </summary>
    private class CountingStream : Stream
    {
        public long BytesWritten { get; private set; }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => BytesWritten;
        public override long Position { get => BytesWritten; set => throw new NotSupportedException(); }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BytesWritten += count;
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            BytesWritten += buffer.Length;
            return ValueTask.CompletedTask;
        }

        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }

    /// <summary>
    /// Stream that writes slowly (simulates backpressure).
    /// </summary>
    private class SlowWriteStream : Stream
    {
        private readonly int _delayMs;
        public long BytesWritten { get; private set; }

        public SlowWriteStream(int delayMs) => _delayMs = delayMs;

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => BytesWritten;
        public override long Position { get => BytesWritten; set => throw new NotSupportedException(); }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Thread.Sleep(_delayMs);
            BytesWritten += count;
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await Task.Delay(_delayMs, cancellationToken);
            BytesWritten += buffer.Length;
        }

        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
