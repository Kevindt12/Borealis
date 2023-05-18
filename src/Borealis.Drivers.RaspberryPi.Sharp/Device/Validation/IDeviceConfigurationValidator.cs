using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;



namespace Borealis.Drivers.RaspberryPi.Sharp.Device.Validation;


/// <summary>
/// Handles the validation of the device configuration.
/// </summary>
public interface IDeviceConfigurationValidator
{
    /// <summary>
    /// Validates the device configuration.
    /// </summary>
    /// <param name="configuration"> The <see cref="DeviceConfiguration" /> that we want to validate. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> A <see cref="DeviceConfigurationValidationResult" /> result indicating the result of the validation. </returns>
    Task<DeviceConfigurationValidationResult> ValidateAsync(DeviceConfiguration configuration, CancellationToken token = default);
}