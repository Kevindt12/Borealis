using System.Net;



namespace Borealis.Networking.IO;


internal class SocketServerFactory : ISocketServerFactory
{
	private readonly ISocketFactory _socketFactory;


	public SocketServerFactory(ISocketFactory socketFactory)
	{
		_socketFactory = socketFactory;
	}


	/// <inheritdoc />
	public ISocketServer CreateSocketServer(EndPoint localEndPoint)
	{
		return new TcpServer(_socketFactory, localEndPoint);
	}
}