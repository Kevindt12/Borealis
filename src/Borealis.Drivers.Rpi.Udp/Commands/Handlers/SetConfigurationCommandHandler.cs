using Borealis.Drivers.Rpi.Udp.Commands.Actions;
using Borealis.Drivers.Rpi.Udp.Contexts;
using Borealis.Drivers.Rpi.Udp.Services;
using Borealis.Shared.Extensions;



namespace Borealis.Drivers.Rpi.Udp.Commands.Handlers;


public class SetConfigurationCommandHandler : ICommandHandler<ConfigurationCommand>
{
    private readonly ILogger<SetConfigurationCommandHandler> _logger;
    private readonly SettingsService _settingsService;
    private readonly LedstripContext _ledstripContext;
    private readonly DisplayContext _displayContext;


    public SetConfigurationCommandHandler(ILogger<SetConfigurationCommandHandler> logger, SettingsService settingsService, LedstripContext ledstripContext, DisplayContext displayContext)
    {
        _logger = logger;
        _settingsService = settingsService;
        _ledstripContext = ledstripContext;
        _displayContext = displayContext;
    }


    /// <inheritdoc />
    public async Task ExecuteAsync(ConfigurationCommand command)
    {
        _logger.LogDebug($"Setting new configuration gotten from the portal. {command.LedstripSettings.LogToJson()}");

        _logger.LogDebug("Clearing all the active ledstrips.");
        await _displayContext.ClearAllLedstripsAsync().ConfigureAwait(false);

        try
        {
            _logger.LogDebug("Setting the new configuration so we can be sure it works.");
            _ledstripContext.SetConfiguration(command.LedstripSettings);

            _logger.LogDebug("Writing the settings to disk.");
            await _settingsService.WriteLedstripSettingsAsync(command.LedstripSettings).ConfigureAwait(false);
        }
        catch (AggregateException aggregateException)
        {
            throw new ApplicationException("Unable to set the configuration.", aggregateException);
        }
    }
}