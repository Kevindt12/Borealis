using System.Net;
using System.Net.Sockets;

using Borealis.Domain.Communication;
using Borealis.Domain.Communication.Messages;



namespace Borealis.Drivers.Rpi.Udp.Connections;


public class TcpClientConnection : IDisposable, IAsyncDisposable
{
    private readonly ILogger<TcpClientConnection> _logger;
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;

    private readonly CancellationTokenSource? _stoppingToken;
    private readonly Task? _runningTask;

    /// <summary>
    /// If the UDP Connection server is running.
    /// </summary>
    public bool IsRunning => _stoppingToken is not null && _runningTask is not null;


    /// <summary>
    /// When a frame has a been received.
    /// </summary>
    public event EventHandler<FrameMessage>? FrameReceived;

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


    public TcpClientConnection(ILogger<TcpClientConnection> logger, TcpClient client)
    {
        _logger = logger;
        _stream = client.GetStream();
        _client = client;

        RemoteEndPoint = client.Client.RemoteEndPoint!;

        // Starting the listening task.\
        _stoppingToken = new CancellationTokenSource();
        _runningTask = Task.Factory.StartNew(RunningTaskLoop, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Current);
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
                    // Creating the buffer and reading.
                    Memory<byte> buffer = new Memory<Byte>();

                    int bytesRead = await _stream.ReadAsync(buffer);

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
            await Task.Delay(16);
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
                       PacketIdentifier.KeepAlive     => HandleKeepAliveAsync(packet),
                       PacketIdentifier.Frame         => HandleFrame(packet),
                       PacketIdentifier.Disconnect    => HandleDisconnect(packet),
                       PacketIdentifier.Configuration => HandleConfiguration(packet),
                       _                              => HandleUnknownPacketAsync(packet)
                   })
                  .ConfigureAwait(false);
    }


    /// <summary>
    /// Handle a keep alive message.
    /// </summary>
    /// <returns> </returns>
    protected virtual async Task HandleKeepAliveAsync(CommunicationPacket packet)
    {
        await SendAcknowledgment(CommunicationPacket.CreateKeepAlive());
    }


    /// <summary>
    /// Handle a keep alive message.
    /// </summary>
    /// <param name="packet"> </param>
    /// <param name="remoteEndPoint"> </param>
    /// <returns> </returns>
    protected virtual async Task HandleDisconnect(CommunicationPacket packet)
    {
        // Announcing that we are disconnecting.
        _logger.LogTrace("incoming packet to request to disconnect.");
        Disconnect?.Invoke(this, EventArgs.Empty);

        // Sending back that we got the packet.
        await SendAcknowledgment(packet).ConfigureAwait(false);

        // Disposing of the connection.
        await DisposeAsync();
    }


    /// <summary>
    /// Handle a keep alive message.
    /// </summary>
    /// <param name="packet"> </param>
    /// <param name="remoteEndPoint"> </param>
    /// <returns> </returns>
    protected virtual async Task HandleConfiguration(CommunicationPacket packet)
    {
        ConfigurationReceived?.Invoke(this, ConfigurationMessage.FromBuffer(packet.Payload!.Value));
        await SendAcknowledgment(packet).ConfigureAwait(false);
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
        await SendAcknowledgment(packet).ConfigureAwait(false);
    }


    /// <summary>
    /// Handle a frame message.
    /// </summary>
    /// <param name="packet"> The frame packet. </param>
    /// <returns> </returns>
    protected virtual async Task HandleFrame(CommunicationPacket packet)
    {
        FrameReceived?.Invoke(this, packet.ReadPayload<FrameMessage>());
        await SendAcknowledgment(packet).ConfigureAwait(false);
    }


    /// <summary>
    /// Sends back to the client that we have received the packet that was send.
    /// </summary>
    /// <param name="packet"> The packet that we received. We wil transfer this into a packet to send back. </param>
    protected virtual async Task SendAcknowledgment(CommunicationPacket packet)
    {
        await _stream.WriteAsync(packet.GenerateAcknowledgementPacket().CreateBuffer()).ConfigureAwait(false);
        await _stream.FlushAsync().ConfigureAwait(false);
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
            catch (Exception)
            {
                // Ignore don't really care if it does not work.
            }
            finally
            {
                _client.Dispose();
            }
        }
    }
}