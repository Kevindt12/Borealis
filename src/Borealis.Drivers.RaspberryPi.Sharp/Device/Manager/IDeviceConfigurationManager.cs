using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Exceptions;



namespace Borealis.Drivers.RaspberryPi.Sharp.Device.Manager;


/// <summary>
/// Manages the device configuration.
/// </summary>
public interface IDeviceConfigurationManager
{
    /// <summary>
    /// Writes the new configuration to disk
    /// </summary>
    /// <param name="deviceConfiguration"> The <see cref="DeviceConfiguration " /> that we want to write to disk. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <exception cref="InvalidConfigurationException"> Thrown when the configuration path of the device was not set. </exception>
    Task UpdateDeviceLedstripConfigurationAsync(DeviceConfiguration deviceConfiguration, CancellationToken token = default);


    /// <summary>
    /// Gets the device configuration from disk.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> The device configuration from disk. </returns>
    /// <exception cref="InvalidOperationException"> Thrown when the configuration path of the device was not set. </exception>
    /// <exception cref="InvalidConfigurationException"> When the configuration file is not valid. </exception>
    Task<DeviceConfiguration> GetDeviceLedstripConfigurationAsync(CancellationToken token = default);
}