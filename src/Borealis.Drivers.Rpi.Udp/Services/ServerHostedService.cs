using Borealis.Domain.Communication.Messages;
using Borealis.Drivers.Rpi.Udp.Connections;
using Borealis.Drivers.Rpi.Udp.Contexts;



namespace Borealis.Drivers.Rpi.Udp.Services;


public class ServerHostedService : IHostedService, IDisposable, IAsyncDisposable
{
    private readonly ILogger<ServerHostedService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly LedstripContext _ledstripContext;

    private UdpConnectionServer _server;


    public ServerHostedService(ILogger<ServerHostedService> logger, IConfiguration configuration, IHostApplicationLifetime hostApplicationLifetime, LedstripContext ledstripContext)
    {
        _logger = logger;
        _configuration = configuration;
        _hostApplicationLifetime = hostApplicationLifetime;
        _ledstripContext = ledstripContext;
    }


    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Getting the port.
            int port = _configuration.GetSection("Server").GetValue("Port", 8885);

            // Creating the server.
            _logger.LogInformation($"Starting UDP Server on port {port}.");
            _server = new UdpConnectionServer(port);

            // Setting up the server.
            _server.FrameReceived += OnServerFrameReceived;
            _server.Connection += OnServerConnected;
            _server.Disconnection += OnServerDisconnection;

            // starting the server.
            await _server.StartAsync(cancellationToken);
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


    private void OnServerDisconnection(Object? sender, EventArgs e)
    {
        _logger.LogInformation("Disconnection request came in. Clearing all ledstrips.");
        _ledstripContext.ClearAllLedstrips();
    }


    private void OnServerConnected(Object? sender, EventArgs e)
    {
        _logger.LogInformation("Connection request came in clearing al ledstrips.");
        _ledstripContext.ClearAllLedstrips();
    }


    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Stopping the UDP Server.");
            await _server.StopAsync(cancellationToken);
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
    private void OnServerFrameReceived(Object? sender, FrameMessage message)
    {
        _ledstripContext[message.LedstripIndex].SetColors(message.Colors);
    }


    /// <inheritdoc />
    /// <inheritdoc />
    public void Dispose()
    {
        _server?.Dispose();
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_server.IsRunning)
        {
            await _server.SendDisconnectionAsync();
        }
    }
}