namespace Borealis.Networking.Exceptions;


/// <summary>
/// An exception with the connection.
/// </summary>
public class ConnectionException : ApplicationException
{
	public ConnectionException() { }


	public ConnectionException(String? message) : base(message) { }


	public ConnectionException(String? message, Exception? innerException) : base(message, innerException) { }
}