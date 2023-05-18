using System.Net;
using System.Net.Sockets;



namespace Borealis.Networking.IO;


internal class TcpSocket : ISocket
{
	private Socket _socket;

	/// <inheritdoc />
	public EndPoint RemoteEndPoint { get; }

	/// <inheritdoc />
	public Int32 DataAvailable
	{
		get
		{
			if (!_socket.Connected) return 0;

			return _socket.Available;
		}
	}

	/// <inheritdoc />
	public Boolean Connected => _socket.Connected;


	public TcpSocket(EndPoint endPoint)
	{
		RemoteEndPoint = endPoint;

		_socket = CreateSocket();
	}


	/// <summary>
	/// Called when a socket is created by the tcp server.
	/// </summary>
	/// <param name="socket"> </param>
	public TcpSocket(Socket socket)
	{
		RemoteEndPoint = socket.RemoteEndPoint!;
		_socket = socket;
	}


	private Socket CreateSocket()
	{
		return new Socket(RemoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
	}


	/// <inheritdoc />
	public async Task ConnectAsync(CancellationToken token = default)
	{
		await _socket.ConnectAsync(RemoteEndPoint, token).ConfigureAwait(false);
	}


	/// <inheritdoc />
	public async Task DisconnectAsync(CancellationToken token = default)
	{
		await _socket.DisconnectAsync(false, token).ConfigureAwait(false);

		// Create a new socket.
		_socket.Dispose();
		_socket = CreateSocket();
	}


	/// <inheritdoc />
	public async ValueTask<Int32> SendAsync(ReadOnlyMemory<Byte> data, CancellationToken token = default)
	{
		return await _socket.SendAsync(data, token).ConfigureAwait(false);
	}


	/// <inheritdoc />
	public async ValueTask<Int32> ReceiveAsync(Memory<Byte> data, CancellationToken token = default)
	{
		return await _socket.ReceiveAsync(data, token).ConfigureAwait(false);
	}


	/// <inheritdoc />
	public async ValueTask<Int32> PeekAsync(Memory<Byte> data, CancellationToken token = default)
	{
		return await _socket.ReceiveAsync(data, SocketFlags.Peek, token).ConfigureAwait(false);
	}


	#region IDisposalbe

	private bool _disposed;


	/// <inheritdoc />
	public void Dispose()
	{
		if (_disposed) return;

		Dispose(true);
		GC.SuppressFinalize(this);

		_disposed = true;
	}


	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_socket.Dispose();
		}
	}

	#endregion
}