using System.Net.Sockets;

using Borealis.Domain.Communication;
using Borealis.Domain.Communication.Messages;
using Borealis.Portal.Domain.Devices;
using Borealis.Portal.Domain.Exceptions;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Infrastructure.Connections;


internal class DeviceConnection : IDeviceConnection
{
    private readonly ILogger<DeviceConnection> _logger;
    private readonly UdpClient _udpClient;
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _stream;

    private bool _isConnected;

    /// <inheritdoc />
    public Device Device { get; }

    /// <summary>
    /// The timeout when waiting for response.
    /// </summary>
    public int Timeout { get; set; } = 10000;


    protected DeviceConnection(ILogger<DeviceConnection> logger, Device device)
    {
        if (device.ConnectionType != ConnectionType.Udp) throw new ApplicationException("Cannot create a Udp connection with a device that has been set to something else.");
        _logger = logger;

        Device = device;
        _udpClient = new UdpClient();
        _tcpClient = new TcpClient();
    }


    public static async Task<DeviceConnection> CreateConnectionAsync(ILogger<DeviceConnection> logger, Device device)
    {
        DeviceConnection connection = new DeviceConnection(logger, device);

        try
        {
            await connection.ConnectAsync();
        }
        catch (Exception e)
        {
            await connection.DisposeAsync();

            throw;
        }

        connection._isConnected = true;

        return connection;
    }


    /// <inheritdoc />
    public async ValueTask SendFrameAsync(FrameMessage frameMessage, CancellationToken token = default)
    {
        await SendUdpPacketAsync(CommunicationPacket.CreatePacketFromMessage(frameMessage), token);
    }


    /// <inheritdoc />
    public async Task SendConfirmedFrameAsync(FrameMessage frameMessage, CancellationToken token = default)
    {
        _logger.LogTrace($"Sending confirmed trace to device {Device.EndPoint}.");
        await SendTcpPacketAsync(CommunicationPacket.CreatePacketFromMessage(frameMessage), token);
    }


    /// <inheritdoc />
    public async Task SendConfigurationAsync(ConfigurationMessage configuration, CancellationToken token = default)
    {
        _logger.LogTrace($"Sending Configuration to device {Device.Id}.");
        await SendTcpPacketAsync(CommunicationPacket.CreatePacketFromMessage(configuration), token);
    }


    protected virtual async Task ConnectAsync(CancellationToken token = default)
    {
        try
        {
            // TCP
            _logger.LogTrace($"Connecting to device {Device.Id}.");
            await _tcpClient.ConnectAsync(Device.EndPoint, token);

            // UDP
            _udpClient.Connect(Device.EndPoint);
        }
        catch (SocketException e)
        {
            _logger.LogTrace(e, $"Socket exception when connecting to device {Device.Id}.");

            throw new DeviceConnectionException("Could not establish a connection with the device.", e, Device);
        }
    }


    protected virtual async ValueTask SendTcpPacketAsync(CommunicationPacket packet, CancellationToken token = default)
    {
        // Creating a time out.
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter(Timeout);

        try
        {
            _logger.LogTrace($"Sending TCP message to device {Device.Id}.");
            await _stream.WriteAsync(packet.CreateBuffer(), cts.Token);
            await _stream.FlushAsync(cts.Token);
        }
        catch (SocketException e)
        {
            throw new DeviceConnectionException("There was a problem with send a tcp packet.", e, Device);
        }
    }


    protected virtual async ValueTask SendUdpPacketAsync(CommunicationPacket packet, CancellationToken token = default)
    {
        _logger.LogTrace($"Sending Udp message to device {Device.Id}.");
        await _udpClient.SendAsync(packet.CreateBuffer(), CancellationToken.None);
    }


    private bool _disposed;


    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed) return;

        Dispose(true);
        GC.SuppressFinalize(this);
    }


    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tcpClient?.Dispose();
            _udpClient?.Dispose();
        }
    }


    protected virtual async ValueTask DisposeAsyncCore()
    {
        // This might happen when 
        if (_tcpClient.Connected)
        {
            await SendTcpPacketAsync(CommunicationPacket.CreateDisconnectionPacket());
        }

        _tcpClient?.Dispose();
        _udpClient?.Dispose();
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (!_disposed) return;

        await DisposeAsyncCore();

        Dispose(false);
        GC.SuppressFinalize(this);
    }
}