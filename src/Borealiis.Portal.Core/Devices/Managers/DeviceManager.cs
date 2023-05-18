using System;
using System.Linq;

using Borealis.Portal.Core.Devices.Contexts;
using Borealis.Portal.Data.Stores;
using Borealis.Portal.Domain.Connectivity.Connections;
using Borealis.Portal.Domain.Devices.Managers;
using Borealis.Portal.Domain.Devices.Models;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Devices.Managers;


internal class DeviceManager : IDeviceManager
{
    private readonly ILogger<DeviceManager> _logger;
    private readonly DeviceConnectionContext _deviceConnectionContext;
    private readonly IDeviceStore _deviceStore;


    public DeviceManager(ILogger<DeviceManager> logger, DeviceConnectionContext deviceConnectionContext, IDeviceStore deviceStore)
    {
        _logger = logger;
        _deviceConnectionContext = deviceConnectionContext;
        _deviceStore = deviceStore;
    }


    /// <inheritdoc />
    public virtual async Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken token = default)
    {
        _logger.LogTrace("Getting the devices form the store.");

        return await _deviceStore.GetDevicesAsync(token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task<Device?> GetDeviceById(Guid id, CancellationToken token = default)
    {
        _logger.LogTrace($"Getting device with Id {id}.");

        return await _deviceStore.FindByIdAsync(id, token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task SaveDeviceAsync(Device device, CancellationToken token = default)
    {
        _logger.LogDebug($"Saving device {device.Name},");

        // Checking if we need to add the device or update the device.
        if (device.Id == Guid.Empty)
        {
            await _deviceStore.AddAsync(device, token).ConfigureAwait(false);
        }
        else
        {
            await _deviceStore.UpdateAsync(device, token).ConfigureAwait(false);
        }
    }


    /// <inheritdoc />
    public virtual async Task DeleteDeviceAsync(Device device, CancellationToken token = default)
    {
        _logger.LogDebug($"Deleting device {device}");

        if (_deviceConnectionContext.IsDeviceConnected(device))
        {
            _logger.LogDebug("Device is connected and disposing of device connection.");
            IDeviceConnection connection = _deviceConnectionContext.GetDeviceConnection(device)!;
            await _deviceConnectionContext.DisposeOfConnectionAsync(connection, token).ConfigureAwait(false);

            _logger.LogDebug("Device connection is disposed removing device from system.");
        }

        // Removes the device from the system.
        await _deviceStore.RemoveAsync(device, token).ConfigureAwait(false);
    }
}