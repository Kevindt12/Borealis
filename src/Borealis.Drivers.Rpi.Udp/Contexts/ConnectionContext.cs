using System.Net.Sockets;

using Borealis.Drivers.Rpi.Udp.Connections;
using Borealis.Drivers.Rpi.Udp.Options;

using Microsoft.Extensions.Options;



namespace Borealis.Drivers.Rpi.Udp.Contexts;


public class ConnectionContext : IAsyncDisposable, IDisposable
{
    private readonly ILogger<ConnectionContext> _logger;
    private readonly ServerOptions _serverOptions;

    private readonly PortalConnectionFactory _portalConnectionFactory;

    private readonly TcpListener _tcpServer;
    private Task? _listeningTask;
    private readonly CancellationTokenSource _listeningStoppingToken;


    /// <summary>
    /// Event that is triggered when a connection status has been changed.
    /// </summary>
    public event EventHandler<PortalConnectionStatusChangedEventArgs>? ConnectionStatusChanged;


    private PortalConnection? _connection;

    public PortalConnection? Connection
    {
        get => _connection;
        private set
        {
            _connection = value;
            ConnectionStatusChanged?.Invoke(this, _connection != null ? PortalConnectionStatusChangedEventArgs.Connected : PortalConnectionStatusChangedEventArgs.Disconnected);
        }
    }


    public ConnectionContext(ILogger<ConnectionContext> logger, IOptions<ServerOptions> serverOptions, PortalConnectionFactory portalConnectionFactory)
    {
        _logger = logger;
        _portalConnectionFactory = portalConnectionFactory;
        _serverOptions = serverOptions.Value;

        // Starting the tcp server.
        _tcpServer = TcpListener.Create(serverOptions.Value.ServerPort);
        _tcpServer.Start();
        _logger.LogInformation("Starting TCP Server on {endpoint}. ", _tcpServer!.LocalEndpoint);

        // Application stopping token.
        _listeningStoppingToken = new CancellationTokenSource();
    }


    public async Task StartListeningAsync(CancellationToken token = default)
    {
        if (_listeningTask != null) throw new InvalidOperationException("The start listening for portal task has already started.");

        // Not looping because we only want to have a single connection.
        _listeningTask = Task.Factory.StartNew(async () => await WaitForConnectionHandler().ConfigureAwait(false), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }


    /// <summary>
    /// The method that handles waiting for the connection and accepting it from the TCP server.
    /// </summary>
    protected virtual async Task WaitForConnectionHandler()
    {
        // Getting the client that once to connect.
        _logger.LogDebug($"Task started to wait for the connection : {_tcpServer.LocalEndpoint}.");

        try
        {
            // Loop until we want to cancel.
            TcpClient client = await _tcpServer.AcceptTcpClientAsync(_listeningStoppingToken.Token).ConfigureAwait(false);
            _logger.LogInformation($"Client connection from : {client.Client.RemoteEndPoint}.");

            // Handling the current incoming connection.
            _logger.LogDebug("Creating the handler for the connection.");
            PortalConnection connection = _portalConnectionFactory.CreateHandler(client);

            // Setting up the disconnection of the portal.
            connection.Disconnecting += HandleServerDisconnection;

            _logger.LogDebug("Setting the connection that we just created.");
            Connection = connection;
        }
        catch (OperationCanceledException canceledException)
        {
            _logger.LogError(canceledException, "Cancelling waiting for TCP connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was a error while waiting for a connection to be established. Since the connection is a core part this is a fatal error.");

            throw;
        }
    }


    /// <summary>
    /// Handles when the portal requests to disconnect.
    /// </summary>
    private async void HandleServerDisconnection(Object? sender, EventArgs e)
    {
        _logger.LogInformation("Handling disconnection of the portal.");
        Connection = null;

        // Not looping because we only want to have a single connection.
        _listeningTask = Task.Factory.StartNew(async () => await WaitForConnectionHandler().ConfigureAwait(false), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }


    #region IDisposable

    private bool _disposed;


    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        // Stopping the listening threads if there running.
        _listeningStoppingToken.Cancel();
        _listeningStoppingToken.Dispose();

        _disposed = true;
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        // Stopping the listening threads if there running.
        _listeningStoppingToken.Cancel();
        _listeningStoppingToken.Dispose();

        // Stopping the task if its running and checking if it had failed. Log the errors that we get.
        if (_listeningTask != null)
        {
            try
            {
                await _listeningTask.ConfigureAwait(false);
            }
            catch (AggregateException e)
            {
                _logger.LogWarning(e, "Exception was thrown when cleaning up the task that is responsible for listening for the portal.");
            }
        }

        _disposed = true;
    }

    #endregion
}



public class PortalConnectionStatusChangedEventArgs : EventArgs
{
    public PortalConnectionStatus PortalConnectionStatus { get; init; }


    public static PortalConnectionStatusChangedEventArgs Connected =>
        new PortalConnectionStatusChangedEventArgs
            { PortalConnectionStatus = PortalConnectionStatus.Connected };

    public static PortalConnectionStatusChangedEventArgs Disconnected =>
        new PortalConnectionStatusChangedEventArgs
            { PortalConnectionStatus = PortalConnectionStatus.Disconnected };
}



public enum PortalConnectionStatus
{
    Connected,
    Disconnected
}