namespace LanRelay.Core.Network;

/// <summary>
/// Service for relaying data between two streams with zero-copy optimization.
/// Implements backpressure handling for efficient memory usage.
/// </summary>
public class RelayService
{
    /// <summary>
    /// Default buffer size (8KB).
    /// </summary>
    public const int DefaultBufferSize = 8 * 1024;

    private readonly int _bufferSize;

    /// <summary>
    /// Creates a new relay service with the specified buffer size.
    /// </summary>
    /// <param name="bufferSize">Size of the relay buffer in bytes.</param>
    public RelayService(int bufferSize = DefaultBufferSize)
    {
        if (bufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer size must be positive.");
        }
        _bufferSize = bufferSize;
    }

    /// <summary>
    /// Relays data from source stream to target stream.
    /// Uses a fixed-size buffer for memory efficiency (zero-copy style).
    /// Implements natural backpressure: if target is slow, reading from source pauses.
    /// </summary>
    /// <param name="source">Source stream to read from.</param>
    /// <param name="target">Target stream to write to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="progress">Optional progress reporter (total bytes transferred).</param>
    public async Task RelayAsync(
        Stream source,
        Stream target,
        CancellationToken cancellationToken,
        IProgress<long>? progress = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        var buffer = new byte[_bufferSize];
        long totalBytes = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Read from source (will block if source is slow)
            var bytesRead = await source.ReadAsync(buffer, cancellationToken);

            if (bytesRead == 0)
            {
                break; // End of stream
            }

            // Write to target (will block if target is slow = natural backpressure)
            await target.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

            totalBytes += bytesRead;
            progress?.Report(totalBytes);
        }

        await target.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Relays data bidirectionally between two streams.
    /// Useful for full-duplex communication relay.
    /// </summary>
    /// <param name="streamA">First stream.</param>
    /// <param name="streamB">Second stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task RelayBidirectionalAsync(
        Stream streamA,
        Stream streamB,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(streamA);
        ArgumentNullException.ThrowIfNull(streamB);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var aToBTask = RelayWithCleanupAsync(streamA, streamB, linkedCts);
        var bToATask = RelayWithCleanupAsync(streamB, streamA, linkedCts);

        // Wait for either direction to complete (usually means connection closed)
        await Task.WhenAny(aToBTask, bToATask);

        // Cancel the other direction
        await linkedCts.CancelAsync();

        // Wait for both to complete (handle exceptions)
        try
        {
            await Task.WhenAll(aToBTask, bToATask);
        }
        catch (OperationCanceledException)
        {
            // Expected when one direction completes
        }
    }

    private async Task RelayWithCleanupAsync(
        Stream source,
        Stream target,
        CancellationTokenSource linkedCts)
    {
        try
        {
            await RelayAsync(source, target, linkedCts.Token);
        }
        catch (Exception) when (linkedCts.IsCancellationRequested)
        {
            // Ignore exceptions caused by cancellation
        }
    }
}
