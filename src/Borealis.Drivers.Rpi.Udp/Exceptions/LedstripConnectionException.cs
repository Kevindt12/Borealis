using Borealis.Domain.Devices;



namespace Borealis.Drivers.Rpi.Udp.Exceptions;


public class LedstripConnectionException : ApplicationException
{
    public Ledstrip Ledstrip { get; set; }


    /// <inheritdoc />
    public LedstripConnectionException() { }


    /// <inheritdoc />
    public LedstripConnectionException(String? message) : base(message) { }


    /// <inheritdoc />
    public LedstripConnectionException(String? message, Exception? innerException) : base(message, innerException) { }


    /// <inheritdoc />
    public LedstripConnectionException(Ledstrip ledstrip)
    {
        Ledstrip = ledstrip;
    }


    /// <inheritdoc />
    public LedstripConnectionException(String? message, Ledstrip ledstrip) : base(message)
    {
        Ledstrip = ledstrip;
    }


    /// <inheritdoc />
    public LedstripConnectionException(String? message, Exception? innerException, Ledstrip ledstrip) : base(message, innerException)
    {
        Ledstrip = ledstrip;
    }
}