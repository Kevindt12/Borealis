using System.Net;
using System.Net.Sockets;

using Borealis.Domain.Communication;
using Borealis.Portal.Domain.Devices;
using Borealis.Portal.Domain.Exceptions;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Infrastructure.Connections;


internal class CombinedDeviceConnection : DeviceConnectionBase
{
    private readonly ILogger<CombinedDeviceConnection> _logger;
    private readonly UdpClient _udpClient;
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _stream;


    /// <summary>
    /// The timeout when waiting for response.
    /// </summary>
    public int Timeout { get; set; } = 10000;


    protected CombinedDeviceConnection(ILogger<CombinedDeviceConnection> logger, Device device, TcpClient tcpClient, UdpClient udpClient) : base(logger, device)
    {
        _logger = logger;

        _udpClient = udpClient;
        _tcpClient = tcpClient;
        _stream = tcpClient.GetStream();
    }


    public static async Task<CombinedDeviceConnection> CreateConnectionAsync(ILogger<CombinedDeviceConnection> logger, Device device, CancellationToken token = default)
    {
        // The clients.
        TcpClient tcpClient = new TcpClient();
        UdpClient udpClient = new UdpClient();

        try
        {
            // TCP
            logger.LogTrace($"Connecting to device {device.Id} with tcp & udp connection on {device.EndPoint}.");
            await tcpClient.ConnectAsync(device.EndPoint, token);

            // UDP
            IPEndPoint udpEndPoint = new IPEndPoint(device.EndPoint.Address, device.EndPoint.Port + 1);
            udpClient.Connect(udpEndPoint);
        }
        catch (SocketException e)
        {
            // Cleaning up the clients if they fail.
            tcpClient.Dispose();
            udpClient.Dispose();

            throw new DeviceConnectionException($"Unable to create connection with device {device.Id}.", e, device);
        }
        catch (Exception e)
        {
            // Cleaning up the clients if they fail.
            tcpClient.Dispose();
            udpClient.Dispose();

            throw;
        }

        return new CombinedDeviceConnection(logger, device, tcpClient, udpClient);
    }


    /// <inheritdoc />
    internal override async ValueTask SendUnconfirmedPacketAsync(CommunicationPacket packet)
    {
        _logger.LogTrace($"Sending Udp message to device {Device.Id}.");
        await _udpClient.SendAsync(packet.CreateBuffer(), CancellationToken.None);
    }


    /// <inheritdoc />
    internal override async Task<CommunicationPacket?> SendConfirmedPacketAsync(CommunicationPacket packet, CancellationToken token = default)
    {
        // Creating a time out.

        try
        {
            _logger.LogTrace($"Sending TCP message to device {Device.Id}.");
            await _stream.WriteAsync(packet.CreateBuffer(), token);
            await _stream.FlushAsync(token);

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(Timeout);

            try
            {
                byte[] buffer = new Byte[512];
                int bytesRead = await _stream.ReadAsync(buffer, cts.Token);

                Array.Resize(ref buffer, bytesRead);

                return CommunicationPacket.FromBuffer(buffer);
            }
            catch (OperationCanceledException e)
            {
                _logger.LogTrace("Device connection timed out.");

                throw new DeviceConnectionException("The device connection timed out.");
            }
        }
        catch (SocketException e)
        {
            throw new DeviceConnectionException("There was a problem with send a tcp packet.", e, Device);
        }
    }


    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tcpClient?.Dispose();
            _udpClient?.Dispose();
        }
    }


    protected override async ValueTask DisposeAsyncCore()
    {
        await SendConfirmedPacketAsync(CommunicationPacket.CreateDisconnectionPacket());

        _tcpClient?.Dispose();
        _udpClient?.Dispose();
    }
}