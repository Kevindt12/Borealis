using System.Net;
using System.Net.Sockets;
using System.Text;

using Borealis.Communication.Messages;
using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Communication;
using Borealis.Drivers.RaspberryPi.Sharp.Communication.Serialization;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Controllers;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Core;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Options;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Manager;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Validation;
using Borealis.Drivers.RaspberryPi.Sharp.Exceptions;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Service;

using Google.FlatBuffers;

using Microsoft.Extensions.Logging;

using Moq;

using UnitsNet;

using Xunit;

using LedstripChip = Borealis.Communication.Messages.LedstripChip;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;



namespace Borealis.Drivers.RaspberryPi.Sharp.Tests.Unit.Connection;


public class ConnectionTests : IAsyncLifetime
{
    private TcpClient _server;

    private PortalConnection _connection;

    private readonly Mock<ConnectionController> _connectionControllerMock;


    public ConnectionTests()
    {
        _connectionControllerMock = new Mock<ConnectionController>(Mock.Of<ILogger<ConnectionController>>(), Mock.Of<IDeviceConfigurationValidator>(), Mock.Of<ILedstripControlService>(), Mock.Of<ILedstripConfigurationService>(), Mock.Of<IDeviceConfigurationManager>());
    }


    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        (TcpClient client, TcpClient server) connections = await CreateConnection(8888);

        _server = connections.server;

        _connection = new PortalConnection(Mock.Of<ILogger<PortalConnection>>(),
                                           MicrosoftOptions.Create(new PortalConnectionOptions
                                                                       { ReceiveTimeoutDuration = 100000 }),
                                           connections.client,
                                           new MessageSerializer(),
                                           _connectionControllerMock.Object);
    }


    /// <inheritdoc />
    public async Task DisposeAsync() { }


    #region Helpers

    private List<PixelMessageT> GenerateFrame(int count)
    {
        Random random = new Random();

        PixelMessageT[] pixels = new PixelMessageT[count];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new PixelMessageT
            {
                R = Convert.ToByte(random.Next(0, 250)),
                G = Convert.ToByte(random.Next(0, 250)),
                B = Convert.ToByte(random.Next(0, 250))
            };
        }

        return pixels.ToList();
    }

    #endregion


    #region Remote Procedure Calls

    [Fact]
    public async Task StartAnimationAsync_SendRequestToStartAnimation_CallControllerAndStartThenAnimation()
    {
        // Create
        StartAnimationRequestT request = new StartAnimationRequestT();

        request.Frequency = Convert.ToSingle(Frequency.FromHertz(10).Value);
        request.LedstripId = Guid.NewGuid().ToString();

        request.InitialFrameBuffer = new List<FrameMessageT>(Enumerable.Range(0, 20)
                                                                       .Select(x => new FrameMessageT
                                                                                   { Pixels = GenerateFrame(10) })
                                                                       .ToList());

        // Building the packet
        FlatBufferBuilder builder = new FlatBufferBuilder(4096);
        builder.Finish(StartAnimationRequest.Pack(builder, request).Value);
        CommunicationPacket requestPacket = new CommunicationPacket(PacketIdentifier.StartAnimationRequest, builder.SizedByteArray());

        // Arrange
        _connectionControllerMock.Setup(x => x.StartAnimationAsync(It.IsAny<Guid>(), It.IsAny<Frequency>(), It.IsAny<IEnumerable<ReadOnlyMemory<PixelColor>>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await WriteFromServerAsync(requestPacket);

        // Act Response
        CommunicationPacket replyPacket = await ReadFromServerAsync();

        // Assert
        _connectionControllerMock.Verify(x => x.StartAnimationAsync(It.IsAny<Guid>(), It.IsAny<Frequency>(), It.IsAny<IEnumerable<ReadOnlyMemory<PixelColor>>>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(PacketIdentifier.SuccessReply, replyPacket.Identifier);
    }


    [Fact]
    public async Task PauseAnimationAsync_SendRequestToStartAnimation_CallControllerAndStartThenAnimation()
    {
        // Create
        PauseAnimationRequestT request = new PauseAnimationRequestT();

        request.LedstripId = Guid.NewGuid().ToString();

        // Building the packet
        FlatBufferBuilder builder = new FlatBufferBuilder(4096);
        builder.Finish(PauseAnimationRequest.Pack(builder, request).Value);
        CommunicationPacket requestPacket = new CommunicationPacket(PacketIdentifier.PauseAnimationRequest, builder.SizedByteArray());

        // Arrange
        _connectionControllerMock.Setup(x => x.PauseAnimationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await WriteFromServerAsync(requestPacket);

        // Act Response
        CommunicationPacket replyPacket = await ReadFromServerAsync();

        // Assert
        _connectionControllerMock.Verify(x => x.PauseAnimationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(PacketIdentifier.SuccessReply, replyPacket.Identifier);
    }


    [Fact]
    public async Task StopAnimation_SendRequestToStartAnimation_CallControllerAndStartThenAnimation()
    {
        // Create
        StopAnimationRequestT request = new StopAnimationRequestT();

        request.LedstripId = Guid.NewGuid().ToString();

        // Building the packet
        FlatBufferBuilder builder = new FlatBufferBuilder(4096);
        builder.Finish(StopAnimationRequest.Pack(builder, request).Value);
        CommunicationPacket requestPacket = new CommunicationPacket(PacketIdentifier.StopAnimationRequest, builder.SizedByteArray());

        // Arrange
        _connectionControllerMock.Setup(x => x.StopAnimationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await WriteFromServerAsync(requestPacket);

        // Act Response
        CommunicationPacket replyPacket = await ReadFromServerAsync();

        // Assert
        _connectionControllerMock.Verify(x => x.StopAnimationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(PacketIdentifier.SuccessReply, replyPacket.Identifier);
    }


    [Fact]
    public async Task SetLedstripFrameAsync_SendRequestToStartAnimation_CallControllerAndStartThenAnimation()
    {
        // Create
        SetLedstripColorRequestT request = new SetLedstripColorRequestT();

        request.Frame = new FrameMessageT
        {
            Pixels = GenerateFrame(10)
        };

        request.LedstripId = Guid.NewGuid().ToString();

        // Building the packet
        FlatBufferBuilder builder = new FlatBufferBuilder(4096);
        builder.Finish(SetLedstripColorRequest.Pack(builder, request).Value);
        CommunicationPacket requestPacket = new CommunicationPacket(PacketIdentifier.DisplayFrameRequest, builder.SizedByteArray());

        // Arrange
        _connectionControllerMock.Setup(x => x.SetLedstripFrameAsync(It.IsAny<Guid>(), It.IsAny<ReadOnlyMemory<PixelColor>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await WriteFromServerAsync(requestPacket);

        // Act Response
        CommunicationPacket replyPacket = await ReadFromServerAsync();

        // Assert
        _connectionControllerMock.Verify(x => x.SetLedstripFrameAsync(It.IsAny<Guid>(), It.IsAny<ReadOnlyMemory<PixelColor>>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(PacketIdentifier.SuccessReply, replyPacket.Identifier);
    }


    [Fact]
    public async Task ClearLedstripFrameAsync_SendRequestToStartAnimation_CallControllerAndStartThenAnimation()
    {
        // Create
        ClearLedstripRequestT request = new ClearLedstripRequestT();

        request.LedstripId = Guid.NewGuid().ToString();

        // Building the packet
        FlatBufferBuilder builder = new FlatBufferBuilder(1024);
        builder.Finish(ClearLedstripRequest.Pack(builder, request).Value);
        CommunicationPacket requestPacket = new CommunicationPacket(PacketIdentifier.ClearLedstripRequest, builder.SizedByteArray());

        // Arrange
        _connectionControllerMock.Setup(x => x.ClearLedstripAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await WriteFromServerAsync(requestPacket);

        // Act Response
        CommunicationPacket replyPacket = await ReadFromServerAsync();

        // Assert
        _connectionControllerMock.Verify(x => x.ClearLedstripAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(PacketIdentifier.SuccessReply, replyPacket.Identifier);
    }

    #endregion


    #region Connection

    public static async Task<(TcpClient client, TcpClient server)> CreateConnection(int port)
    {
        TcpListener listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();

        TcpClient client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port);

        TcpClient server = await listener.AcceptTcpClientAsync();

        server.Client.NoDelay = true;
        client.Client.NoDelay = true;

        listener.Stop();

        return (client, server);
    }


    private async Task WriteFromServerAsync(CommunicationPacket packet)
    {
        ReadOnlyMemory<byte> requestPacketBuffer = packet.CreateBuffer();

        await _server.GetStream().WriteAsync(BitConverter.GetBytes(Convert.ToUInt32(requestPacketBuffer.Length)));
        await _server.GetStream().WriteAsync(requestPacketBuffer);
        await _server.GetStream().FlushAsync();
    }


    private async Task<CommunicationPacket> ReadFromServerAsync()
    {
        byte[] lengthBuffer = new byte[4];
        _ = await _server.GetStream().ReadAsync(lengthBuffer);

        UInt32 communicationPacketLength = BitConverter.ToUInt32(lengthBuffer);

        byte[] communicationPacketBuffer = new byte[communicationPacketLength];
        _ = await _server.GetStream().ReadAsync(communicationPacketBuffer);

        return CommunicationPacket.FromBuffer(communicationPacketBuffer);
    }

    #endregion


    #region Connection and Confiugration

    [Fact]
    public async Task TestConnectionBetweenServerAndClient_TestThatConnectionCanBeMade()
    {
        // Create 
        Memory<byte> testMessage = Encoding.UTF8.GetBytes("Test Message");

        // Arrange
        (TcpClient client, TcpClient server) connections = await CreateConnection(8889);

        NetworkStream clientStream = connections.client.GetStream();
        NetworkStream serverStream = connections.server.GetStream();

        // Act
        await serverStream.WriteAsync(testMessage);
        await serverStream.FlushAsync();

        Memory<byte> receiveBuffer = new Memory<byte>(new Byte[testMessage.Length]);

        int amount = await clientStream.ReadAsync(receiveBuffer);

        // Assert
        Assert.Equal(Encoding.UTF8.GetString(testMessage.Span), Encoding.UTF8.GetString(receiveBuffer.Span));
    }


    // Invalid Configuration Exception
    // File not found exception


    [Fact]
    public async Task ConnectRequestAsync_TestValidConnectionRequest_InvokesConnectionController()
    {
        // Create
        ConnectRequestT request = new ConnectRequestT();
        request.ConfigurationConcurrencyToken = "test Token";

        // Building the packet
        FlatBufferBuilder builder = new FlatBufferBuilder(512);
        builder.Finish(ConnectRequest.Pack(builder, request).Value);
        CommunicationPacket requestPacket = new CommunicationPacket(PacketIdentifier.ConnectRequest, builder.SizedByteArray());

        // Arrange
        _connectionControllerMock.Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(ConnectResult.Success));

        // Act
        await WriteFromServerAsync(requestPacket);

        // Act Response
        CommunicationPacket replyPacket = await ReadFromServerAsync();

        // Assert
        _connectionControllerMock.Verify(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(PacketIdentifier.ConnectReply, replyPacket.Identifier);
    }


    [Fact]
    public async Task ConnectionRequestAsync_TestCloseConnection_WhenConfigurationNotValidOrFound()
    {
        // Create
        ConnectRequestT request = new ConnectRequestT();
        request.ConfigurationConcurrencyToken = "test Token";

        // Building the packet
        FlatBufferBuilder builder = new FlatBufferBuilder(512);
        builder.Finish(ConnectRequest.Pack(builder, request).Value);
        CommunicationPacket requestPacket = new CommunicationPacket(PacketIdentifier.ConnectRequest, builder.SizedByteArray());

        // Arrange
        _connectionControllerMock.Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws<InvalidConfigurationException>();

        // Act
        await WriteFromServerAsync(requestPacket);

        // Act Response
        CommunicationPacket replyPacket = await ReadFromServerAsync();

        // Assert
        _connectionControllerMock.Verify(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(PacketIdentifier.ErrorReply, replyPacket.Identifier);
    }


    [Fact]
    public async Task SetConfigurationAsync_TestIfConfigurationChanges_IfValidConfigurationIsSend()
    {
        // Create
        SetConfigurationRequestT request = new SetConfigurationRequestT();

        request.ConcurrencyToken = "Test Token";

        request.Configuration = new ConfigurationMessageT
        {
            Ledstrips = new List<LedstripMessageT>
            {
                new LedstripMessageT
                {
                    BusId = 0,
                    LedstripId = Guid.NewGuid().ToString(),
                    Chip = LedstripChip.WS2812B,
                    PixelCount = 100
                }
            }
        };

        // Building the packet
        FlatBufferBuilder builder = new FlatBufferBuilder(1024);
        builder.Finish(SetConfigurationRequest.Pack(builder, request).Value);
        CommunicationPacket requestPacket = new CommunicationPacket(PacketIdentifier.SetConfigurationRequest, builder.SizedByteArray());

        // Arrange
        _connectionControllerMock.Setup(x => x.SetConfigurationAsync(It.IsAny<IEnumerable<Ledstrip>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));

        // Act
        await WriteFromServerAsync(requestPacket);

        // Act Response
        CommunicationPacket replyPacket = await ReadFromServerAsync();

        // Assert
        _connectionControllerMock.Verify(x => x.SetConfigurationAsync(It.IsAny<IEnumerable<Ledstrip>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(PacketIdentifier.SetConfigurationReply, replyPacket.Identifier);
    }


    [Fact]
    public async Task SetConfigurationAsync_TestIfApplicationExceptionIsHandled_WhenTheSetConfigurationHasAnException()
    {
        // Create
        SetConfigurationRequestT request = new SetConfigurationRequestT();

        request.ConcurrencyToken = "Test Token";

        request.Configuration = new ConfigurationMessageT
        {
            Ledstrips = new List<LedstripMessageT>
            {
                new LedstripMessageT
                {
                    BusId = 0,
                    LedstripId = Guid.NewGuid().ToString(),
                    Chip = LedstripChip.WS2812B,
                    PixelCount = 100
                }
            }
        };

        // Building the packet
        FlatBufferBuilder builder = new FlatBufferBuilder(1024);
        builder.Finish(SetConfigurationRequest.Pack(builder, request).Value);
        CommunicationPacket requestPacket = new CommunicationPacket(PacketIdentifier.SetConfigurationRequest, builder.SizedByteArray());

        // Arrange
        _connectionControllerMock.Setup(x => x.SetConfigurationAsync(It.IsAny<IEnumerable<Ledstrip>>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws<ApplicationException>();

        // Act
        await WriteFromServerAsync(requestPacket);

        // Act Response
        CommunicationPacket replyPacket = await ReadFromServerAsync();

        // Assert
        _connectionControllerMock.Verify(x => x.SetConfigurationAsync(It.IsAny<IEnumerable<Ledstrip>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(PacketIdentifier.ErrorReply, replyPacket.Identifier);
    }

    #endregion
}