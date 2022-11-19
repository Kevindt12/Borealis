using System;
using System.Linq;

using Borealis.Portal.Data.Contexts;
using Borealis.Portal.Domain.Devices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Devices;


public class DeviceManager : IDeviceManager
{
    private readonly ILogger<DeviceManager> _logger;
    private readonly ApplicationDbContext _context;


    public DeviceManager(ILogger<DeviceManager> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }


    /// <inheritdoc />
    public async Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken token = default)
    {
        _logger.LogTrace("Getting the devices form the store.");

        return await _context.Devices.ToListAsync(token);
    }


    /// <inheritdoc />
    public async Task<Device> GetDeviceById(Guid id, CancellationToken token = default)
    {
        _logger.LogTrace($"Getting device with Id {id}.");

        return await _context.Devices.SingleAsync(d => d.Id == id, token);
    }


    /// <inheritdoc />
    public async Task SaveDeviceAsync(Device device, CancellationToken token = default)
    {
        _logger.LogDebug($"Saving device {device.Name},");

        // Checking if we need to add the device or update the device.
        if (device.Id == Guid.Empty)
        {
            _context.Devices.Add(device);
        }
        else
        {
            _context.Devices.Update(device);
        }

        // Saving changes the the database.
        await _context.SaveChangesAsync(token);
    }
}