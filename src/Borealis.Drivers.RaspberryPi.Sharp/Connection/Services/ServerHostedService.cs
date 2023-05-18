using Borealis.Drivers.RaspberryPi.Sharp.Connection.Core;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Factories;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Services;


public sealed class ServerHostedService : IHostedService, IDisposable
{
    private readonly ILogger<ServerHostedService> _logger;

    private readonly Server _server;


    public ServerHostedService(ILogger<ServerHostedService> logger, ServerFactory serverFactory)
    {
        _logger = logger;

        _server = serverFactory.CreateServer();
    }


    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting server to allow the portal to connect.");
        await _server.StartAsync(cancellationToken).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping the server.");
        await _server.StopAsync(cancellationToken).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public void Dispose()
    {
        _server.Dispose();
    }
}