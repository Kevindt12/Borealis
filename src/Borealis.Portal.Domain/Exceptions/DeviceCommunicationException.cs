using Borealis.Portal.Domain.Devices.Models;



namespace Borealis.Portal.Domain.Exceptions;


/// <summary>
/// A exception that is thrown when the device and the portal have had communication issues.
/// </summary>
public class DeviceCommunicationException : DeviceException
{
    /// <inheritdoc />
    public DeviceCommunicationException() { }


    /// <inheritdoc />
    public DeviceCommunicationException(String? message) : base(message) { }


    /// <inheritdoc />
    public DeviceCommunicationException(String? message, Exception? innerException) : base(message, innerException) { }


    /// <inheritdoc />
    public DeviceCommunicationException(Device device) : base(device) { }


    /// <inheritdoc />
    public DeviceCommunicationException(String? message, Device device) : base(message, device) { }


    /// <inheritdoc />
    public DeviceCommunicationException(String? message, Exception? innerException, Device device) : base(message, innerException, device) { }
}