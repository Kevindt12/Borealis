using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using Borealis.Networking.IO;

using Xunit;



namespace Borealis.Networking.Tests.Unit.IO;


public class SocketTests : IClassFixture<ServerSocketFixture>, IDisposable
{
	private readonly ServerSocketFixture _serverSocket;

	private readonly ISocket _socket;


	public SocketTests(ServerSocketFixture serverSocketFixture)
	{
		_serverSocket = serverSocketFixture;

		_socket = new TcpSocket(_serverSocket.ServerEndPoint);
	}


	/// <inheritdoc />
	public void Dispose()
	{
		_socket.Dispose();
	}


	#region Reading

	[Fact]
	public async Task ReceiveAsync_ReadDataFromTheSocketBuffer_WhenDataIsOnTheBuffer()
	{
		// Create
		byte[] sendBuffer = "Test Data"u8.ToArray();
		byte[] receiveBuffer = new Byte[sendBuffer.Length];

		// Arrange
		await _socket.ConnectAsync();
		Socket clientSocket = await _serverSocket.Socket.AcceptAsync();

		try
		{
			// Arrange
			await clientSocket.SendAsync(sendBuffer);

			int dataAvailable = _socket.DataAvailable;

			// Act
			int read = await _socket.ReceiveAsync(receiveBuffer);

			// Assert
			Assert.Equal(dataAvailable, read);
			Assert.Equal(sendBuffer.Length, read);
			Assert.Equal(sendBuffer, receiveBuffer);
		}
		finally
		{
			clientSocket.Dispose();
		}
	}


	[Fact]
	public async Task ReceiveAsync_ThrowsSocketException_WhenNoConnectionHasBeenMade()
	{
		// Arrange
		byte[] sendBuffer = "Test Data"u8.ToArray();

		// Act and assert
		await Assert.ThrowsAsync<SocketException>(async () => await _socket.SendAsync(sendBuffer));
	}


	[Fact]
	public async Task PeekAsync_ReadsTheBufferWithoutClearingIt_WhenThereIsDataOnTheBuffer()
	{
		// Create
		byte[] sendBuffer = "Test Data"u8.ToArray();
		byte[] receiveBuffer = new Byte[sendBuffer.Length];
		byte[] peekBuffer = new Byte[sendBuffer.Length];

		// Arrange
		await _socket.ConnectAsync();
		Socket clientSocket = await _serverSocket.Socket.AcceptAsync();

		try
		{
			// Arrange
			await clientSocket.SendAsync(sendBuffer);
			int dataAvailableBeforePeek = _socket.DataAvailable;

			// Act
			int peekRead = await _socket.PeekAsync(peekBuffer);
			int dataAvailableAfterPeek = _socket.DataAvailable;

			int read = await _socket.ReceiveAsync(receiveBuffer);
			int dataAvailableAfterRead = _socket.DataAvailable;

			// Assert
			Assert.Equal(dataAvailableBeforePeek, sendBuffer.Length);

			Assert.Equal(dataAvailableAfterPeek, sendBuffer.Length);
			Assert.Equal(peekRead, sendBuffer.Length);
			Assert.Equal(peekBuffer, receiveBuffer);

			Assert.Equal(read, sendBuffer.Length);
			Assert.Equal(sendBuffer, receiveBuffer);

			Assert.Equal(dataAvailableAfterRead, 0);
		}
		finally
		{
			clientSocket.Dispose();
		}
	}

	#endregion


	#region Connection

	[Fact]
	public async Task ConnectAsync_StartConnectionAsync_WithValidRemoteEndpoint()
	{
		// Act
		await _socket.ConnectAsync();
		Socket clientSocket = await _serverSocket.Socket.AcceptAsync();

		try
		{
			// Assert
			Assert.True(clientSocket.Connected);
		}
		finally
		{
			// Destroy
			clientSocket.Dispose();
		}
	}


	[Fact]
	public async Task ConnectAsync_ThrowSocketException_WhenNoServerAvailable()
	{
		// Arrange
		ISocket socket = new TcpSocket(new IPEndPoint(IPAddress.Loopback, 5500));

		// Act and assert
		await Assert.ThrowsAsync<SocketException>(() => socket.ConnectAsync());
	}


	[Fact]
	public async Task DisconnectAsync_DisconnectsFromSocket_WhenConnectionIsValid()
	{
		// Arrange
		await _socket.ConnectAsync();
		Socket clientSocket = await _serverSocket.Socket.AcceptAsync();

		try
		{
			bool connection = _socket.Connected;

			// Act
			await _socket.DisconnectAsync();

			// Assert
			Assert.True(connection);
			Assert.False(_socket.Connected);
		}
		finally
		{
			// Destroy
			clientSocket.Dispose();
		}
	}


	[Fact]
	public async Task DisconnectAsync_ThrowsSocketException_WhenNotConnected()
	{
		// Act and assert
		await Assert.ThrowsAsync<SocketException>(() => _socket.DisconnectAsync());
	}


	[Fact]
	public async Task ConnectAsync_ConnectAfterDisconnect_WhenDisconnectionWasCalledButConnectCalledAgain()
	{
		// Call this twice to see if we can reconnect with the same socket instance once we have disconnected..
		await DisconnectAsync_DisconnectsFromSocket_WhenConnectionIsValid();

		await DisconnectAsync_DisconnectsFromSocket_WhenConnectionIsValid();
	}

	#endregion


	#region Writing

	[Fact]
	public async Task SendAsync_SendDataToTheRemoteClient_WhenDataIsOnTheBuffer()
	{
		// Create
		byte[] sendBuffer = "Test Data"u8.ToArray();
		byte[] receiveBuffer = new Byte[sendBuffer.Length];

		// Arrange
		await _socket.ConnectAsync();
		Socket clientSocket = await _serverSocket.Socket.AcceptAsync();

		try
		{
			// Arrange
			await _socket.SendAsync(sendBuffer);

			// Act
			int read = await clientSocket.ReceiveAsync(receiveBuffer);

			Assert.Equal(sendBuffer.Length, read);
			Assert.Equal(sendBuffer, receiveBuffer);
		}
		finally
		{
			clientSocket.Dispose();
		}
	}


	[Fact]
	public async Task SendAsync_ThrowsSocketException_WhenNoConnectionHasBeenMade()
	{
		// Arrange
		byte[] sendBuffer = new byte[1];

		// Act and assert
		await Assert.ThrowsAsync<SocketException>(async () => await _socket.ReceiveAsync(sendBuffer));
	}

	#endregion
}