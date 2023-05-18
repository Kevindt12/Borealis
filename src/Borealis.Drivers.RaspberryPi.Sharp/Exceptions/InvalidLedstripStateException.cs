namespace Borealis.Drivers.RaspberryPi.Sharp.Exceptions;


/// <summary>
/// Thrown when we want to do something to an ledstrip but its state is invalid.
/// </summary>
public class InvalidLedstripStateException : ApplicationException
{
    /// <inheritdoc />
    public InvalidLedstripStateException() { }


    /// <inheritdoc />
    public InvalidLedstripStateException(String? message) : base(message) { }


    /// <inheritdoc />
    public InvalidLedstripStateException(String? message, Exception? innerException) : base(message, innerException) { }
}