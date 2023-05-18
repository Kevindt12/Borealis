using Borealis.Portal.Domain.Devices.Models;



namespace Borealis.Portal.Domain.Exceptions;


/// <summary>
/// A exception because there is a issue with the device configuration.
/// </summary>
public class InvalidDeviceConfigurationException : DeviceException
{
    /// <inheritdoc />
    public InvalidDeviceConfigurationException(String? message) : base(message) { }


    /// <inheritdoc />
    public InvalidDeviceConfigurationException(String? message, Exception? innerException) : base(message, innerException) { }


    /// <inheritdoc />
    public InvalidDeviceConfigurationException(Device device) : base(device) { }


    /// <inheritdoc />
    public InvalidDeviceConfigurationException(String? message, Device device) : base(message, device) { }


    /// <inheritdoc />
    public InvalidDeviceConfigurationException(String? message, Exception? innerException, Device device) : base(message, innerException, device) { }
}