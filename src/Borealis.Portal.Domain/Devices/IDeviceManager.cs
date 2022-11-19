namespace Borealis.Portal.Domain.Devices;


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
    /// <exception cref="InvalidOperationException"> If the device we ware looking for cannot be found. </exception>
    /// <param name="id"> The id of the device we want to get. </param>
    /// <returns> </returns>
    Task<Device> GetDeviceById(Guid id, CancellationToken token = default);


    /// <summary>
    /// Saves a device to the application.
    /// </summary>
    /// <param name="device"> The device we want to save. </param>
    /// <returns> </returns>
    Task SaveDeviceAsync(Device device, CancellationToken token = default);
}