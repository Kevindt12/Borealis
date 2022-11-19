using System;
using System.Linq;

using Borealis.Portal.Domain.Devices;



namespace Borealis.Portal.Core.Exceptions;


public class DeviceException : ApplicationException
{
    public Device Device { get; set; }


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


    /// <inheritdoc />
    public DeviceException(String? message) : base(message) { }


    /// <inheritdoc />
    public DeviceException(String? message, Exception? innerException) : base(message, innerException) { }
}