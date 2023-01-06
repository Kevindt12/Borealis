using System.Net;
using System.Net.Sockets;

using Borealis.Domain.Communication;
using Borealis.Domain.Communication.Messages;
using Borealis.Portal.Domain.Devices;
using Borealis.Portal.Domain.Exceptions;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Infrastructure.Connections;


internal class TcpDeviceConnection : DeviceConnectionBase
{
    private readonly ILogger<TcpDeviceConnection> _logger;
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _stream;


    private readonly CancellationTokenSource? _stoppingToken;
    private readonly Task? _runningTask;


    /// <summary>
    /// The timeout when waiting for response.
    /// </summary>
    public int Timeout { get; set; } = 10000;


    protected TcpDeviceConnection(ILogger<TcpDeviceConnection> logger, Device device, TcpClient tcpClient) : base(logger, device)
    {
        _logger = logger;

        _tcpClient = tcpClient;
        _stream = tcpClient.GetStream();

        _stoppingToken = new CancellationTokenSource();
        _runningTask = Task.Run(RunningTaskLoop);
    }


    public static async Task<TcpDeviceConnection> CreateConnectionAsync(ILogger<TcpDeviceConnection> logger, Device device, CancellationToken token = default)
    {
        // The clients.
        TcpClient tcpClient = new TcpClient();

        try
        {
            // TCP
            logger.LogTrace($"Connecting to device {device.Id} with tcp & udp connection on {device.EndPoint}.");
            await tcpClient.ConnectAsync(device.EndPoint, token);
        }
        catch (SocketException e)
        {
            // Cleaning up the clients if they fail.
            tcpClient.Dispose();

            throw new DeviceConnectionException($"Unable to create connection with device {device.Id}.", e, device);
        }
        catch (Exception e)
        {
            // Cleaning up the clients if they fail.
            tcpClient.Dispose();

            throw;
        }

        return new TcpDeviceConnection(logger, device, tcpClient);
    }


    /// <summary>
    /// The running task loop.
    /// </summary>
    /// <returns> </returns>
    private async Task RunningTaskLoop()
    {
        // Looping till we get data.
        while (!_stoppingToken!.Token.IsCancellationRequested)
        {
            if (_stream.DataAvailable)
            {
                try
                {
                    int counter = 0;

                    // waiting for 0xCC
                    while (_stream.ReadByte() != 0XCC)
                    {
                        counter++;

                        if (counter > 4)
                        {
                            // Have passed the header demimiters for sure so we will throw a exception.
                            throw new InvalidOperationException("Delimiter of TCP payload invalid.");
                        }
                    }

                    // Check if header is now on 0xDD
                    if (_stream.ReadByte() != 0xDD)
                    {
                        throw new InvalidOperationException("Header invalid.");
                    }

                    // Now we can be sure that we are on the begin of the packet and ready to read the length.
                    byte[] lengthBuffer = new byte[4];
                    _stream.Read(lengthBuffer, 0, 4);

                    uint length = BitConverter.ToUInt32(lengthBuffer);

                    // Creating the buffer and reading.
                    Memory<byte> buffer = new Memory<Byte>(new Byte[length]);

                    uint bytesRead = 0;

                    while (bytesRead < length)
                    {
                        bytesRead = +Convert.ToUInt32(await _stream.ReadAsync(buffer).ConfigureAwait(false));
                    }

                    // Decoing the packet.
                    CommunicationPacket packet = CommunicationPacket.FromBuffer(buffer);
                    await HandleIncomingPacket(packet).ConfigureAwait(false);
                }
                catch (SocketException socketException)
                {
                    _logger.LogError(socketException, "Socket exception.");
                }
                catch (IOException e)
                {
                    _logger.LogError(e, "Error with reading data from the tcp server.");
                }
            }

            // Adding a 16 ms delay. It should then run at 60 FPS about that. If we need faster use UDP.
            await Task.Delay(16);
        }

        _logger.LogTrace($"Stop listening to client : {Device.EndPoint}.");
    }


    /// <summary>
    /// Handles a incoming packet.
    /// </summary>
    /// <param name="packet"> The <see cref="CommunicationPacket" /> that we received. </param>
    /// <param name="remoteEndPoint"> The <see cref="IPEndPoint" /> from the remote device. </param>
    protected virtual async Task HandleIncomingPacket(CommunicationPacket packet)
    {
        await Task.Run(() => packet.Identifier switch
                   {
                       PacketIdentifier.KeepAlive           => HandleKeepAliveAsync(packet),
                       PacketIdentifier.Disconnect          => HandleDisconnectAsync(packet),
                       PacketIdentifier.FramesBufferRequest => HandleFrameBufferRequest(packet),
                       _                                    => HandleUnknownPacketAsync(packet)
                   })
                  .ConfigureAwait(false);
    }


    private Task HandleUnknownPacketAsync(CommunicationPacket packet)
    {
        return Task.CompletedTask;
    }


    private Task HandleFrameBufferRequest(CommunicationPacket packet)
    {
        FrameBufferRequestMessage message = packet.ReadPayload<FrameBufferRequestMessage>()!;

        LedstripConnections[message.LedstripIndex].InvokeRequestForFFrames(message.NumberOfFrames);

        return Task.CompletedTask;
    }


    private Task HandleDisconnectAsync(CommunicationPacket packet)
    {
        _logger.LogError("Device is disconnecting.");

        return Task.CompletedTask;
    }


    private Task HandleKeepAliveAsync(CommunicationPacket packet)
    {
        _logger.LogTrace("KeepAlive received;");

        return Task.CompletedTask;
    }


    /// <inheritdoc />
    internal override async Task SendPacketAsync(CommunicationPacket packet, CancellationToken token = default)
    {
        try
        {
            _logger.LogTrace($"Sending TCP message to device {Device.Id}.");
            ReadOnlyMemory<byte> packetBuffer = packet.CreateBuffer();
            await _stream.WriteAsync(new Byte[] { 0xAA, 0xBB, 0xCC, 0xDD }, token);
            await _stream.WriteAsync(BitConverter.GetBytes(Convert.ToUInt32(packetBuffer.Length)), token);
            await _stream.WriteAsync(packetBuffer, token);

            await _stream.FlushAsync(token);
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
        }
    }


    protected override async ValueTask DisposeAsyncCore()
    {
        await SendPacketAsync(CommunicationPacket.CreateDisconnectionPacket());

        _tcpClient?.Dispose();
    }
}