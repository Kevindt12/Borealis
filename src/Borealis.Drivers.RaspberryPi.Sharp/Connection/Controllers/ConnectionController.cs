using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Manager;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Validation;
using Borealis.Drivers.RaspberryPi.Sharp.Exceptions;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Service;

using Microsoft.Extensions.Logging;

using UnitsNet;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Controllers;


public class ConnectionController
{
    private readonly ILogger<ConnectionController> _logger;
    private readonly IDeviceConfigurationValidator _deviceConfigurationValidator;
    private readonly ILedstripControlService _ledstripControlService;
    private readonly ILedstripConfigurationService _ledstripConfigurationService;
    private readonly IDeviceConfigurationManager _deviceConfigurationManager;


    public ConnectionController(ILogger<ConnectionController> logger,
                                IDeviceConfigurationValidator deviceConfigurationValidator,
                                ILedstripControlService ledstripControlService,
                                ILedstripConfigurationService ledstripConfigurationService,
                                IDeviceConfigurationManager deviceConfigurationManager)
    {
        _logger = logger;
        _deviceConfigurationValidator = deviceConfigurationValidator;
        _ledstripControlService = ledstripControlService;
        _ledstripConfigurationService = ledstripConfigurationService;
        _deviceConfigurationManager = deviceConfigurationManager;
    }


    /// <summary>
    /// Starts the process of connecting to a controller.
    /// </summary>
    /// <param name="connectionConcurrencyToken"> The configuration concurrency token that is send to check if the configuration is the same. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> A flag indicating that we can continue with the connection. </returns>
    /// <returns> A <see cref="bool" /> indicating that we can continue the connection or should suspend the connection. </returns>
    public virtual async Task<ConnectResult> ConnectAsync(string connectionConcurrencyToken, CancellationToken token = default)
    {
        // Getting the token logging and returning.
        _logger.LogInformation("Starting connection process with portal.");
        DeviceConfiguration deviceConfiguration = await _deviceConfigurationManager.GetDeviceLedstripConfigurationAsync(token).ConfigureAwait(false);

        // Checking concurrency token.
        string driverToken = deviceConfiguration.ConcurrencyToken;
        _logger.LogInformation($"Checking configuration token given by server: {connectionConcurrencyToken}, Us: {driverToken}");

        if (driverToken != connectionConcurrencyToken)
        {
            _logger.LogInformation("The device configuration has changed");

            return ConnectResult.DeviceConfigurationChanged();
        }

        return ConnectResult.Success;
    }


    /// <summary>
    /// Sets the configuration that we got from the client.
    /// </summary>
    /// <param name="ledstrips"> The array of <see cref="Ledstrip" /> that we should set in to the configuration. </param>
    /// <param name="connectionConcurrencyToken"> The new concurrency token from the portal. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> A result indicating how the configuration change proceeded. </returns>
    public virtual async Task SetConfigurationAsync(IEnumerable<Ledstrip> ledstrips, string connectionConcurrencyToken, CancellationToken token = default)
    {
        _logger.LogInformation("Request to set configuration has come in. Updating ledstrip configuration.");

        // Creating the configuration.
        DeviceConfiguration configuration = new DeviceConfiguration
        {
            ConcurrencyToken = connectionConcurrencyToken,
            Ledstrips = new List<Ledstrip>(ledstrips)
        };

        // Validating the configuration.
        _logger.LogTrace($"Validating device configuration, configuration : {configuration.GenerateLogMessage()}");
        DeviceConfigurationValidationResult result = await _deviceConfigurationValidator.ValidateAsync(configuration, token).ConfigureAwait(false);

        if (!result.Successful)
        {
            throw new ValidationException("The device configuration was not valid", result.Errors);
        }

        // Checking if the device is ready to accept an new configuration.
        if (!_ledstripConfigurationService.CanLoadConfiguration())
        {
            throw new InvalidOperationException("The ledstrips are still busy with");
        }

        _logger.LogInformation("Updating the device configuration.");
        await ChangeDeviceConfigurationAsync(configuration, token).ConfigureAwait(false);

        _logger.LogInformation("Done loading new configuration.");
    }


    private async Task ChangeDeviceConfigurationAsync(DeviceConfiguration configuration, CancellationToken token = default)
    {
        // Getting the old configuration.
        DeviceConfiguration oldConfiguration = await _deviceConfigurationManager.GetDeviceLedstripConfigurationAsync(token).ConfigureAwait(false);

        try
        {
            // Changing the configuration
            await _deviceConfigurationManager.UpdateDeviceLedstripConfigurationAsync(configuration, token).ConfigureAwait(false);
            await _ledstripConfigurationService.LoadConfigurationAsync(configuration).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // Rolling back the configuration.
            _logger.LogDebug("Unable to update device configuration.");
            try
            {
                await _deviceConfigurationManager.UpdateDeviceLedstripConfigurationAsync(oldConfiguration, token).ConfigureAwait(false);
                await _ledstripConfigurationService.LoadConfigurationAsync(oldConfiguration).ConfigureAwait(false);
                _logger.LogDebug("Device configuration has rolled back.");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to rollback the configuration.");
            }

            throw;
        }
    }


    /// <summary>
    /// Starts animation on a ledstrip.
    /// </summary>
    /// <param name="ledstripId"> The id of the ledstrip. </param>
    /// <param name="frequency"> The <see cref="Frequency" /> of the ledstrip. </param>
    /// <param name="initialFrameBuffer"> The initial frame buffer of the ledstrip. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <exception cref="InvalidOperationException"> If the ledstrip was not found. </exception>
    public virtual async Task StartAnimationAsync(Guid ledstripId, Frequency frequency, IEnumerable<ReadOnlyMemory<PixelColor>> initialFrameBuffer, CancellationToken token = default)
    {
        _logger.LogInformation("Request to start animation on ledstrip came in.");
        Ledstrip ledstrip = _ledstripControlService.GetLedstripById(ledstripId) ?? throw new LedstripNotFoundException("No ledstrip with id found.");

        await _ledstripControlService.StartAnimationAsync(ledstrip, frequency, initialFrameBuffer.ToArray(), token).ConfigureAwait(false);
    }


    /// <summary>
    /// Pauses the animation.
    /// </summary>
    /// <param name="ledstripId"> The ledstrip id of the ledstrip that we want to pause. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <exception cref="InvalidOperationException"> </exception>
    public virtual async Task PauseAnimationAsync(Guid ledstripId, CancellationToken token = default)
    {
        _logger.LogInformation("Request to pause animation has come in.");
        Ledstrip ledstrip = _ledstripControlService.GetLedstripById(ledstripId) ?? throw new LedstripNotFoundException("No ledstrip with id found.");

        await _ledstripControlService.PauseAnimationAsync(ledstrip, token).ConfigureAwait(false);
    }


    /// <summary>
    /// Stops the animation from playing. Also cleans up the animation.
    /// </summary>
    /// <param name="ledstripId"> </param>
    /// <param name="token"> </param>
    /// <returns> </returns>
    /// <exception cref="InvalidOperationException"> </exception>
    public virtual async Task StopAnimationAsync(Guid ledstripId, CancellationToken token = default)
    {
        _logger.LogInformation("Request to stop animation has come in.");
        Ledstrip ledstrip = _ledstripControlService.GetLedstripById(ledstripId) ?? throw new LedstripNotFoundException("No ledstrip with id found.");

        await _ledstripControlService.StopAnimationAsync(ledstrip, token).ConfigureAwait(false);
    }


    /// <summary>
    /// Displays a single frame on the ledstrip.
    /// </summary>
    /// <param name="ledstripId"> The ledstrip id. </param>
    /// <param name="frame"> The frame that we want to display. </param>
    /// <param name="token"> CancellationToken token = default </param>
    /// <exception cref="InvalidOperationException"> </exception>
    public virtual async Task SetLedstripFrameAsync(Guid ledstripId, ReadOnlyMemory<PixelColor> frame, CancellationToken token = default)
    {
        _logger.LogInformation("Request ot display frame on ledstrip has come in.");
        Ledstrip ledstrip = _ledstripControlService.GetLedstripById(ledstripId) ?? throw new InvalidOperationException("No ledstrip with id found.");

        await _ledstripControlService.DisplayFameAsync(ledstrip, frame, token).ConfigureAwait(false);
    }


    public virtual async Task ClearLedstripAsync(Guid ledstripId, CancellationToken token = default)
    {
        _logger.LogInformation("Request ot clear ledstrip has come in.");
        Ledstrip ledstrip = _ledstripControlService.GetLedstripById(ledstripId) ?? throw new LedstripNotFoundException("No ledstrip with id found.");

        await _ledstripControlService.ClearLedstripAsync(ledstrip, token);
    }


    public virtual async Task<IEnumerable<DriverLedstripStatus>> GetDriverStatus(Guid? ledstripId, CancellationToken token = default)
    {
        _logger.LogInformation("Request for device status has come in.");
        List<DriverLedstripStatus> statuses = new List<DriverLedstripStatus>();

        // Only get a single ledstrip if the id is filled.
        if (ledstripId != null)
        {
            Ledstrip ledstrip = _ledstripControlService.GetLedstripById(ledstripId.Value) ?? throw new LedstripNotFoundException("No ledstrip with id found.");

            statuses.Add(new DriverLedstripStatus
            {
                Ledstrip = ledstrip.Id,
                Status = _ledstripControlService.GetLedstripStatus(ledstrip)!.Value
            });

            return statuses;
        }

        // Get all the ledstrips if filled.
        DeviceConfiguration configuration = await _deviceConfigurationManager.GetDeviceLedstripConfigurationAsync(token).ConfigureAwait(false);

        foreach (Ledstrip ledstrip in configuration.Ledstrips)
        {
            statuses.Add(new DriverLedstripStatus
            {
                Ledstrip = ledstrip.Id,
                Status = _ledstripControlService.GetLedstripStatus(ledstrip)!.Value
            });
        }

        return statuses;
    }
}