using System.Net;
using System.Net.Sockets;

using Borealis.Drivers.RaspberryPi.Sharp.Connection.Context;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Managers;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Core;


public class Server : IDisposable
{
    private readonly ILogger<Server> _logger;
    private readonly IConnectionManager _connectionManager;
    private readonly ServerOptions _serverOptions;


    private readonly TcpListener _listener;
    private CancellationTokenSource? _waitingForConnectionCancellationTokenSource;


    public Server(ILogger<Server> logger, IConnectionManager connectionManager, ConnectionContext connectionContext, IOptions<ServerOptions> serverOptions)
    {
        _logger = logger;
        _connectionManager = connectionManager;
        _serverOptions = serverOptions.Value;

        connectionContext.ConnectionDisconnected += ConnectionDisconnected;

        // Starting the TCP Server/
        _listener = new TcpListener(IPAddress.Any, _serverOptions.Port);
    }


    private void ConnectionDisconnected(Object? sender, EventArgs e)
    {
        _logger.LogInformation("Connection disconnected start listening for connections form the portal.");
        StartListeningForConnection();
    }


    /// <summary>
    /// Stars the server and listens for a connection.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> </returns>
    public virtual Task StartAsync(CancellationToken token = default)
    {
        _logger.LogInformation($"Start listening for clients on {_listener.LocalEndpoint}");
        StartListeningForConnection();

        return Task.CompletedTask;
    }


    /// <summary>
    /// Stops the server and the connection that we have connected.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> </returns>
    public virtual Task StopAsync(CancellationToken token = default)
    {
        _waitingForConnectionCancellationTokenSource?.Cancel();
        _listener.Stop();

        return Task.CompletedTask;
    }


    /// <summary>
    /// Starts a task to listen for a new connection.
    /// </summary>
    private void StartListeningForConnection()
    {
        // Validating the we are not listening.
        if (_waitingForConnectionCancellationTokenSource != null) throw new InvalidOperationException("Cannot start listing while we are already listing.");

        // Starting the task.
        _waitingForConnectionCancellationTokenSource = new CancellationTokenSource();

        // Start the task for listening for an connection.
        Task.Factory.StartNew(StartListeningAsync)
            .ContinueWith(HandleListeningCancellation, TaskContinuationOptions.OnlyOnCanceled)
            .ContinueWith(HandleListeningError, TaskContinuationOptions.OnlyOnFaulted);
    }


    protected virtual async Task StartListeningAsync()
    {
        if (_waitingForConnectionCancellationTokenSource == null) throw new InvalidOperationException("The cancellation token for listening is not initialized.");

        // Starting listener.
        _listener.Start();

        // Accepting the socket that we received.
        TcpClient client = await _listener.AcceptTcpClientAsync(_waitingForConnectionCancellationTokenSource.Token).ConfigureAwait(false);

        _logger.LogTrace("Client found.");
        await _connectionManager.SetCurrentConnectionAsync(client).ConfigureAwait(false);

        // Stops listing for connections.
        _waitingForConnectionCancellationTokenSource = null;
        _listener.Stop();
    }


    protected virtual void HandleListeningError(Task task)
    {
        // Log the error.
        _logger.LogError(task.Exception, "There was a problem with accepting connection from the portal.");
    }


    protected virtual void HandleListeningCancellation(Task task)
    {
        // Log the cancellation.
        _logger.LogDebug("Listening for clients has been cancelled.");
    }


    /// <inheritdoc />
    public void Dispose()
    {
        _waitingForConnectionCancellationTokenSource?.Cancel();
    }
}