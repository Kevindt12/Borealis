using System.Net;
using System.Net.Sockets;

using Borealis.Domain.Communication;
using Borealis.Domain.Communication.Messages;



namespace Borealis.Drivers.Rpi.Udp.Connections;


public class UdpConnectionServer : IDisposable
{
    private readonly UdpClient _client;

    private CancellationTokenSource? _stoppingToken;
    private Task? _runningTask;


    /// <summary>
    /// If the UDP Connection server is running.
    /// </summary>
    public bool IsRunning => _stoppingToken is not null && _runningTask is not null;

    public int Port { get; }


    public event EventHandler Connection;

    public event EventHandler Disconnection;

    public event EventHandler<FrameMessage> FrameReceived;


    /// <summary>
    /// The current server connection that we have.
    /// </summary>
    public IPEndPoint? CurrentConnection { get; set; }


    public UdpConnectionServer(int port)
    {
        Port = port;
        _client = new UdpClient(port);
    }


    /// <summary>
    /// Starts the client and listens on that port for packets,
    /// </summary>
    /// <param name="token"> </param>
    /// <returns> </returns>
    /// <exception cref="InvalidOperationException"> </exception>
    public async Task StartAsync(CancellationToken token = default)
    {
        // Making sure that we don't have race conditions.

        // Checking if we are still running.
        if (IsRunning)
        {
            throw new InvalidOperationException("The Udp client is already running.");
        }

        _stoppingToken = new CancellationTokenSource();

        _runningTask = Task.Factory.StartNew(async () =>
                                             {
                                                 while (!_stoppingToken.Token.IsCancellationRequested)
                                                 {
                                                     UdpReceiveResult result = await _client.ReceiveAsync(_stoppingToken.Token);

                                                     await HandleIncomingPacket(CommunicationPacket.FromBuffer(result.Buffer), result.RemoteEndPoint);
                                                 }
                                             },
                                             token,
                                             TaskCreationOptions.LongRunning,
                                             TaskScheduler.Default);
    }


    protected virtual async Task HandleIncomingPacket(CommunicationPacket packet, IPEndPoint remoteEndPoint)
    {
        await Task.Run(() => packet.Identifier switch
        {
            PacketIdentifier.KeepAlive  => HandleKeepAliveAsync(packet, remoteEndPoint),
            PacketIdentifier.Connect    => HandleIncomingConnection(packet, remoteEndPoint),
            PacketIdentifier.Disconnect => HandleIncomingDisconnection(packet, remoteEndPoint),
            PacketIdentifier.Frame      => HandleFrame(packet)
        });
    }


    protected virtual async Task HandleKeepAliveAsync(CommunicationPacket packet, IPEndPoint remoteEndPoint)
    {
        await _client.SendAsync(packet.GenerateAcknowledgementPacket().CreateBuffer(), remoteEndPoint);
    }


    protected virtual async Task HandleIncomingConnection(CommunicationPacket packet, IPEndPoint remoteEndPoint)
    {
        CurrentConnection = remoteEndPoint;
        Connection?.Invoke(this, EventArgs.Empty);

        await _client.SendAsync(packet.GenerateAcknowledgementPacket().CreateBuffer(), remoteEndPoint);
    }


    protected virtual async Task HandleIncomingDisconnection(CommunicationPacket packet, IPEndPoint remoteEndPoint)
    {
        Disconnection?.Invoke(this, EventArgs.Empty);

        await _client.SendAsync(packet.GenerateAcknowledgementPacket().CreateBuffer(), remoteEndPoint);
    }


    protected virtual Task HandleFrame(CommunicationPacket packet)
    {
        FrameReceived.Invoke(this, packet.ReadPayload<FrameMessage>());

        return Task.CompletedTask;
    }


    public async Task StopAsync(CancellationToken token = default)
    {
        if (!IsRunning)
        {
            // Cleaning up then throwing the exception.

            throw new InvalidOperationException("The Udp client is not running.");
        }

        _stoppingToken!.Cancel();
        await _runningTask!;

        _stoppingToken = null;
        _runningTask = null;
    }


    public async Task SendDisconnectionAsync()
    {
        // Sending that we are disconnecting
        if (CurrentConnection == null) return;
        await _client.SendAsync(CommunicationPacket.CreateDisconnectionPacket().CreateBuffer(), CurrentConnection);
    }


    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        _stoppingToken?.Dispose();
        _runningTask?.Dispose();
    }
}