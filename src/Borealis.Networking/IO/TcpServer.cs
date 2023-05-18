using System.Net;
using System.Net.Sockets;



namespace Borealis.Networking.IO;


public class TcpServer : ISocketServer
{
	private readonly ISocketFactory _socketFactory;
	private readonly Socket _socket;

	/// <inheritdoc />
	public EndPoint LocalEndPoint { get; }

	/// <inheritdoc />
	public Boolean IsRunning { get; protected set; }


	public TcpServer(ISocketFactory socketFactory, EndPoint localEndPoint)
	{
		_socketFactory = socketFactory;
		LocalEndPoint = localEndPoint;

		_socket = new Socket(LocalEndPoint.AddressFamily, SocketType.Stream, ProtocolType.IPv4);
	}


	/// <inheritdoc />
	public async Task StartAsync(CancellationToken token = default)
	{
		_socket.Bind(LocalEndPoint);
	}


	/// <inheritdoc />
	public async Task StopAsync(CancellationToken token = default)
	{
		_socket.Shutdown(SocketShutdown.Both);
		await _socket.DisconnectAsync(true, token).ConfigureAwait(false);
	}


	/// <inheritdoc />
	public async Task<ISocket> AcceptSocketAsync(CancellationToken token = default)
	{
		Socket socket = await _socket.AcceptAsync(token);

		return _socketFactory.CreateConnectedSocket(socket);
	}


	/// <inheritdoc />
	public void Dispose()
	{
		_socket.Dispose();
	}
}