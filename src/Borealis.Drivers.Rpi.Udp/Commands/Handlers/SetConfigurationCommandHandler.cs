using System;

using Borealis.Shared.Extensions;

using System.Linq;

using Borealis.Drivers.Rpi.Commands.Actions;
using Borealis.Drivers.Rpi.Contexts;
using Borealis.Drivers.Rpi.Services;



namespace Borealis.Drivers.Rpi.Commands.Handlers;


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
        _logger.LogDebug($"Setting new configuration gotten from the portal. {command.DeviceConfiguration.LogToJson()}");

        _logger.LogDebug("Clearing all the active ledstrips.");
        await _displayContext.ClearAllLedstripsAsync().ConfigureAwait(false);

        try
        {
            _logger.LogDebug("Setting the new configuration so we can be sure it works.");
            _ledstripContext.SetConfiguration(command.DeviceConfiguration);

            _logger.LogDebug("Writing the settings to disk.");
            await _settingsService.WriteLedstripSettingsAsync(command.DeviceConfiguration).ConfigureAwait(false);
        }
        catch (AggregateException aggregateException)
        {
            throw new ApplicationException("Unable to set the configuration.", aggregateException);
        }
    }
}