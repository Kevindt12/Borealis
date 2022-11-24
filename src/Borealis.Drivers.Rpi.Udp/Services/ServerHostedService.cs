using System.Net.Sockets;

using Borealis.Domain.Communication.Messages;
using Borealis.Drivers.Rpi.Udp.Connections;
using Borealis.Drivers.Rpi.Udp.Contexts;
using Borealis.Drivers.Rpi.Udp.Options;

using Microsoft.Extensions.Options;



namespace Borealis.Drivers.Rpi.Udp.Services;


public class ServerHostedService : IHostedService, IDisposable, IAsyncDisposable
{
    private readonly ILogger<ServerHostedService> _logger;
    private readonly UdpServerFactory _udpServerFactory;
    private readonly TcpClientHandlerFactory _tcpClientHandlerFactory;
    private readonly ServerOptions _serverOptions;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly LedstripContext _ledstripContext;

    private UdpServer _udpServer;
    private readonly TcpListener _tcpServer;

    private TcpClientHandler _currentTcpClientHandler;


    public ServerHostedService(ILogger<ServerHostedService> logger,
                               IOptions<ServerOptions> serverOptions,
                               UdpServerFactory udpServerFactory,
                               TcpClientHandlerFactory tcpClientHandlerFactory,
                               IHostApplicationLifetime hostApplicationLifetime,
                               LedstripContext ledstripContext
    )
    {
        _logger = logger;
        _udpServerFactory = udpServerFactory;
        _tcpClientHandlerFactory = tcpClientHandlerFactory;
        _serverOptions = serverOptions.Value;
        _hostApplicationLifetime = hostApplicationLifetime;
        _ledstripContext = ledstripContext;

        _tcpServer = TcpListener.Create(serverOptions.Value.ServerPort);
    }


    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting the UDP server for clients.");
        await StartUdpServerAsync(cancellationToken);

        _logger.LogInformation("Starting the tcp server.");
        await StartTcpServer(cancellationToken);
    }


    protected virtual async Task StartUdpServerAsync(CancellationToken token)
    {
        try
        {
            // Getting the port.
            int port = _serverOptions.ServerPort;

            // Creating the server.
            _logger.LogInformation($"Starting UDP Server on port {port}.");
            _udpServer = _udpServerFactory.CreateUdpServer(port);

            // Setting up the server.
            _udpServer.FrameReceived += HandlerOnFrameReceived;

            // starting the server.
            await _udpServer.StartAsync(token);
            _logger.LogInformation("Serer has started.");
        }
        catch (InvalidOperationException invalidOperationException)
        {
            _logger.LogWarning(invalidOperationException, "The Udp server is already running.");
        }
        catch (OperationCanceledException operationCanceledException)
        {
            _logger.LogError(operationCanceledException, "Starting server cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was a error with starting the UDP client.");

            throw;
        }
    }


    protected virtual async Task StartWaitingForConnection(CancellationToken token)
    {
        // Getting the client that once to connect.
        _logger.LogDebug($"Starting to listen on : {_tcpServer.LocalEndpoint}.");
        TcpClient client = await _tcpServer.AcceptTcpClientAsync(token).ConfigureAwait(false);
        _logger.LogDebug($"Client connection from : {client.Client.RemoteEndPoint}.");

        // Handling the current incoming connection.
        TcpClientHandler handler = _tcpClientHandlerFactory.CreateHandler(client);
        _currentTcpClientHandler = handler;

        handler.FrameReceived += HandlerOnFrameReceived;
        handler.Disconnect += OnServerDisconnection;
        handler.ConfigurationReceived += OnConfigurationReceived;
    }


    protected virtual async Task StartTcpServer(CancellationToken token)
    {
        // Starting the tcp server.
        _tcpServer.Start();

        // Not looping we only want to have a single connection.
        _ = await Task.Factory.StartNew(async () => await StartWaitingForConnection(token), token).ConfigureAwait(false);
    }


    private void OnConfigurationReceived(Object? sender, ConfigurationMessage e)
    {
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


    private async void OnServerDisconnection(Object? sender, EventArgs e)
    {
        // Stopping the current ledstrip context and making sure all ledstrips are clear.
        _logger.LogInformation("Disconnection request came in. Clearing all ledstrips.");
        _ledstripContext.ClearAllLedstrips();

        // Disposing of the handler.
        _logger.LogDebug("Disposing the tcp client handler.");
        await _currentTcpClientHandler.DisposeAsync();
        _currentTcpClientHandler = null;

        // Starting the listener to wait for a connection again.
        await StartWaitingForConnection(CancellationToken.None);
    }


    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Stopping the UDP Server.");
            await _udpServer.StopAsync(cancellationToken);
        }
        catch (InvalidOperationException invalidOperationException)
        {
            // Server is already stopped.
            _logger.LogWarning(invalidOperationException, "Tried stopping server that is already stopped.");
        }
    }


    /// <summary>
    /// When the server has received a package.
    /// </summary>
    /// <param name="sender"> </param>
    /// <param name="e"> </param>
    private void HandlerOnFrameReceived(Object? sender, FrameMessage message)
    {
        _ledstripContext[message.LedstripIndex].SetColors(message.Colors);
    }


    /// <inheritdoc />
    /// <inheritdoc />
    public void Dispose()
    {
        _udpServer?.Dispose();
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_udpServer.IsRunning) { }
    }
}