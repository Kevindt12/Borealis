using Borealis.Portal.Domain.Devices;



namespace Borealis.Portal.Domain.Exceptions;


public class DeviceConnectionException : ApplicationException
{
    public Device? Device { get; init; }


    /// <inheritdoc />
    public DeviceConnectionException(Device? device)
    {
        Device = device;
    }


    /// <inheritdoc />
    public DeviceConnectionException(String? message, Device? device) : base(message)
    {
        Device = device;
    }


    /// <inheritdoc />
    public DeviceConnectionException(String? message, Exception? innerException, Device? device) : base(message, innerException)
    {
        Device = device;
    }


    /// <inheritdoc />
    public DeviceConnectionException() { }


    /// <inheritdoc />
    public DeviceConnectionException(String? message) : base(message) { }


    /// <inheritdoc />
    public DeviceConnectionException(String? message, Exception? innerException) : base(message, innerException) { }
}