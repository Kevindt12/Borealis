using Borealis.Domain.Devices;
using Borealis.Drivers.Rpi.Udp.Commands.Actions;
using Borealis.Drivers.Rpi.Udp.Services;



namespace Borealis.Drivers.Rpi.Udp.Commands.Handlers;


public class ConnectQueryHandler : IQueryHandler<ConnectCommand, ConnectedQuery>
{
    private readonly ILogger<ConnectQueryHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly SettingsService _settingsService;


    public ConnectQueryHandler(ILogger<ConnectQueryHandler> logger, IConfiguration configuration, SettingsService settingsService)
    {
        _logger = logger;
        _configuration = configuration;
        _settingsService = settingsService;
    }


    /// <inheritdoc />
    public async Task<ConnectedQuery> Execute(ConnectCommand command)
    {
        _logger.LogInformation($"Handling connection request from client {command.RemoteConnection}.");
        LedstripSettings settings = await _settingsService.ReadLedstripSettingsAsync().ConfigureAwait(false);

        ConnectedQuery resultQuery = new ConnectedQuery
        {
            // Checking if the configuration is still the same.
            IsConfigurationValid = settings.Token == command.ConfigurationConcurrencyToken
        };

        _logger.LogDebug($"Checking if the configuration is still valid. Result {resultQuery.IsConfigurationValid}");

        return resultQuery;
    }
}