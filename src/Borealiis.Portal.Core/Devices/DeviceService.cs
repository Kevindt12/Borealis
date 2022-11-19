using System;
using System.Linq;

using Borealis.Portal.Core.Interaction;
using Borealis.Portal.Domain.Devices;
using Borealis.Portal.Infrastructure.Connections;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Devices;


internal class DeviceService : IDeviceService
{
    private readonly ILogger<DeviceService> _logger;
    private readonly DeviceContext _deviceContext;
    private readonly LedstripContext _ledstripContext;
    private readonly IDeviceConnectionFactory _deviceConnectionFactory;


    public DeviceService(ILogger<DeviceService> logger, DeviceContext deviceContext, LedstripContext ledstripContext, IDeviceConnectionFactory deviceConnectionFactory)
    {
        _logger = logger;
        _deviceContext = deviceContext;
        _ledstripContext = ledstripContext;
        _deviceConnectionFactory = deviceConnectionFactory;
    }


    /// <inheritdoc />
    public async Task ConnectToDeviceAsync(Device device, CancellationToken token = default)
    {
        // Guarding for double connections.
        if (_deviceContext.Connections.Any(c => c.Device == device)) throw new InvalidOperationException($"The device {device.Id} is already connected.");

        _logger.LogTrace($"Connecting to device {device.Id}.");

        // Create the connection.
        IDeviceConnection connection = await _deviceConnectionFactory.CreateConnectionAsync(device, token);

        _logger.LogTrace("Device connected adding device to deviceContext.");
        _deviceContext.AddDeviceConnection(connection);

        _logger.LogTrace("Device connection saved in deviceContext.");
    }


    /// <inheritdoc />
    public async Task DisconnectToDeviceAsync(Device device, CancellationToken token = default)
    {
        // Getting the device.
        IDeviceConnection connection = _deviceContext.Connections.SingleOrDefault(c => c.Device == device) ?? throw new InvalidOperationException($"The device {device.Id} was not connected.");

        _logger.LogTrace($"Stopping connection with device {device.Id}.");

        _logger.LogTrace("Stopping all the animation players that are using this connection.");
        IEnumerable<LedstripInteractorBase> players = _ledstripContext.GetInteractorsFromDevice(device).ToList();

        // Each animationPlayer.
        foreach (LedstripInteractorBase player in players)
        {
            _logger.LogTrace($"Stopping animation player {player.Device.Name}, {player.Ledstrip.Name}.");
            await _ledstripContext.RemoveAndStopInteractorAsync(player);
        }

        _logger.LogTrace("Removing the logger from the deviceContext.");
        await _deviceContext.RemoveDeviceConnectionAsync(connection);
    }


    /// <inheritdoc />
    public Boolean IsDeviceConnected(Device device)
    {
        return _deviceContext.Connections.Any(c => c.Device == device);
    }


    /// <inheritdoc />
    public async Task UploadDeviceConfigurationAsync(Device device, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}