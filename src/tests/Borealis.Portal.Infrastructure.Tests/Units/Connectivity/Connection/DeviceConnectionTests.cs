using System.Net;
using System.Net.Sockets;
using System.Reflection;

using Borealis.Domain.Ledstrips;
using Borealis.Portal.Domain.Connectivity.Models;
using Borealis.Portal.Domain.Devices.Models;
using Borealis.Portal.Domain.Exceptions;
using Borealis.Portal.Domain.Ledstrips.Models;
using Borealis.Portal.Infrastructure.Communication;
using Borealis.Portal.Infrastructure.Connectivity.Connections;
using Borealis.Portal.Infrastructure.Connectivity.Factories;
using Borealis.Portal.Infrastructure.Connectivity.Handlers;
using Borealis.Portal.Infrastructure.Connectivity.Options;
using Borealis.Portal.Infrastructure.Connectivity.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;



namespace Borealis.Portal.Infrastructure.Tests.Units.Connectivity.Connection;


public class DeviceConnectionTests
{
	private const int TcpPort = 9000;

	private readonly Mock<MessageSerializer> _messageSerializerMock;
	private readonly Mock<LedstripConnectionFactory> _ledstripConnectionFactoryMock;
	private readonly Mock<CommunicationHandlerFactory> _communicationHandlerFactoryMock;

	private readonly DeviceConnection _deviceConnection;
	private readonly Device _device;


	public DeviceConnectionTests()
	{
		_device = new Device
		{
			Id = Guid.NewGuid(),
			Name = "Test Device",
			ConfigurationConcurrencyToken = "Test Token",
			EndPoint = new IPEndPoint(IPAddress.Loopback, TcpPort)
		};

		List<DevicePort> ports = new List<DevicePort> { new DevicePort(_device, 0, new Ledstrip("test", 200, LedstripChip.WS2812B)) };

		typeof(Device).GetField("_ports", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(_device, ports);

		IOptions<ConnectivityOptions> connectivityOptions = Options.Create(new ConnectivityOptions
		{
			ReceiveTimeout = 5000,
			ResponseTimeout = 10000
		});

		_messageSerializerMock = new Mock<MessageSerializer>();
		_ledstripConnectionFactoryMock = new Mock<LedstripConnectionFactory>(Mock.Of<ILoggerFactory>(), Mock.Of<MessageSerializer>());
		_communicationHandlerFactoryMock = new Mock<CommunicationHandlerFactory>(Mock.Of<ILoggerFactory>(), connectivityOptions, Options.Create(new KeepAliveOptions()));

		_deviceConnection = new DeviceConnection(NullLogger<DeviceConnection>.Instance, connectivityOptions, _messageSerializerMock.Object, _ledstripConnectionFactoryMock.Object, _communicationHandlerFactoryMock.Object, _device);
	}


	private TcpListener StartTcpListener()
	{
		TcpListener listener = new TcpListener(IPAddress.Any, TcpPort);
		listener.Start();

		_ = Task.Run(async () =>
		{
			TcpClient client = await listener.AcceptTcpClientAsync();
			client.Dispose();
			listener.Stop();
		});

		return listener;
	}


	#region Helpers

	private Mock<ICommunicationHandler> SetupCommunicationHandler()
	{
		Mock<ICommunicationHandler> communicationHandlerMock = new Mock<ICommunicationHandler>();

		typeof(DeviceConnection).GetField("_communicationHandler", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(_deviceConnection, communicationHandlerMock.Object);

		return communicationHandlerMock;
	}

	#endregion


	#region UploadConfigurationAsync

	[Fact]
	public async Task UploadConfigurationAsync_WhenUploadIsSuccessful_SetFlagIndicatingGoodUploadAndLoadTheLedstripConnections()
	{
		// Create
		bool success = true;
		string? errorMessage = null;

		// Arrange

		// Messages Serialization.
		_messageSerializerMock.Setup(x => x.SerializeSetConfigurationRequest(It.IsAny<string>(), It.IsAny<IEnumerable<DevicePort>>()))
							  .Returns(CommunicationPacket.NullPacket);

		_messageSerializerMock.Setup(x => x.DeserializeSetConfigurationReplyPacket(It.IsAny<CommunicationPacket>(), out success, out errorMessage));

		// Communication Handler
		Mock<ICommunicationHandler> communicationHandlerMock = SetupCommunicationHandler();

		communicationHandlerMock.Setup(x => x.SendWithReplyAsync(It.IsAny<CommunicationPacket>(), It.IsAny<CancellationToken>()))
								.Returns(Task.FromResult(new CommunicationPacket(PacketIdentifier.SetConfigurationReply, ReadOnlyMemory<Byte>.Empty)));

		// Ledstrip connection
		_ledstripConnectionFactoryMock.Setup(x => x.Create(It.IsAny<Ledstrip>(), It.IsAny<ICommunicationHandler>()))
									  .Returns(Mock.Of<IDeviceLedstripConnection>());

		// Act
		await _deviceConnection.UploadConfigurationAsync(CancellationToken.None);

		// Assert
		Assert.True(_deviceConnection.IsConfigurationValid);
		_ledstripConnectionFactoryMock.Verify(x => x.Create(It.IsAny<Ledstrip>(), It.IsAny<ICommunicationHandler>()), Times.AtLeastOnce);
	}


	[Fact]
	public async Task UploadConfigurationAsync_ThrowsDeviceConfigurationException_WhenUnableToSetConfiguration()
	{
		// Create
		bool success = false;
		string? errorMessage = "Test error message";

		// Arrange

		// Messages Serialization.
		_messageSerializerMock.Setup(x => x.SerializeSetConfigurationRequest(It.IsAny<string>(), It.IsAny<IEnumerable<DevicePort>>()))
							  .Returns(CommunicationPacket.NullPacket);

		_messageSerializerMock.Setup(x => x.DeserializeSetConfigurationReplyPacket(It.IsAny<CommunicationPacket>(), out success, out errorMessage));

		// Communication Handler
		Mock<ICommunicationHandler> communicationHandlerMock = SetupCommunicationHandler();

		communicationHandlerMock.Setup(x => x.SendWithReplyAsync(It.IsAny<CommunicationPacket>(), It.IsAny<CancellationToken>()))
								.Returns(Task.FromResult(new CommunicationPacket(PacketIdentifier.SetConfigurationReply, ReadOnlyMemory<Byte>.Empty)));

		// Ledstrip connection
		_ledstripConnectionFactoryMock.Setup(x => x.Create(It.IsAny<Ledstrip>(), It.IsAny<ICommunicationHandler>()))
									  .Returns(Mock.Of<IDeviceLedstripConnection>());

		// Act abd Assert
		await Assert.ThrowsAsync<DeviceConfigurationException>(() => _deviceConnection.UploadConfigurationAsync(CancellationToken.None));

		// Assert
		Assert.False(_deviceConnection.IsConfigurationValid);
		_ledstripConnectionFactoryMock.Verify(x => x.Create(It.IsAny<Ledstrip>(), It.IsAny<ICommunicationHandler>()), Times.Never);
		Assert.Empty(_deviceConnection.LedstripConnections);
	}

	#endregion


	#region ConnectAsync

	[Fact]
	public async Task ConnectAsync_ValidConnectionStateWithValidConfiguration_WhenConfigurationTokenIsSameAndConnectionCanBeStarted()
	{
		// Create
		bool isConfigurationValid = true;

		// Arrange
		// Starting the TCP Server
		StartTcpListener();

		// Message serialization
		_messageSerializerMock.Setup(x => x.SerializeConnectRequest(It.IsAny<string>()))
							  .Returns(CommunicationPacket.NullPacket);

		_messageSerializerMock.Setup(x => x.DeserializeConnectReply(It.IsAny<CommunicationPacket>(), out isConfigurationValid));

		// Ledstrip Connections
		_ledstripConnectionFactoryMock.Setup(x => x.Create(It.IsAny<Ledstrip>(), It.IsAny<ICommunicationHandler>())).Returns(Mock.Of<IDeviceLedstripConnection>());

		// Communication Handler
		Mock<ICommunicationHandler> communicationHandlerMock = new Mock<ICommunicationHandler>();
		communicationHandlerMock.Setup(x => x.SendWithReplyAsync(It.IsAny<CommunicationPacket>(), It.IsAny<CancellationToken>()))
								.Returns(Task.FromResult(new CommunicationPacket(PacketIdentifier.ConnectReply, ReadOnlyMemory<Byte>.Empty)));

		// Communication Handler Factory
		_communicationHandlerFactoryMock.Setup(x => x.Create(It.IsAny<TcpClient>(), It.IsAny<ReceivedHandler>()))
										.Returns(communicationHandlerMock.Object);

		// Act
		DeviceConnectionResult result = await _deviceConnection.ConnectAsync();

		// Assert
		_messageSerializerMock.Verify(x => x.SerializeConnectRequest(It.IsAny<string>()), Times.Once);
		_messageSerializerMock.Verify(x => x.DeserializeConnectReply(It.IsAny<CommunicationPacket>(), out isConfigurationValid), Times.Once);
		communicationHandlerMock.Verify(x => x.SendWithReplyAsync(It.IsAny<CommunicationPacket>(), It.IsAny<CancellationToken>()), Times.Once);
		_communicationHandlerFactoryMock.Verify(x => x.Create(It.IsAny<TcpClient>(), It.IsAny<ReceivedHandler>()), Times.Once);

		_ledstripConnectionFactoryMock.Verify(x => x.Create(It.IsAny<Ledstrip>(), It.IsAny<ICommunicationHandler>()), Times.AtLeastOnce);

		Assert.NotEmpty(_deviceConnection.LedstripConnections);
		Assert.True(result.ConfigurationValid);
	}


	[Fact]
	public async Task ConnectAsync_AbleToConnectWhenConfigurationAreNotTheSame_StartsTheConnectionButDoesNotCreateTheLedstripConnections()
	{
		// Create
		bool isConfigurationValid = false;

		// Arrange
		// Starting the TCP Server
		StartTcpListener();

		// Message serialization
		_messageSerializerMock.Setup(x => x.SerializeConnectRequest(It.IsAny<string>()))
							  .Returns(CommunicationPacket.NullPacket);

		_messageSerializerMock.Setup(x => x.DeserializeConnectReply(It.IsAny<CommunicationPacket>(), out isConfigurationValid));

		// Ledstrip Connections
		_ledstripConnectionFactoryMock.Setup(x => x.Create(It.IsAny<Ledstrip>(), It.IsAny<ICommunicationHandler>())).Returns(Mock.Of<IDeviceLedstripConnection>());

		// Communication Handler
		Mock<ICommunicationHandler> communicationHandlerMock = new Mock<ICommunicationHandler>();
		communicationHandlerMock.Setup(x => x.SendWithReplyAsync(It.IsAny<CommunicationPacket>(), It.IsAny<CancellationToken>()))
								.Returns(Task.FromResult(new CommunicationPacket(PacketIdentifier.ConnectReply, ReadOnlyMemory<Byte>.Empty)));

		// Communication Handler Factory
		_communicationHandlerFactoryMock.Setup(x => x.Create(It.IsAny<TcpClient>(), It.IsAny<ReceivedHandler>()))
										.Returns(communicationHandlerMock.Object);

		// Act
		DeviceConnectionResult result = await _deviceConnection.ConnectAsync();

		// Assert
		_messageSerializerMock.Verify(x => x.SerializeConnectRequest(It.IsAny<string>()), Times.Once);
		_messageSerializerMock.Verify(x => x.DeserializeConnectReply(It.IsAny<CommunicationPacket>(), out isConfigurationValid), Times.Once);
		communicationHandlerMock.Verify(x => x.SendWithReplyAsync(It.IsAny<CommunicationPacket>(), It.IsAny<CancellationToken>()), Times.Once);
		_communicationHandlerFactoryMock.Verify(x => x.Create(It.IsAny<TcpClient>(), It.IsAny<ReceivedHandler>()), Times.Once);

		_ledstripConnectionFactoryMock.Verify(x => x.Create(It.IsAny<Ledstrip>(), It.IsAny<ICommunicationHandler>()), Times.Never);

		Assert.Empty(_deviceConnection.LedstripConnections);
		Assert.False(result.ConfigurationValid);
	}


	[Fact]
	public async Task ConnectAsync_ThrowsInvalidOperationException_WhenTheConnectionHasAlreadyStarted()
	{
		// Arrange
		await ConnectAsync_ValidConnectionStateWithValidConfiguration_WhenConfigurationTokenIsSameAndConnectionCanBeStarted();

		// Act and Assert
		await Assert.ThrowsAsync<InvalidOperationException>(() => _deviceConnection.ConnectAsync());
	}


	[Fact]
	public async Task ConnectAsync_ThrowsDeviceConnectionException_WhenEndPointIsUnreachable()
	{
		// Act and Assert
		await Assert.ThrowsAsync<DeviceConnectionException>(() => _deviceConnection.ConnectAsync());
	}


	[Fact]
	public async Task ConnectAsync_ThrowsDeviceException_WhenErrorHasBeenReceivedFromDevice()
	{
		// Create
		string testErrorMessage = "Test Error";

		// Arrange
		// Starting the TCP Server.
		StartTcpListener();

		// Message serializer.
		_messageSerializerMock.Setup(x => x.SerializeConnectRequest(It.IsAny<string>()))
							  .Returns(CommunicationPacket.NullPacket);

		_messageSerializerMock.Setup(x => x.DeserializeErrorReplyPacket(It.IsAny<CommunicationPacket>(), out testErrorMessage));

		// Communication Handler
		Mock<ICommunicationHandler> communicationHandlerMock = new Mock<ICommunicationHandler>();
		communicationHandlerMock.Setup(x => x.SendWithReplyAsync(It.IsAny<CommunicationPacket>(), It.IsAny<CancellationToken>()))
								.Returns(Task.FromResult(new CommunicationPacket(PacketIdentifier.ErrorReply, ReadOnlyMemory<Byte>.Empty)));

		communicationHandlerMock.Setup(x => x.DisposeAsync());

		// Communication Handler Factory
		_communicationHandlerFactoryMock.Setup(x => x.Create(It.IsAny<TcpClient>(), It.IsAny<ReceivedHandler>()))
										.Returns(communicationHandlerMock.Object);

		// Act and Assert
		await Assert.ThrowsAsync<DeviceException>(() => _deviceConnection.ConnectAsync());

		// Assert
		communicationHandlerMock.Verify(x => x.DisposeAsync());
	}


	[Fact]
	public async Task ConnectAsync_ThrowsDeviceCommunicationException_WhenAnUnknownPacketHasBeenReceivedFromTheDevice()
	{
		// Create
		StartTcpListener();

		// Arrange
		// Message Serialization
		_messageSerializerMock.Setup(x => x.SerializeConnectRequest(It.IsAny<string>()))
							  .Returns(CommunicationPacket.NullPacket);

		// Communication Handler
		Mock<ICommunicationHandler> communicationHandlerMock = new Mock<ICommunicationHandler>();
		communicationHandlerMock.Setup(x => x.SendWithReplyAsync(It.IsAny<CommunicationPacket>(), It.IsAny<CancellationToken>()))
								.Returns(Task.FromResult(new CommunicationPacket(0, ReadOnlyMemory<Byte>.Empty)));

		communicationHandlerMock.Setup(x => x.DisposeAsync());

		_communicationHandlerFactoryMock.Setup(x => x.Create(It.IsAny<TcpClient>(), It.IsAny<ReceivedHandler>()))
										.Returns(communicationHandlerMock.Object);

		// Act

		// Act and Assert
		await Assert.ThrowsAsync<DeviceCommunicationException>(() => _deviceConnection.ConnectAsync());
		communicationHandlerMock.Verify(x => x.DisposeAsync());
	}

	#endregion
}