namespace Borealis.Domain.Communication.Exceptions;


public class CommunicationException : ApplicationException
{
    /// <inheritdoc />
    public CommunicationException() { }


    /// <inheritdoc />
    public CommunicationException(String? message) : base(message) { }


    /// <inheritdoc />
    public CommunicationException(String? message, Exception? innerException) : base(message, innerException) { }
}