using Borealis.Shared.Extensions;



namespace Borealis.Drivers.Rpi.Udp.Exceptions;


public class PortalException : ApplicationException
{
    /// <summary>
    /// The exception that was thrown at the portal side.
    /// </summary>
    public ExceptionInfo? PortalsException { get; set; }


    /// <inheritdoc />
    public PortalException() { }


    /// <inheritdoc />
    public PortalException(String? message) : base(message) { }


    /// <inheritdoc />
    public PortalException(String? message, Exception? innerException) : base(message, innerException) { }


    /// <inheritdoc />
    public PortalException(ExceptionInfo? portalsException)
    {
        PortalsException = portalsException;
    }


    /// <inheritdoc />
    public PortalException(String? message, ExceptionInfo? portalsException) : base(message)
    {
        PortalsException = portalsException;
    }


    /// <inheritdoc />
    public PortalException(String? message, Exception? innerException, ExceptionInfo? portalsException) : base(message, innerException)
    {
        PortalsException = portalsException;
    }
}