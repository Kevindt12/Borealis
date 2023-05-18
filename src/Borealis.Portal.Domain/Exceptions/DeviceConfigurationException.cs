using Borealis.Portal.Domain.Devices.Models;



namespace Borealis.Portal.Domain.Exceptions;


public class DeviceConfigurationException : DeviceException
{
    /// <inheritdoc />
    public DeviceConfigurationException() { }


    /// <inheritdoc />
    public DeviceConfigurationException(String? message) : base(message) { }


    /// <inheritdoc />
    public DeviceConfigurationException(String? message, Exception? innerException) : base(message, innerException) { }


    /// <inheritdoc />
    public DeviceConfigurationException(Device device) : base(device) { }


    /// <inheritdoc />
    public DeviceConfigurationException(String? message, Device device) : base(message, device) { }


    /// <inheritdoc />
    public DeviceConfigurationException(String? message, Exception? innerException, Device device) : base(message, innerException, device) { }
}