using Borealis.Portal.Domain.Devices.Models;



namespace Borealis.Portal.Domain.Devices.Services;


/// <summary>
/// The device service that handles everything to do with the device interactions.
/// </summary>
public interface IDeviceService
{
    /// <summary>
    /// Connect to device.
    /// </summary>
    /// <param name="device"> The device we want to connect to. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task ConnectAsync(Device device, CancellationToken token = default);


    /// <summary>
    /// Connect to device.
    /// </summary>
    /// <param name="device"> The device we want to connect to. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task DisconnectAsync(Device device, CancellationToken token = default);


    /// <summary>
    /// Checks if the device is connected.
    /// </summary>
    /// <param name="device"> The device we want to know is connected. </param>
    /// <returns> <see cref="true" /> if device is connected. </returns>
    bool IsConnected(Device device);


    /// <summary>
    /// Upload the configuration.
    /// </summary>
    /// <param name="device"> The device we want to update. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task UploadDeviceConfigurationAsync(Device device, CancellationToken token = default);
}