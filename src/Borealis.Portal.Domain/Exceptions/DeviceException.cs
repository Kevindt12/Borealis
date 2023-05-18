using Borealis.Portal.Domain.Devices.Models;



namespace Borealis.Portal.Domain.Exceptions;


public class DeviceException : ApplicationException
{
    /// <summary>
    /// The device on want the issue is about.
    /// </summary>
    public virtual Device? Device
    {
        get => (Device?)Data[nameof(Device)];
        set
        {
            if (Data[nameof(Device)] == null)
                Data.Add(nameof(Device), value);
            else
                Data[nameof(Device)] = value;
        }
    }


    /// <inheritdoc />
    public DeviceException() { }


    /// <inheritdoc />
    public DeviceException(String? message) : base(message) { }


    /// <inheritdoc />
    public DeviceException(String? message, Exception? innerException) : base(message, innerException) { }


    /// <inheritdoc />
    public DeviceException(Device device)
    {
        Device = device;
    }


    /// <inheritdoc />
    public DeviceException(String? message, Device device) : base(message)
    {
        Device = device;
    }


    /// <inheritdoc />
    public DeviceException(String? message, Exception? innerException, Device device) : base(message, innerException)
    {
        Device = device;
    }
}