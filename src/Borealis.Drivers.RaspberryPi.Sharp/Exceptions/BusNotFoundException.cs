namespace Borealis.Drivers.RaspberryPi.Sharp.Exceptions;


public class BusNotFoundException : ApplicationException
{
    /// <inheritdoc />
    public BusNotFoundException() { }


    /// <inheritdoc />
    public BusNotFoundException(String? message) : base(message) { }


    /// <inheritdoc />
    public BusNotFoundException(String? message, Exception? innerException) : base(message, innerException) { }
}