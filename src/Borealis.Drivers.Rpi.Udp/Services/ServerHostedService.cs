using Borealis.Domain.Devices;
using Borealis.Drivers.Rpi.Udp.Contexts;
using Borealis.Portal.Domain.Exceptions;
using Borealis.Shared.Extensions;



namespace Borealis.Drivers.Rpi.Udp.Services;


public class ServerHostedService : IHostedService
{
    private readonly ILogger<ServerHostedService> _logger;
    private readonly SettingsService _settingsService;
    private readonly DisplayContext _displayContext;
    private readonly ConnectionContext _connectionContext;
    private readonly LedstripContext _ledstripContext;

    private readonly IHostApplicationLifetime _hostApplicationLifetime;


    public ServerHostedService(ILogger<ServerHostedService> logger,
                               SettingsService settingsService,
                               DisplayContext displayContext,
                               ConnectionContext connectionContext,
                               LedstripContext ledstripContext,
                               IHostApplicationLifetime hostApplicationLifetime
    )
    {
        _logger = logger;
        _settingsService = settingsService;
        _displayContext = displayContext;
        _connectionContext = connectionContext;
        _ledstripContext = ledstripContext;
        _hostApplicationLifetime = hostApplicationLifetime;
    }


    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await StartLedstripContextAsync(cancellationToken).ConfigureAwait(false);

        await StartConnectionContextAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Systems have started!");
    }


    /// <summary>
    /// Starting the ledstrip context and setting its configuration.
    /// </summary>
    /// <param name="cancellationToken"> A token to cancel the current operation. </param>
    private async Task StartLedstripContextAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting up the ledstrips. Getting configuration from settings file.");
            LedstripSettings settings = await _settingsService.ReadLedstripSettingsAsync(cancellationToken).ConfigureAwait(false);

            // Starting the ledstrip context.
            _logger.LogDebug($" Settings : {settings.LogToJson()}");
            _logger.LogInformation("Starting the ledstrip system.");
            _ledstripContext.SetConfiguration(settings);
        }
        catch (AggregateException aggregateException)
        {
            // Invalid ledstrips. Ignore and continue.
            _logger.LogWarning(aggregateException, "Unable to set configuration of the ledstrips.");
        }
        catch (IOException e)
        {
            // When there is no configuration then this is a fatal error. Because we cant do anything without it.
            _logger.LogError(e, "Unable to load the configuration.");
            _hostApplicationLifetime.StopApplication();
        }
        catch (InvalidDeviceConfigurationException invalidDeviceConfigurationException)
        {
            // Invalid Json ignore and continue start on application..
            _logger.LogWarning(invalidDeviceConfigurationException, "Invalid Json.");
        }
    }


    /// <summary>
    /// Starts the connection allowing a portal to connect with the driver.
    /// </summary>
    /// <param name="cancellationToken"> A token to cancel the current operation. </param>
    private async Task StartConnectionContextAsync(CancellationToken cancellationToken)
    {
        // Loading the connection.
        _logger.LogInformation("Starting up the connection services.");
        await _connectionContext.StartListeningAsync(cancellationToken).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await StopDisplayItemsAsync(cancellationToken).ConfigureAwait(false);

        await StopConnectionAsync(cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Stops the display process like the animations and clearing the ledstrips.
    /// </summary>
    /// <param name="cancellationToken"> A token to cancel the current operation. </param>
    private async Task StopDisplayItemsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down, stopping display items.");
        await _displayContext.ClearAllLedstripsAsync(cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Stops the connection and stopping the portal to connect.
    /// </summary>
    /// <param name="cancellationToken"> </param>
    /// <returns> </returns>
    private async Task StopConnectionAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping the connection system.");
        await _connectionContext.DisposeAsync().ConfigureAwait(false);
    }
}