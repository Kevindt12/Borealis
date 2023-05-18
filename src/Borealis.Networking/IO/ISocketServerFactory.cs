using System.Net;



namespace Borealis.Networking.IO;


/// <summary>
/// Creates the socket server used as an server to be able to connect clients to.
/// </summary>
public interface ISocketServerFactory
{
	/// <summary>
	/// Creates a new socket server for us to use.
	/// </summary>
	/// <param name="localEndPoint"> The end point used to listen for new connection. </param>
	/// <returns> The <see cref="ISocketServer" /> used for those connections. </returns>
	ISocketServer CreateSocketServer(EndPoint localEndPoint);
}