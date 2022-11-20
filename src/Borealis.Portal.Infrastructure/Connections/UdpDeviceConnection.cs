using System.Net.Sockets;

using Borealis.Domain.Communication;
using Borealis.Domain.Communication.Messages;
using Borealis.Portal.Domain.Devices;
using Borealis.Portal.Domain.Exceptions;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Infrastructure.Connections;


internal sealed class UdpDeviceConnection : IDeviceConnection
{
    private readonly ILogger<UdpDeviceConnection> _logger;
    private readonly UdpClient _udpClient;

    private bool _isConnected;


    /// <inheritdoc />
    public Device Device { get; }

    /// <summary>
    /// The timeout when waiting for response.
    /// </summary>
    public int Timeout { get; set; } = 10000;


    protected UdpDeviceConnection(ILogger<UdpDeviceConnection> logger, Device device)
    {
        if (device.ConnectionType != ConnectionType.Udp) throw new ApplicationException("Cannot create a Udp connection with a device that has been set to something else.");
        _logger = logger;

        Device = device;

        _udpClient = new UdpClient();
        _udpClient.Connect(device.EndPoint);
    }


    public static async Task<UdpDeviceConnection> CreateConnectionAsync(ILogger<UdpDeviceConnection> logger, Device device)
    {
        UdpDeviceConnection connection = new UdpDeviceConnection(logger, device);

        try
        {
            await connection.SendPacketAsync(CommunicationPacket.CreateConnectionPacket());
            connection._isConnected = true;
        }
        catch (TimeoutException e)
        {
            throw new DeviceConnectionException("The device was not able to connect.", e, device);
        }

        return connection;
    }


    /// <inheritdoc />
    public async ValueTask SendFrameAsync(FrameMessage frameMessage)
    {
        await _udpClient.SendAsync(CommunicationPacket.CreatePacketFromMessage(frameMessage).CreateBuffer());
    }


    private async Task SendPacketAsync(CommunicationPacket packet)
    {
        CancellationTokenSource cts = new CancellationTokenSource();

        cts.CancelAfter(Timeout);

        await _udpClient.SendAsync(packet.CreateBuffer(), CancellationToken.None);

        // Timeout if it takes to long
        try
        {
            UdpReceiveResult result = await _udpClient.ReceiveAsync(cts.Token);

            CommunicationPacket receivePacket = CommunicationPacket.FromBuffer(result.Buffer);

            if (!receivePacket.IsAcknowledgement)
            {
                _logger.LogWarning("Did not receive a ack message.");
            }
        }
        catch (OperationCanceledException oce)
        {
            _logger.LogWarning(oce, "Timeout reached on udp connection.");

            throw new TimeoutException("The Udp connection timed out.", oce);
        }
    }


    /// <inheritdoc />
    public void Dispose()
    {
        _udpClient?.Dispose();
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await SendPacketAsync(CommunicationPacket.CreateDisconnectionPacket());
    }
}