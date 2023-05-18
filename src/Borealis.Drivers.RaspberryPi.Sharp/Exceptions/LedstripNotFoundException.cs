namespace Borealis.Drivers.RaspberryPi.Sharp.Exceptions;


/// <summary>
/// Exception indicating that we did not find an ledstrip that was requested for.
/// </summary>
public class LedstripNotFoundException : ApplicationException
{
    /// <inheritdoc />
    public LedstripNotFoundException() { }


    /// <inheritdoc />
    public LedstripNotFoundException(String? message) : base(message) { }


    /// <inheritdoc />
    public LedstripNotFoundException(String? message, Exception? innerException) : base(message, innerException) { }
}