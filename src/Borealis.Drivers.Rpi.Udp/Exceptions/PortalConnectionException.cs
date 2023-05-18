using System;
using System.Linq;



namespace Borealis.Drivers.Rpi.Exceptions;


public class PortalConnectionException : PortalException
{
    /// <inheritdoc />
    public PortalConnectionException() { }


    /// <inheritdoc />
    public PortalConnectionException(String? message) : base(message) { }


    /// <inheritdoc />
    public PortalConnectionException(String? message, Exception? innerException) : base(message, innerException) { }
}