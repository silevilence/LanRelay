using System.Security.Cryptography;

namespace LanRelay.Core.FileTransfer;

/// <summary>
/// Service for handling file transfer operations.
/// </summary>
public class FileTransferService
{
    /// <summary>
    /// Default buffer size for stream copying (64KB).
    /// </summary>
    public const int DefaultBufferSize = 64 * 1024;

    /// <summary>
    /// Copies data from source stream to destination stream with progress reporting.
    /// </summary>
    /// <param name="source">Source stream to read from.</param>
    /// <param name="destination">Destination stream to write to.</param>
    /// <param name="totalBytes">Total number of bytes to copy.</param>
    /// <param name="bufferSize">Buffer size for copying.</param>
    /// <param name="progress">Optional progress reporter (0.0 to 1.0).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task CopyStreamAsync(
        Stream source,
        Stream destination,
        long totalBytes,
        int bufferSize = DefaultBufferSize,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var buffer = new byte[bufferSize];
        long totalRead = 0;

        while (totalRead < totalBytes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bytesToRead = (int)Math.Min(bufferSize, totalBytes - totalRead);
            var bytesRead = await source.ReadAsync(buffer.AsMemory(0, bytesToRead), cancellationToken);

            if (bytesRead == 0)
            {
                break; // End of stream
            }

            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            totalRead += bytesRead;

            // Report progress
            if (progress is not null && totalBytes > 0)
            {
                var progressValue = (double)totalRead / totalBytes;
                progress.Report(progressValue);
            }
        }

        await destination.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Calculates the MD5 hash of a stream.
    /// </summary>
    /// <param name="stream">The stream to hash.</param>
    /// <returns>The MD5 hash as a lowercase hex string.</returns>
    public string CalculateMd5(Stream stream)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(stream);
        stream.Position = 0; // Reset stream position
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Checks if there is enough disk space at the specified path.
    /// </summary>
    /// <param name="path">The path to check (can be file or directory).</param>
    /// <param name="requiredBytes">The number of bytes required.</param>
    /// <returns>True if there is enough space, false otherwise.</returns>
    public bool CheckDiskSpace(string path, long requiredBytes)
    {
        try
        {
            var root = Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(root))
            {
                return false;
            }

            var driveInfo = new DriveInfo(root);
            return driveInfo.AvailableFreeSpace >= requiredBytes;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a file transfer request for a file.
    /// </summary>
    /// <param name="filePath">Path to the file to transfer.</param>
    /// <returns>A file transfer request with file metadata.</returns>
    public FileTransferRequest CreateTransferRequest(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        using var stream = fileInfo.OpenRead();
        var md5 = CalculateMd5(stream);

        return new FileTransferRequest
        {
            TransferId = Guid.NewGuid(),
            FileName = fileInfo.Name,
            FileSize = fileInfo.Length,
            Md5Hash = md5
        };
    }

    /// <summary>
    /// Verifies a received file against its expected MD5 hash.
    /// </summary>
    /// <param name="filePath">Path to the received file.</param>
    /// <param name="expectedMd5">The expected MD5 hash.</param>
    /// <returns>True if the hash matches, false otherwise.</returns>
    public bool VerifyFile(string filePath, string expectedMd5)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            var actualMd5 = CalculateMd5(stream);
            return string.Equals(actualMd5, expectedMd5, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
