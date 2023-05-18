using System.Net;
using System.Net.Sockets;



namespace Borealis.Networking.IO;


internal class SocketFactory : ISocketFactory
{
	public SocketFactory() { }


	/// <inheritdoc />
	public ISocket CreateSocket(EndPoint endPoint)
	{
		return new TcpSocket(endPoint);
	}


	/// <inheritdoc />
	public ISocket CreateConnectedSocket(Socket socket)
	{
		return new TcpSocket(socket);
	}
}