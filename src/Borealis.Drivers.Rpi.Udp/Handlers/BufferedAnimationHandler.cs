using System.Net.Sockets;

using Borealis.Domain.Effects;
using Borealis.Drivers.Rpi.Udp.Buffering;
using Borealis.Drivers.Rpi.Udp.Connections;
using Borealis.Drivers.Rpi.Udp.Contexts;
using Borealis.Drivers.Rpi.Udp.Ledstrips;



namespace Borealis.Drivers.Rpi.Udp.Handlers;


public class BufferedAnimationHandler
{
    private readonly LedstripProxyBase _ledstrip;
    private readonly ILogger<TcpClientConnection> _logger;

    private readonly TcpClient _tcpClient;
    private readonly LedstripContext _ledstripContext;


    public FrameBuffer FrameBuffer { get; set; } = new FrameBuffer(500);

    private CancellationTokenSource? _stoppingToken;
    private Task? _runningTask;

    private int _delayTime;


    public BufferedAnimationHandler(LedstripProxyBase ledstrip)
    {
        _ledstrip = ledstrip;
    }


    public async Task StartAsync(int delayTime, CancellationToken cancellationToken = default)
    {
        _stoppingToken = new CancellationTokenSource();
        _runningTask = Task.Run(RunningTaskLoop);

        _delayTime = delayTime;
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
            ReadOnlyMemory<PixelColor> frame = FrameBuffer.Pop();

            _ledstrip.SetColors(frame);

            // Adding a 16 ms delay. It should then run at 60 FPS about that. If we need faster use UDP.
            await Task.Delay(_delayTime);
        }
    }


    public async Task StopAsync()
    {
        _stoppingToken?.Cancel();

        if (_runningTask != null)
        {
            await _runningTask.ConfigureAwait(false);
        }
    }
}