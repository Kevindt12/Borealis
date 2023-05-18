namespace Borealis.Drivers.RaspberryPi.Sharp.Exceptions;


/// <summary>
/// Exception when a configuration file is not valid.
/// </summary>
public class InvalidConfigurationException : ApplicationException
{
    /// <inheritdoc />
    public InvalidConfigurationException() { }


    /// <inheritdoc />
    public InvalidConfigurationException(String? message) : base(message) { }


    /// <inheritdoc />
    public InvalidConfigurationException(String? message, Exception? innerException) : base(message, innerException) { }
}