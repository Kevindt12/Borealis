using Borealis.Drivers.RaspberryPi.Sharp.Device.Manager;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Exceptions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;



namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Service;


public class LedstripHostedService : IHostedService
{
    private readonly ILogger<LedstripHostedService> _logger;
    private readonly IDeviceConfigurationManager _deviceConfigurationManager;
    private readonly ILedstripConfigurationService _ledstripConfigurationService;


    public LedstripHostedService(ILogger<LedstripHostedService> logger, IDeviceConfigurationManager deviceConfigurationManager, ILedstripConfigurationService ledstripConfigurationService)
    {
        _logger = logger;
        _deviceConfigurationManager = deviceConfigurationManager;
        _ledstripConfigurationService = ledstripConfigurationService;
    }


    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting the configuration that we want to load.");
        DeviceConfiguration configuration = await GetDeviceConfiguration(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Loading ledstrips from configuration.");
        await _ledstripConfigurationService.LoadConfigurationAsync(configuration).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken) { }


    /// <summary>
    /// Gets the device configuration.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> The <see cref="DeviceConfiguration" /> from the service. </returns>
    private async Task<DeviceConfiguration> GetDeviceConfiguration(CancellationToken token = default)
    {
        try
        {
            DeviceConfiguration configuration = await _deviceConfigurationManager.GetDeviceLedstripConfigurationAsync(token).ConfigureAwait(false);
            _logger.LogTrace($"Device configuration: {configuration.GenerateLogMessage()}");

            return configuration;
        }
        catch (InvalidConfigurationException invalidConfigurationException)
        {
            _logger.LogError(invalidConfigurationException, "There is a problem with the configuration file that is on disk.");

            throw;
        }
        catch (InvalidOperationException invalidOperationException)
        {
            _logger.LogError(invalidOperationException, "The configuration path was not found.");

            throw;
        }
    }
}