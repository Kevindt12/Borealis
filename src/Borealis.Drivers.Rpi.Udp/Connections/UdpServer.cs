using System.Net;
using System.Net.Sockets;

using Borealis.Domain.Communication;
using Borealis.Domain.Communication.Messages;



namespace Borealis.Drivers.Rpi.Udp.Connections;


public class UdpServer : IDisposable, IAsyncDisposable
{
    private readonly ILogger<UdpServer> _logger;
    private readonly UdpClient _client;

    private CancellationTokenSource? _stoppingToken;
    private Task? _runningTask;


    /// <summary>
    /// If the UDP Connection server is running.
    /// </summary>
    public bool IsRunning => _stoppingToken is not null && _runningTask is not null;

    /// <summary>
    /// The port that we are listening on.
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// When a frame has a been received.
    /// </summary>
    public event EventHandler<FrameMessage>? FrameReceived;


    public UdpServer(ILogger<UdpServer> logger, int port)
    {
        _logger = logger;
        Port = port;
        _client = new UdpClient(port);
    }


    /// <summary>
    /// Starts the client and listens on that port for packets,
    /// </summary>
    /// <param name="token"> </param>
    /// <returns> </returns>
    /// <exception cref="InvalidOperationException"> </exception>
    public Task StartAsync(CancellationToken token = default)
    {
        // Checking if we are still running.
        if (IsRunning)
        {
            throw new InvalidOperationException("The Udp client is already running.");
        }

        _logger.LogDebug($"Starting udp server. Udp server on : {Port}.");

        _stoppingToken = new CancellationTokenSource();

        _runningTask = Task.Factory.StartNew(RunningTaskLoop,
                                             token,
                                             TaskCreationOptions.LongRunning,
                                             TaskScheduler.Default);

        _logger.LogDebug("Task started and running listening on the port for packet.");

        return Task.CompletedTask;
    }


    /// <summary>
    /// The running task loop.
    /// </summary>
    /// <returns> </returns>
    private async Task RunningTaskLoop()
    {
        // Loop until we want to stop.
        while (!_stoppingToken!.Token.IsCancellationRequested)
        {
            // Waiting for a packet to be received.
            UdpReceiveResult result = await _client.ReceiveAsync(_stoppingToken.Token).ConfigureAwait(false);

            await HandleIncomingPacket(CommunicationPacket.FromBuffer(result.Buffer), result.RemoteEndPoint).ConfigureAwait(false);
        }
    }


    /// <summary>
    /// Handles a incoming packet.
    /// </summary>
    /// <param name="packet"> The <see cref="CommunicationPacket" /> that we received. </param>
    /// <param name="remoteEndPoint"> The <see cref="IPEndPoint" /> from the remote device. </param>
    protected virtual async Task HandleIncomingPacket(CommunicationPacket packet, IPEndPoint remoteEndPoint)
    {
        try
        {
            _ = Task.Run(() => packet.Identifier switch
            {
                PacketIdentifier.KeepAlive => HandleKeepAliveAsync(packet, remoteEndPoint),
                PacketIdentifier.Frame     => HandleFrame(packet),
                _                          => HandleUnknownPacketAsync(packet, remoteEndPoint)
            });
        }

        catch (IOException ioException)
        {
            _logger.LogError(ioException, "Problem with handling the packet that was received on the udp server.");
        }
    }


    /// <summary>
    /// Handle a keep alive message.
    /// </summary>
    /// <param name="packet"> </param>
    /// <param name="remoteEndPoint"> </param>
    /// <returns> </returns>
    protected virtual async Task HandleKeepAliveAsync(CommunicationPacket packet, IPEndPoint remoteEndPoint)
    {
        await _client.SendAsync(packet.GenerateAcknowledgementPacket().CreateBuffer(), remoteEndPoint).ConfigureAwait(false);
    }


    /// <summary>
    /// Handle a frame message.
    /// </summary>
    /// <param name="packet"> The frame packet. </param>
    /// <returns> </returns>
    protected virtual Task HandleFrame(CommunicationPacket packet)
    {
        FrameReceived?.Invoke(this, packet.ReadPayload<FrameMessage>()!);

        return Task.CompletedTask;
    }


    /// <summary>
    /// A unknown packet that has been received.
    /// </summary>
    /// <param name="packet"> </param>
    /// <param name="remoteEndPoint"> </param>
    /// <returns> </returns>
    protected virtual async Task HandleUnknownPacketAsync(CommunicationPacket packet, IPEndPoint remoteEndPoint)
    {
        _logger.LogError($"Unknown packet received from {remoteEndPoint}");
        await _client.SendAsync(packet.GenerateAcknowledgementPacket().CreateBuffer(), remoteEndPoint).ConfigureAwait(false);
    }


    /// <summary>
    /// Stops the udp server.
    /// </summary>
    /// <param name="token"> </param>
    /// <returns> </returns>
    /// <exception cref="InvalidOperationException"> </exception>
    public async Task StopAsync(CancellationToken token = default)
    {
        if (!IsRunning)
        {
            throw new InvalidOperationException("The Udp client is not running.");
        }

        _stoppingToken!.Cancel();
        await _runningTask!;

        _stoppingToken = null;
        _runningTask = null;
    }


    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        _stoppingToken?.Dispose();
        _runningTask?.Dispose();
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (IsRunning)
        {
            try
            {
                await _runningTask!.ConfigureAwait(false);
            }
            catch (Exception) { }
        }
    }
}