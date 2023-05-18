using System.Runtime.Serialization;



namespace Borealis.Drivers.RaspberryPi.Sharp.Exceptions;


public class InvalidSettingsException : ApplicationException
{
    /// <summary>
    /// A exception indicating that there is something wrong with an settings file.
    /// </summary>
    public InvalidSettingsException() { }


    /// <inheritdoc />
    protected InvalidSettingsException(SerializationInfo info, StreamingContext context) : base(info, context) { }


    /// <inheritdoc />
    public InvalidSettingsException(String? message, Exception? innerException) : base(message, innerException) { }
}