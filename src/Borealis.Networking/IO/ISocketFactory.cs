using System.Net;
using System.Net.Sockets;



namespace Borealis.Networking.IO;


/// <summary>
/// The factory that creates <see cref="ISocket" />
/// </summary>
public interface ISocketFactory
{
	/// <summary>
	/// Creates the socket with a specified endpoint.
	/// </summary>
	/// <param name="endPoint"> The <see cref="EndPoint" /> we want to connect to. </param>
	/// <returns>
	/// A <see cref="ISocket" /> that can connect to that
	/// <see cref="EndPoint" />.
	/// </returns>
	ISocket CreateSocket(EndPoint endPoint);


	/// <summary>
	/// Creates a socket from an connected socket.
	/// </summary>
	/// <param name="socket"> The connected <see cref="Socket" /> that we want to use in our connection. </param>
	/// <returns> A <see cref="ISocket" /> abstraction over the socket. </returns>
	ISocket CreateConnectedSocket(Socket socket);
}