namespace Borealis.Networking.Connections;


public enum FrameType : byte
{
	/// <summary>
	/// indicating that the contents of the next fame is an packet.
	/// </summary>
	Packet = 0,

	/// <summary>
	/// Indicating that we have received an keep alive message from the connection.
	/// </summary>
	KeepAlive = 1,

	/// <summary>
	/// Indicating that we want to disconnect from the remote client.
	/// </summary>
	Disconnect = 2
}