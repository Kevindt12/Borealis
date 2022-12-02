using Borealis.Portal.Domain.Devices;
using Borealis.Shared.Extensions;



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
    public DeviceConnectionException(String? message, ExceptionInfo info) : base(message)
    {
        Data.Add("Exception Info", info);
    }


    /// <inheritdoc />
    public DeviceConnectionException(String? message, Exception? innerException) : base(message, innerException) { }
}