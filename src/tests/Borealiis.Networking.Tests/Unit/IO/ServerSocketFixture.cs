using System.Net;
using System.Net.Sockets;



namespace Borealis.Networking.Tests.Unit.IO;


public class ServerSocketFixture : IDisposable
{
	public IPEndPoint ServerEndPoint { get; protected set; } = new IPEndPoint(IPAddress.Loopback, 5555);


	public Socket Socket { get; protected set; }


	public ServerSocketFixture()
	{
		Socket socket = new Socket(ServerEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		socket.Bind(ServerEndPoint);

		socket.Listen(10);

		Socket = socket;
	}


	/// <inheritdoc />
	public void Dispose()
	{
		Socket.Close();
		Socket.Dispose();
	}
}