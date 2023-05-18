using Borealis.Portal.Domain.Devices.Models;



namespace Borealis.Portal.Domain.Devices.Managers;


/// <summary>
/// The manager that manages all the device details in the application.
/// </summary>
public interface IDeviceManager
{
    /// <summary>
    /// Gets all the devices.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> A list of devices. </returns>
    Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken token = default);


    /// <summary>
    /// Gets a device by its id.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <param name="id"> The id of the device we want to get. </param>
    /// <exception cref="InvalidOperationException"> If the device we ware looking for cannot be found. </exception>
    /// <returns> A device by the given id or <c> null </c> if nop device by that is has been found. </returns>
    Task<Device?> GetDeviceById(Guid id, CancellationToken token = default);


    /// <summary>
    /// Saves a device to the application.
    /// </summary>
    /// <param name="device"> The device we want to save. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> </returns>
    Task SaveDeviceAsync(Device device, CancellationToken token = default);


    /// <summary>
    /// Deletes a device out of the system.
    /// </summary>
    /// <param name="device"> The <see cref="Device" /> that we want to delete. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task DeleteDeviceAsync(Device device, CancellationToken token = default);
}