using System.Net;
using System.Net.Sockets;

using Borealis.Domain.Communication;
using Borealis.Domain.Communication.Messages;



namespace Borealis.Drivers.Rpi.Udp.Connections;


/**
 * TODO: Add Keep alive to make sure that both connection maintain a connection.
 * TODO:
 */
public class PortalConnection : IDisposable, IAsyncDisposable
{
    private readonly ILogger<PortalConnection> _logger;
    private readonly TcpClient _client;

    private readonly NetworkStream _stream;

    private readonly CancellationTokenSource? _stoppingToken;
    private readonly Task? _runningTask;

    /// <summary>
    /// When a frame has a been received.
    /// </summary>
    public event EventHandler<FrameMessage>? FrameReceived;


    /// <summary>
    /// When a frame has a been received.
    /// </summary>
    public event EventHandler<FramesBufferMessage>? FrameBufferReceived;

    /// <summary>
    /// Start a animation on a ledstrip.
    /// </summary>
    public event EventHandler<StartAnimationMessage>? StartAnimationReceived;

    /// <summary>
    /// Start a animation on ledstrip.
    /// </summary>
    public event EventHandler<StopAnimationMessage>? StopAnimationReceived;


    /// <summary>
    /// When a disconnection has been requested.
    /// </summary>
    public event EventHandler? Disconnect;

    /// <summary>
    /// When a configuration has been received.
    /// </summary>
    public event EventHandler<ConfigurationMessage>? ConfigurationReceived;

    /// <summary>
    /// The remote endpoint of the server we are now connected with.
    /// </summary>
    public virtual EndPoint RemoteEndPoint { get; init; }


    public PortalConnection(ILogger<PortalConnection> logger, TcpClient client)
    {
        _logger = logger;
        _stream = client.GetStream();
        _client = client;

        RemoteEndPoint = client.Client.RemoteEndPoint!;

        // Starting the listening task.\
        _stoppingToken = new CancellationTokenSource();
        _runningTask = Task.Run(RunningTaskLoop);
    }


    /// <summary>
    /// The running task loop.
    /// </summary>
    /// <returns> </returns>
    private async Task RunningTaskLoop()
    {
        _logger.LogTrace($"Start listening for packets from client : {_client.Client.RemoteEndPoint}.");

        // Looping till we get data.
        while (!_stoppingToken!.Token.IsCancellationRequested)
        {
            if (_stream.DataAvailable)
            {
                try
                {
                    int counter = 0;

                    // waiting for 0xCC
                    while (_stream.ReadByte() != 0xCC)
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

                    await Task.Delay(30);

                    // Creating the buffer and reading.
                    Memory<byte> buffer = new Memory<Byte>(new Byte[length]);

                    int bytesRead = await _stream.ReadAsync(buffer).ConfigureAwait(false);

                    // Decoing the packet.
                    CommunicationPacket packet = CommunicationPacket.FromBuffer(buffer);
                    await HandleIncomingPacket(packet).ConfigureAwait(false);
                }
                catch (SocketException socketException)
                {
                    _logger.LogError(socketException, "Socket exception.");
                    Disconnect?.Invoke(this, EventArgs.Empty);
                }
                catch (IOException e)
                {
                    _logger.LogError(e, "Error with reading data from the tcp server.");
                }
            }

            // Adding a 16 ms delay. It should then run at 60 FPS about that. If we need faster use UDP.
            await Task.Delay(10);
        }

        _logger.LogTrace($"Stop listening to client : {_client.Client.RemoteEndPoint}.");
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
                       PacketIdentifier.KeepAlive      => HandleKeepAliveAsync(packet),
                       PacketIdentifier.Disconnect     => HandleDisconnectAsync(packet),
                       PacketIdentifier.StartAnimation => HandleStartAnimationAsync(packet),
                       PacketIdentifier.StopAnimation  => HandleStopAnimationAsync(packet),
                       PacketIdentifier.Frame          => HandleFrameAsync(packet),
                       PacketIdentifier.FramesBuffer   => HandleFrameBufferAsync(packet),
                       PacketIdentifier.Configuration  => HandleConfiguration(packet),
                       _                               => HandleUnknownPacketAsync(packet)
                   })
                  .ConfigureAwait(false);
    }


    protected virtual Task HandleStopAnimationAsync(CommunicationPacket packet)
    {
        _logger.LogTrace("incoming packet request to stop animation.");

        StopAnimationMessage message = packet.ReadPayload<StopAnimationMessage>()!;

        StopAnimationReceived?.Invoke(this, message);

        return Task.CompletedTask;
    }


    protected virtual async Task HandleKeepAliveAsync(CommunicationPacket packet)
    {
        _logger.LogTrace("incoming packet keep alive.");

        await SendAsync(CommunicationPacket.CreateKeepAlive());
    }


    protected virtual Task HandleFrameAsync(CommunicationPacket packet)
    {
        _logger.LogTrace("incoming packet frames.");

        FrameMessage message = packet.ReadPayload<FrameMessage>()!;

        FrameReceived?.Invoke(this, message);

        return Task.CompletedTask;
    }


    protected virtual Task HandleStartAnimationAsync(CommunicationPacket packet)
    {
        _logger.LogTrace("incoming packet request to start animation.");

        StartAnimationMessage animationMessage = packet.ReadPayload<StartAnimationMessage>()!;

        StartAnimationReceived?.Invoke(this, animationMessage);

        return Task.CompletedTask;
    }


    /// <summary>
    /// Handle a frame message.
    /// </summary>
    /// <param name="packet"> The frame packet. </param>
    /// <returns> </returns>
    protected virtual Task HandleFrameBufferAsync(CommunicationPacket packet)
    {
        FramesBufferMessage bufferMessage = packet.ReadPayload<FramesBufferMessage>()!;

        FrameBufferReceived?.Invoke(this, bufferMessage);

        return Task.CompletedTask;
    }


    /// <summary>
    /// Handle a keep alive message.
    /// </summary>
    /// <param name="packet"> </param>
    /// <param name="remoteEndPoint"> </param>
    /// <returns> </returns>
    protected virtual async Task HandleDisconnectAsync(CommunicationPacket packet)
    {
        // Announcing that we are disconnecting.
        _logger.LogTrace("incoming packet to request to disconnect.");
        Disconnect?.Invoke(this, EventArgs.Empty);

        // Disposing of the connection.
        await DisposeAsync();
    }


    /// <summary>
    /// Handle a keep alive message.
    /// </summary>
    /// <param name="packet"> </param>
    /// <param name="remoteEndPoint"> </param>
    /// <returns> </returns>
    protected virtual Task HandleConfiguration(CommunicationPacket packet)
    {
        ConfigurationReceived?.Invoke(this, ConfigurationMessage.FromBuffer(packet.Payload!.Value));

        return Task.CompletedTask;
    }


    /// <summary>
    /// A unknown packet that has been received.
    /// </summary>
    /// <param name="packet"> </param>
    /// <param name="remoteEndPoint"> </param>
    /// <returns> </returns>
    protected virtual async Task HandleUnknownPacketAsync(CommunicationPacket packet)
    {
        // _logger.LogError($"Unknown packet received from {remoteEndPoint}");
        await SendAsync(CommunicationPacket.CreatePacketFromMessage(new ErrorMessage("Unknown communication packet.")));
    }


    /// <summary>
    /// Sends back to the client that we have received the packet that was send.
    /// </summary>
    /// <param name="packet"> The packet that we received. We wil transfer this into a packet to send back. </param>
    public virtual async Task SendAsync(CommunicationPacket packet)
    {
        ReadOnlyMemory<byte> packetBuffer = packet.CreateBuffer();
        await _stream.WriteAsync(new Byte[] { 0xAA, 0xBB, 0xCC, 0xDD });
        await _stream.WriteAsync(BitConverter.GetBytes(Convert.ToUInt32(packetBuffer.Length)));
        await _stream.WriteAsync(packetBuffer);

        await _stream.FlushAsync();
    }


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
            if (_client.Connected)
            {
                _client.Dispose();
            }
        }
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        await DisposeAsyncCore();

        Dispose(false);
        GC.SuppressFinalize(this);

        _disposed = true;
    }


    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_stream.Socket.Connected)
        {
            try
            {
                await _client.Client.DisconnectAsync(true);
            }
            catch (Exception e)
            {
                // Ignore don't really care if it does not work.
                _logger.LogWarning(e, "Exception caught while disposing of the portal connection, exception ignored since we are disposing of it.");
            }
            finally
            {
                _client.Dispose();
            }
        }
    }
}