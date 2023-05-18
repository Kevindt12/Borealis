using Borealis.Networking.Exceptions;



namespace Borealis.Networking.Connections;


public class ConnectionDisconnectedEventArgs : EventArgs
{
	/// <summary>
	/// The exception that was thrown when the socket disconnected.
	/// </summary>
	public ConnectionException? ConnectionException { get; init; }
}