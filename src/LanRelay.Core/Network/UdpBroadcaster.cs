using System.Net;
using System.Net.Sockets;

namespace LanRelay.Core.Network;

/// <summary>
/// Result of a UDP receive operation.
/// </summary>
public record UdpReceiveResult
{
    /// <summary>
    /// The received data bytes.
    /// </summary>
    public required byte[] Data { get; init; }

    /// <summary>
    /// The remote endpoint that sent the data.
    /// </summary>
    public required IPEndPoint RemoteEndpoint { get; init; }
}

/// <summary>
/// UDP broadcaster that supports binding to a specific local IP address.
/// This is essential for dual-NIC relay scenarios where we need to control
/// which network interface is used for communication.
/// </summary>
public class UdpBroadcaster : IDisposable
{
    private readonly UdpClient _client;
    private bool _disposed;

    /// <summary>
    /// Gets the local endpoint this broadcaster is bound to.
    /// </summary>
    public IPEndPoint LocalEndpoint { get; }

    /// <summary>
    /// Creates a new UdpBroadcaster bound to the specified local endpoint.
    /// </summary>
    /// <param name="localEndpoint">The local IP and port to bind to.</param>
    public UdpBroadcaster(IPEndPoint localEndpoint)
    {
        LocalEndpoint = localEndpoint ?? throw new ArgumentNullException(nameof(localEndpoint));

        // Create socket with explicit binding to the specified local endpoint
        _client = new UdpClient();

        // Enable broadcast
        _client.EnableBroadcast = true;

        // Allow address reuse for multiple listeners
        _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        // Bind to the specific local endpoint
        _client.Client.Bind(localEndpoint);
    }

    /// <summary>
    /// Sends data to the specified remote endpoint.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="remoteEndpoint">The target endpoint.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SendAsync(byte[] data, IPEndPoint remoteEndpoint, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(remoteEndpoint);

        await _client.SendAsync(data, remoteEndpoint, cancellationToken);
    }

    /// <summary>
    /// Receives data from any remote endpoint.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The received data and sender information.</returns>
    public async Task<UdpReceiveResult> ReceiveAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var result = await _client.ReceiveAsync(cancellationToken);

        return new UdpReceiveResult
        {
            Data = result.Buffer,
            RemoteEndpoint = result.RemoteEndPoint
        };
    }

    /// <summary>
    /// Disposes the UDP client and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}
