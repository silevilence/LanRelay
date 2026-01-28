using System.Net;
using System.Net.Sockets;

namespace LanRelay.Core.Network;

/// <summary>
/// TCP client that connects to a remote server and handles message protocol.
/// </summary>
public class TcpMessageClient : IDisposable
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    /// <summary>
    /// Event raised when a message is received from the server.
    /// </summary>
    public event Action<Message>? OnMessageReceived;

    /// <summary>
    /// Event raised when disconnected from the server.
    /// </summary>
    public event Action? OnDisconnected;

    /// <summary>
    /// Gets whether the client is currently connected.
    /// </summary>
    public bool IsConnected => _client?.Connected ?? false;

    /// <summary>
    /// Gets the remote endpoint if connected.
    /// </summary>
    public IPEndPoint? RemoteEndpoint => _client?.Client.RemoteEndPoint as IPEndPoint;

    /// <summary>
    /// Connects to the specified remote endpoint.
    /// </summary>
    /// <param name="remoteEndpoint">The server endpoint to connect to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ConnectAsync(IPEndPoint remoteEndpoint, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (IsConnected)
        {
            throw new InvalidOperationException("Already connected.");
        }

        _client = new TcpClient();
        await _client.ConnectAsync(remoteEndpoint, cancellationToken);
        _stream = _client.GetStream();

        _cts = new CancellationTokenSource();
        _ = ReceiveLoopAsync(_cts.Token);
    }

    /// <summary>
    /// Sends a message to the connected server.
    /// </summary>
    public async Task SendAsync(Message message, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_stream is null)
        {
            throw new InvalidOperationException("Not connected.");
        }

        var bytes = message.ToBytes();
        await _stream.WriteAsync(bytes, cancellationToken);
        await _stream.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Disconnects from the server.
    /// </summary>
    public void Disconnect()
    {
        _cts?.Cancel();
        _stream?.Dispose();
        _client?.Dispose();
        _stream = null;
        _client = null;
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var headerBuffer = new byte[MessageHeader.HeaderSize];

        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected && _stream is not null)
            {
                // Read header
                var headerRead = await ReadExactAsync(headerBuffer, cancellationToken);
                if (!headerRead)
                {
                    break;
                }

                var header = MessageHeader.FromBytes(headerBuffer);
                if (header is null)
                {
                    continue;
                }

                // Read body
                var bodyBuffer = new byte[header.BodyLength];
                if (header.BodyLength > 0)
                {
                    var bodyRead = await ReadExactAsync(bodyBuffer, cancellationToken);
                    if (!bodyRead)
                    {
                        break;
                    }
                }

                var message = new Message
                {
                    Header = header,
                    Body = bodyBuffer
                };

                OnMessageReceived?.Invoke(message);
            }
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested || _disposed)
        {
            // Expected
        }
        catch (Exception)
        {
            // Connection error
        }
        finally
        {
            OnDisconnected?.Invoke();
        }
    }

    private async Task<bool> ReadExactAsync(byte[] buffer, CancellationToken cancellationToken)
    {
        if (_stream is null) return false;

        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await _stream.ReadAsync(buffer.AsMemory(offset), cancellationToken);
            if (read == 0)
            {
                return false;
            }
            offset += read;
        }
        return true;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Disconnect();
        _cts?.Dispose();
    }
}
