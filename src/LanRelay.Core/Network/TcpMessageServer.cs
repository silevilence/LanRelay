using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace LanRelay.Core.Network;

/// <summary>
/// TCP server that listens for incoming connections and handles message protocol.
/// Supports binding to a specific local IP address for dual-NIC scenarios.
/// </summary>
public class TcpMessageServer : IDisposable
{
    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<IPEndPoint, TcpClientConnection> _connections = new();
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;
    private bool _started;

    /// <summary>
    /// Event raised when a new client connects.
    /// </summary>
    public event Action<TcpClientConnection>? OnClientConnected;

    /// <summary>
    /// Event raised when a client disconnects.
    /// </summary>
    public event Action<TcpClientConnection>? OnClientDisconnected;

    /// <summary>
    /// Event raised when a message is received from any client.
    /// </summary>
    public event Action<TcpClientConnection, Message>? OnMessageReceived;

    /// <summary>
    /// Gets the local endpoint this server is bound to.
    /// </summary>
    public IPEndPoint LocalEndpoint { get; }

    /// <summary>
    /// Gets all currently connected clients.
    /// </summary>
    public IReadOnlyCollection<TcpClientConnection> Connections => _connections.Values.ToList().AsReadOnly();

    /// <summary>
    /// Creates a new TCP message server bound to the specified endpoint.
    /// </summary>
    /// <param name="localEndpoint">The local IP and port to listen on.</param>
    public TcpMessageServer(IPEndPoint localEndpoint)
    {
        LocalEndpoint = localEndpoint ?? throw new ArgumentNullException(nameof(localEndpoint));
        _listener = new TcpListener(localEndpoint);
    }

    /// <summary>
    /// Starts listening for incoming connections.
    /// </summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_started)
        {
            throw new InvalidOperationException("Server is already started.");
        }

        _listener.Start();
        _started = true;

        _ = AcceptLoopAsync(_cts.Token);
    }

    /// <summary>
    /// Stops the server and disconnects all clients.
    /// </summary>
    public void Stop()
    {
        if (!_started) return;

        _cts.Cancel();
        _listener.Stop();

        foreach (var connection in _connections.Values)
        {
            connection.Dispose();
        }
        _connections.Clear();

        _started = false;
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                var connection = new TcpClientConnection(client);

                _connections[connection.RemoteEndpoint] = connection;

                connection.OnMessageReceived += msg => OnMessageReceived?.Invoke(connection, msg);
                connection.OnDisconnected += () => HandleClientDisconnected(connection);

                connection.StartReceiving();
                OnClientConnected?.Invoke(connection);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }
        catch (Exception)
        {
            // Listener stopped
        }
    }

    private void HandleClientDisconnected(TcpClientConnection connection)
    {
        _connections.TryRemove(connection.RemoteEndpoint, out _);
        OnClientDisconnected?.Invoke(connection);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Stop();
        _cts.Dispose();
    }
}
