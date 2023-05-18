using Borealis.Networking.Communication;



namespace Borealis.Networking.Exceptions;


/// <summary>
/// Un exception that is thrown when we received and error from an remote client.
/// </summary>
public class RemoteException : CommunicationException
{
	public RemoteException(String? message) : base(message) { }


	public RemoteException(String? message, Exception? innerException) : base(message, innerException) { }


	public RemoteException(CommunicationPacket packet, String? message) : base(packet, message) { }


	public RemoteException(CommunicationPacket packet, String? message, Exception? innerException) : base(packet, message, innerException) { }


	public RemoteException(object[] data, String? message) : base(message) { }


	public RemoteException(object[] data, String? message, Exception? innerException) : base(message, innerException) { }


	public RemoteException(CommunicationPacket packet, object[] data, String? message) : base(packet, message) { }


	public RemoteException(CommunicationPacket packet, object[] data, String? message, Exception? innerException) : base(packet, message, innerException) { }
}