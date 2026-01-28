using System.Net;
using System.Net.Sockets;

namespace LanRelay.Core.Network;

/// <summary>
/// Represents a connected TCP client on the server side.
/// Handles reading and writing messages from/to a single client.
/// </summary>
public class TcpClientConnection : IDisposable
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;

    /// <summary>
    /// Event raised when a message is received from this client.
    /// </summary>
    public event Action<Message>? OnMessageReceived;

    /// <summary>
    /// Event raised when this client disconnects.
    /// </summary>
    public event Action? OnDisconnected;

    /// <summary>
    /// Gets the remote endpoint of the connected client.
    /// </summary>
    public IPEndPoint RemoteEndpoint { get; }

    /// <summary>
    /// Gets whether the connection is still active.
    /// </summary>
    public bool IsConnected => _client.Connected && !_disposed;

    internal TcpClientConnection(TcpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _stream = client.GetStream();
        RemoteEndpoint = (IPEndPoint)client.Client.RemoteEndPoint!;
    }

    /// <summary>
    /// Starts the receive loop for this connection.
    /// </summary>
    internal void StartReceiving()
    {
        _ = ReceiveLoopAsync(_cts.Token);
    }

    /// <summary>
    /// Sends a message to the connected client.
    /// </summary>
    public async Task SendAsync(Message message, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var bytes = message.ToBytes();
        await _stream.WriteAsync(bytes, cancellationToken);
        await _stream.FlushAsync(cancellationToken);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var headerBuffer = new byte[MessageHeader.HeaderSize];

        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                // Read header
                var headerRead = await ReadExactAsync(headerBuffer, cancellationToken);
                if (!headerRead)
                {
                    break; // Connection closed
                }

                var header = MessageHeader.FromBytes(headerBuffer);
                if (header is null)
                {
                    continue; // Invalid header, skip
                }

                // Read body
                var bodyBuffer = new byte[header.BodyLength];
                if (header.BodyLength > 0)
                {
                    var bodyRead = await ReadExactAsync(bodyBuffer, cancellationToken);
                    if (!bodyRead)
                    {
                        break; // Connection closed
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
            // Expected during shutdown
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
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await _stream.ReadAsync(buffer.AsMemory(offset), cancellationToken);
            if (read == 0)
            {
                return false; // Connection closed
            }
            offset += read;
        }
        return true;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cts.Cancel();
        _cts.Dispose();
        _stream.Dispose();
        _client.Dispose();
    }
}
