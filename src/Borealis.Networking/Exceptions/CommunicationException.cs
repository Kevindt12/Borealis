using Borealis.Networking.Communication;



namespace Borealis.Networking.Exceptions;


/// <summary>
/// An exception that is thrown when there is an problem with the communication.
/// </summary>
public class CommunicationException : ApplicationException
{
	/// <summary>
	/// The communication that we where using to communicate with.
	/// </summary>
	public CommunicationPacket? CommunicationPacket
	{
		get => (CommunicationPacket?)Data[nameof(CommunicationPacket)];
		set => Data[nameof(CommunicationPacket)] = value;
	}


	public CommunicationException() { }


	public CommunicationException(String? message) : base(message) { }


	public CommunicationException(String? message, Exception? innerException) : base(message, innerException) { }


	public CommunicationException(CommunicationPacket packet, String? message) : base(message)
	{
		CommunicationPacket = packet;
	}


	public CommunicationException(CommunicationPacket packet, String? message, Exception? innerException) : base(message, innerException)
	{
		CommunicationPacket = packet;
	}
}