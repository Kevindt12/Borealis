using System;
using System.Linq;

using Borealis.Portal.Core.Devices.Contexts;
using Borealis.Portal.Core.Ledstrips.Contexts;
using Borealis.Portal.Domain.Connectivity.Connections;
using Borealis.Portal.Domain.Connectivity.Factories;
using Borealis.Portal.Domain.Connectivity.Models;
using Borealis.Portal.Domain.Devices.Models;
using Borealis.Portal.Domain.Devices.Services;
using Borealis.Portal.Domain.Exceptions;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Devices.Services;


internal class DeviceService : IDeviceService
{
    private readonly ILogger<DeviceService> _logger;
    private readonly DeviceConnectionContext _deviceConnectionContext;
    private readonly LedstripDisplayContext _ledstripDisplayContext;
    private readonly IDeviceConnectionFactory _deviceConnectionFactory;


    public DeviceService(ILogger<DeviceService> logger, DeviceConnectionContext deviceConnectionContext, LedstripDisplayContext ledstripDisplayContext, IDeviceConnectionFactory deviceConnectionFactory)
    {
        _logger = logger;
        _deviceConnectionContext = deviceConnectionContext;
        _ledstripDisplayContext = ledstripDisplayContext;
        _deviceConnectionFactory = deviceConnectionFactory;
    }


    /// <inheritdoc />
    /// <exception cref="InvalidDeviceConfigurationException"> Thrown when the device configuration is not valid or null. </exception>
    /// <exception cref="OperationCanceledException"> When the operation was cancelled by the token. </exception>
    /// <exception cref="InvalidOperationException"> Thrown when the device is already connected. </exception>
    /// <returns> A <see cref="DeviceException" /> Thrown when there is a problem at the device. </returns>
    /// <returns> A <see cref="DeviceCommunicationException" /> Thrown when there is a problem with the communication between the portal and the device. </returns>
    /// <exception cref="DeviceConnectionException"> Thrown when there is a problem with the connection between the driver and the portal. </exception>
    public async Task ConnectAsync(Device device, CancellationToken token = default)
    {
        // Guarding for double connections.
        if (_deviceConnectionContext.IsDeviceConnected(device)) throw new InvalidOperationException($"The device {device.Id} is already connected.");

        // Start the connection process.
        _logger.LogTrace($"Starting device connection to {device.Id}.");
        IDeviceConnection connection = _deviceConnectionFactory.CreateConnection(device);

        // Try and start the connection.
        try
        {
            DeviceConnectionResult result = await connection.ConnectAsync(token);
        }
        catch (Exception e)
        {
            await connection.DisposeAsync();

            throw;
        }

        // Start tracking the connection.
        _logger.LogDebug("Device connected adding device to deviceContext.");
        _deviceConnectionContext.TrackConnection(connection);

        // Start tracking the state of the ledstrips
        _logger.LogTrace("Start tracking the display states of the ledstrip.");
        _ledstripDisplayContext.TrackDeviceConnection(connection);

        _logger.LogTrace("Device connection saved in deviceContext.");
    }


    /// <inheritdoc />
    /// <exception cref="InvalidOperationException"> Thrown when the device is already connected. </exception>
    /// <exception cref="OperationCanceledException"> When the operation was cancelled by the token. </exception>
    public virtual async Task DisconnectAsync(Device device, CancellationToken token = default)
    {
        // Getting the device.
        _logger.LogTrace($"Stopping connection with device {device.Id}.");
        IDeviceConnection connection = _deviceConnectionContext.GetDeviceConnection(device) ?? throw new InvalidOperationException($"The device {device.Id} was not connected.");

        _logger.LogTrace("Removing the device connection,");
        await _deviceConnectionContext.DisposeOfConnectionAsync(connection, token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public bool IsConnected(Device device)
    {
        return _deviceConnectionContext.IsDeviceConnected(device);
    }


    /// <inheritdoc />
    public async Task UploadDeviceConfigurationAsync(Device device, CancellationToken token = default)
    {
        _logger.LogTrace("Start uploading new configuration to device.");
        IDeviceConnection? connection = _deviceConnectionContext.GetDeviceConnection(device) ?? throw new InvalidOperationException("Action could not be performed because the device is disconnected.");

        await connection.UploadConfigurationAsync(token).ConfigureAwait(false);
        _logger.LogDebug("Device configuration has been updated.");
    }
}