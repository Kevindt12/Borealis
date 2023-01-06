using System.Net.Sockets;

using Borealis.Domain.Communication.Messages;
using Borealis.Drivers.Rpi.Udp.Connections;
using Borealis.Drivers.Rpi.Udp.Contexts;
using Borealis.Drivers.Rpi.Udp.Ledstrips;
using Borealis.Drivers.Rpi.Udp.Options;

using Microsoft.Extensions.Options;



namespace Borealis.Drivers.Rpi.Udp.Services;


public class ServerHostedService : IHostedService, IDisposable, IAsyncDisposable
{
    private readonly ILogger<ServerHostedService> _logger;
    private readonly ConnectionContext _connectionContext;
    private readonly VisualService _visualService;
    private readonly PortalConnectionFactory _portalConnectionFactory;
    private readonly ServerOptions _serverOptions;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly LedstripContext _ledstripContext;

    private readonly TcpListener _tcpServer;

    private CancellationTokenSource _stoppingToken;


    public ServerHostedService(ILogger<ServerHostedService> logger,
                               IOptions<ServerOptions> serverOptions,
                               ConnectionContext connectionContext,
                               VisualService visualService,
                               PortalConnectionFactory portalConnectionFactory,
                               IHostApplicationLifetime hostApplicationLifetime,
                               LedstripContext ledstripContext
    )
    {
        _logger = logger;
        _connectionContext = connectionContext;
        _visualService = visualService;
        _portalConnectionFactory = portalConnectionFactory;
        _serverOptions = serverOptions.Value;
        _hostApplicationLifetime = hostApplicationLifetime;
        _ledstripContext = ledstripContext;

        _tcpServer = TcpListener.Create(serverOptions.Value.ServerPort);
    }


    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting the tcp server.");
        await StartTcpServer(cancellationToken);
    }


    /// <summary>
    /// Starts the TCP server to listen on the port specified in the appsettigns.js.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    protected virtual async Task StartTcpServer(CancellationToken token)
    {
        // Starting the tcp server.
        _logger.LogInformation("Starting TCP Server on {endpoint}. ", _tcpServer.LocalEndpoint);
        _tcpServer.Start();

        // Not looping we only want to have a single connection.
        _logger.LogInformation("TCP Server started. Listening for a client.");
        _stoppingToken = new CancellationTokenSource();
        _ = Task.Run(async () => await StartWaitingForConnection(_stoppingToken.Token).ConfigureAwait(false), token);
    }


    /// <summary>
    /// The method that handles waiting for the connection and accepting it from the TCP server.
    /// </summary>
    /// <param name="stoppingToken"> A token to cancel the current operation. </param>
    protected virtual async Task StartWaitingForConnection(CancellationToken stoppingToken)
    {
        // Getting the client that once to connect.
        _logger.LogDebug($"On task waiting for the connection : {_tcpServer.LocalEndpoint}.");

        try
        {
            // Loop until we want to cancel.
            TcpClient client = await _tcpServer.AcceptTcpClientAsync(stoppingToken).ConfigureAwait(false);
            _logger.LogInformation($"Client connection from : {client.Client.RemoteEndPoint}.");

            // Setting the new connection and thereby also
            await SetPortalConnectionConnectionAsync(client, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException canceledException)
        {
            _logger.LogError(canceledException, "Cancelling waiting for TCP connection.");
        }
    }


    /// <summary>
    /// setting the portal connection that we got from the TCP Server.
    /// </summary>
    /// <param name="client"> The TCP client that we received. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    private async Task SetPortalConnectionConnectionAsync(TcpClient client, CancellationToken token = default)
    {
        _logger.LogDebug("New connection {client} has connected.", client);

        // Handling the current incoming connection.
        _logger.LogDebug("Creating the handler for the connection.");
        PortalConnection connection = _portalConnectionFactory.CreateHandler(client);

        // Setting the portal connection.
        _connectionContext.Connection = connection;
        _logger.LogDebug("Connection has been established and configured.");

        // Setup the events for the animation handlers
        _logger.LogTrace("Setting the event handlers.");

        // Setting the connection events.
        connection.StartAnimationReceived += HandleStartAnimationReceived;
        connection.StopAnimationReceived += HandleStopAnimationReceived;
        connection.FrameReceived += HandlerFrameReceived;
        connection.FrameBufferReceived += HandleFrameBufferReceived;
        connection.Disconnect += HandleServerDisconnection;
        connection.ConfigurationReceived += HandleConfigurationReceived;
    }


    /// <summary>
    /// Handles the connection request to start playing the animation on the ledstrip.
    /// </summary>
    private async void HandleStartAnimationReceived(Object? sender, StartAnimationMessage e)
    {
        _logger.LogInformation($"Starting animation on ledstrip {e.LedstripIndex} at speed {e.Frequency.Hertz} Hertz.");

        // Getting the ledstrip.
        LedstripProxyBase ledstrip = _ledstripContext[e.LedstripIndex];

        // Clearing the ledstrip.
        await _visualService.StartAnimationAsync(ledstrip, e.Frequency, e.InitialFrameBuffer);
    }


    /// <summary>
    /// Handles the connection requesting to display a single frame.
    /// </summary>
    private async void HandlerFrameReceived(Object? sender, FrameMessage message)
    {
        _logger.LogInformation("displaying a frame for a single ledstrip.");

        // Getting the ledstrip.
        LedstripProxyBase ledstrip = _ledstripContext[message.LedstripIndex];

        // Clearing the ledstrip.
        await _visualService.DisplayFrameAsync(ledstrip, message.Frame);
    }


    /// <summary>
    /// Handles the connection request to stop playing an animation or clear the ledstrip.
    /// </summary>
    private async void HandleStopAnimationReceived(Object? sender, StopAnimationMessage e)
    {
        _logger.LogInformation($"Stopping animation on ledstrip {e.LedstripIndex}");

        // Getting the ledstrip.
        LedstripProxyBase ledstrip = _ledstripContext[e.LedstripIndex];

        // Clearing the ledstrip.
        await _visualService.ClearLedstripAsync(ledstrip);
    }


    /// <summary>
    /// Handles the connection sending us a frame buffer.
    /// </summary>
    private void HandleFrameBufferReceived(Object? sender, FramesBufferMessage e)
    {
        _logger.LogInformation($"Handling frame buffer received for {e.LedstripIndex}, {e.Frames.Length} frames received.");

        // Getting the ledstrip.
        LedstripProxyBase ledstrip = _ledstripContext[e.LedstripIndex];

        // Clearing the ledstrip.
        _visualService.ProcessIncomingFrameBuffer(ledstrip, e.Frames);
    }


    /// <summary>
    /// Handles a new configuration gotten from the protal.
    /// </summary>
    /// <param name="sender"> </param>
    /// <param name="e"> </param>
    private void HandleConfigurationReceived(Object? sender, ConfigurationMessage e)
    {
        throw new NotImplementedException();

        // Set the configuration and restart.
        try
        {
            _ledstripContext.SetConfiguration(e.Settings);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "There was a problem with loading the new configuration.");
        }
    }


    /// <summary>
    /// Handles when the portal requests to disconnect.
    /// </summary>
    private async void HandleServerDisconnection(Object? sender, EventArgs e)
    {
        // Stopping the current ledstrip context and making sure all ledstrips are clear.
        _logger.LogInformation("Disconnection request came in. Clearing all ledstrips.");
        _ledstripContext.ClearAllLedstrips();

        // Disposing of the connection.
        _logger.LogDebug("Disposing the tcp client connection.");
        await _connectionContext.DisconnectPortalAsync();

        // Starts a new connection.
        _logger.LogInformation("Start waiting for a new connection again,");
        await StartWaitingForConnection(CancellationToken.None);
    }


    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Stopping TCP Server");
            _stoppingToken?.Cancel();
        }
        catch (InvalidOperationException invalidOperationException)
        {
            // Server is already stopped.
            _logger.LogWarning(invalidOperationException, "Tried stopping server that is already stopped.");
        }
    }


    /// <inheritdoc />
    /// <inheritdoc />
    public void Dispose()
    {
        _stoppingToken?.Dispose();
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync() { }
}