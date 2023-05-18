using Borealis.Portal.Domain.Devices.Models;



namespace Borealis.Portal.Data.Stores;


/// <summary>
/// The store that holds the devices known to the application.
/// </summary>
public interface IDeviceStore
{
    /// <summary>
    /// Finds the device by its id.
    /// </summary>
    /// <param name="id"> The id of the device. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> A <see cref="Device" /> or <c> null </c> if a device cannot be found. </returns>
    Task<Device?> FindByIdAsync(Guid id, CancellationToken token = default);


    /// <summary>
    /// Gets all the devices known by the application.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> A <see cref="Device" /> list. </returns>
    Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken token = default);


    /// <summary>
    /// Adds a new device to the system.
    /// </summary>
    /// <param name="device"> The device that we ware adding. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task AddAsync(Device device, CancellationToken token = default);


    /// <summary>
    /// Updates a device that is in the system.
    /// </summary>
    /// <param name="device"> The device that is in the system. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task UpdateAsync(Device device, CancellationToken token = default);


    /// <summary>
    /// Removes a device that is in the system.
    /// </summary>
    /// <param name="device"> The device that we want to remove. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> </returns>
    Task RemoveAsync(Device device, CancellationToken token = default);
}