using System.Net.Sockets;
using System.Text;

using Borealis.Networking.Connections;
using Borealis.Networking.Exceptions;
using Borealis.Networking.IO;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using Xunit;



namespace Borealis.Networking.Tests.Unit.Connections;


public class ConnectionTests : IDisposable
{
	private static readonly byte[] _testData = Encoding.ASCII.GetBytes("Hello world");

	private readonly IConnection _connection;

	private readonly Mock<ISocket> _socketMock;


	public ConnectionTests()
	{
		NullLogger<Connection> logger = NullLogger<Connection>.Instance;

		_socketMock = new Mock<ISocket>();

		_connection = new Connection(logger, _socketMock.Object);
	}


	[Fact]
	public async Task ConnectAsync_ConnectsToTheSocket_WhenSocketIsUp()
	{
		// Arrange
		_socketMock.Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>()));

		// Act
		await _connection.ConnectAsync();

		// Assert
		_socketMock.Verify(x => x.ConnectAsync(It.IsAny<CancellationToken>()), Times.Once);
	}


	[Fact]
	public async Task ConnectAsync_ThrowConnectionException_WhenConnectionIsAlreadyEstablished()
	{
		// Arrange
		_socketMock.SetupGet(x => x.Connected).Returns(true);

		// Act and Assert
		await Assert.ThrowsAsync<ConnectionException>(async () => await _connection.ConnectAsync());
	}


	[Fact]
	public async Task ConnectAsync_ThrowConnectionException_WhenConnectionCouldNotBeEstablished()
	{
		// Arrange
		_socketMock.SetupGet(x => x.Connected).Returns(false);
		_socketMock.Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>())).Throws<SocketException>();

		// Act and Assert
		await Assert.ThrowsAsync<ConnectionException>(async () => await _connection.ConnectAsync());
	}


	[Fact]
	public async Task DisconnectAsync_DisconnectsFromTheRemoteClient_WhenSocketIsConnected()
	{
		// Arrange
		await ConnectAsync_ConnectsToTheSocket_WhenSocketIsUp();

		ReadOnlyMemory<byte> disconnectFrame = FrameHeader.DisconnectFrame().CreateBuffer().ToArray();
		_socketMock.Setup(x => x.SendAsync(It.Is<ReadOnlyMemory<byte>>(y => disconnectFrame.ToArray().SequenceEqual(y.ToArray())), It.IsAny<CancellationToken>()));
		_socketMock.Setup(x => x.DisconnectAsync(It.IsAny<CancellationToken>()));

		_socketMock.SetupGet(x => x.Connected).Returns(true);

		// Act
		await _connection.DisconnectAsync();

		// Assert
		_socketMock.Verify(x => x.DisconnectAsync(It.IsAny<CancellationToken>()), Times.Once);
		_socketMock.Verify(x => x.SendAsync(It.Is<ReadOnlyMemory<byte>>(y => disconnectFrame.ToArray().SequenceEqual(y.ToArray())), It.IsAny<CancellationToken>()), Times.Once);
	}


	[Fact]
	public async Task DisconnectAsync_ThrowConnectionException_WhenSocketIsDisconnected()
	{
		// Arrange
		_socketMock.SetupGet(x => x.Connected).Returns(false);

		// Act and Assert
		await Assert.ThrowsAsync<ConnectionException>(async () => await _connection.DisconnectAsync());
	}


	[Fact]
	public async Task SendAsync_SendDataToTheRemoteConnection_WhenDataIsValid()
	{
		// Arrange
		_socketMock.Setup(x => x.SendAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()));
		_socketMock.SetupGet(x => x.Connected).Returns(true);

		// Act
		await _connection.SendAsync(_testData, CancellationToken.None);

		// Assert
		_socketMock.Verify(x => x.SendAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Once);
	}


	[Fact]
	public async Task SendAsync_ThrowConnectionException_WhenSocketThrowSocketException()
	{
		// Arrange
		_socketMock.Setup(x => x.SendAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>())).Throws<SocketException>();
		_socketMock.SetupGet(x => x.Connected).Returns(true);

		// Act and Assert
		await Assert.ThrowsAsync<ConnectionException>(async () => await _connection.SendAsync(_testData, CancellationToken.None));
	}


	[Fact]
	public async Task SendAsync_ThrowConnectionException_WhenSocketIsDisconnected()
	{
		// Arrange
		_socketMock.SetupGet(x => x.Connected).Returns(false);

		// Act and Assert
		await Assert.ThrowsAsync<ConnectionException>(async () => await _connection.SendAsync(_testData, CancellationToken.None));
	}


	[Fact]
	public async Task ReceiveAsync_ReceivesDataFromTheBuffer_WhenDataIsAvailableOnTheSocketBuffer()
	{
		// Create
		Memory<byte> frameHeaderBuffer = new FrameHeader(FrameType.Packet, _testData.Length).CreateBuffer().ToArray();

		MockSequence sequence = new MockSequence();

		// Arrange
		_socketMock.SetupGet(x => x.Connected).Returns(true);

		_socketMock.InSequence(sequence).Setup(x => x.ReceiveAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>())).Callback<Memory<byte>, CancellationToken>((buffer, ctx) => frameHeaderBuffer.CopyTo(buffer));
		_socketMock.InSequence(sequence).Setup(x => x.ReceiveAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>())).Callback<Memory<byte>, CancellationToken>((buffer, ctx) => _testData.CopyTo(buffer)).Returns(ValueTask.FromResult(_testData.Length));

		// Act
		ReadOnlyMemory<byte> data = await _connection.ReceiveAsync(CancellationToken.None);

		// Assert
		Assert.Equal(_testData, data.ToArray());
		_socketMock.Verify(x => x.ReceiveAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
	}


	[Fact]
	public async Task ReceiveAsync_ThrowConnectionException_WhenSocketThrowSocketException()
	{
		// Arrange
		_socketMock.Setup(x => x.SendAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>())).Throws<SocketException>();
		_socketMock.SetupGet(x => x.Connected).Returns(true);

		// Act and Assert
		await Assert.ThrowsAsync<ConnectionException>(async () => await _connection.SendAsync(_testData, CancellationToken.None));
	}


	[Fact]
	public async Task ReceiveAsync_ThrowConnectionException_WhenSocketIsDisconnected()
	{
		// Arrange
		_socketMock.SetupGet(x => x.Connected).Returns(false);

		// Act and Assert
		await Assert.ThrowsAsync<ConnectionException>(async () => await _connection.SendAsync(_testData, CancellationToken.None));
	}


	[Fact]
	public async Task ConnectionStateThread_VerifyDataAvailableCalledIndicatingThreadStarted_WhenConnectionHasSucceeded()
	{
		// Arrange
		_socketMock.SetupGet(x => x.DataAvailable).Returns(0);

		// Act
		await ConnectAsync_ConnectsToTheSocket_WhenSocketIsUp();

		// Assert
		_socketMock.VerifyGet(x => x.DataAvailable, Times.AtLeastOnce);
	}


	/// <inheritdoc />
	public void Dispose()
	{
		_connection.Dispose();
	}
}