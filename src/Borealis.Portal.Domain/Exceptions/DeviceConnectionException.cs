using Borealis.Portal.Domain.Devices.Models;



namespace Borealis.Portal.Domain.Exceptions;


/// <summary>
/// A exception that has happens because of the connection with the device.
/// </summary>
public class DeviceConnectionException : DeviceException
{
    /// <inheritdoc />
    public DeviceConnectionException() { }


    /// <inheritdoc />
    public DeviceConnectionException(String? message) : base(message) { }


    /// <inheritdoc />
    public DeviceConnectionException(String? message, Exception? innerException) : base(message, innerException) { }


    /// <inheritdoc />
    public DeviceConnectionException(Device device) : base(device) { }


    /// <inheritdoc />
    public DeviceConnectionException(String? message, Device device) : base(message, device) { }


    /// <inheritdoc />
    public DeviceConnectionException(String? message, Exception? innerException, Device device) : base(message, innerException, device) { }
}