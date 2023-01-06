using Borealis.Domain.Effects;
using Borealis.Drivers.Rpi.Udp.Ledstrips;
using Borealis.Shared.Extensions;

using UnitsNet;



namespace Borealis.Drivers.Rpi.Udp.Handlers;


public delegate Task RequestFramesForAnimation(int amount);



public class AnimationPlayerHandler
{
    private readonly Stack<ReadOnlyMemory<PixelColor>> _frameBuffer;

    private CancellationTokenSource? _stoppingToken;
    private Task? _runningTask;


    private bool _requestInProgress;


    private readonly RequestFramesForAnimation _requestFramesForAnimationCallback;


    /// <summary>
    /// The ledstrip that we want to play the animation on.
    /// </summary>
    public LedstripProxyBase Ledstrip { get; }


    /// <summary>
    /// The Threshold that needs to be passed before the request is send to get more frame buffers.
    /// </summary>
    public double FrameBufferRequestThreshold { get; set; } = 0.8;

    /// <summary>
    /// The stack size of the frame buffer.
    /// </summary>
    public int StackSize { get; set; } = 400;


    /// <summary>
    /// The frequency we want to play the animation at.
    /// </summary>
    public Frequency Frequency { get; set; }


    public AnimationPlayerHandler(LedstripProxyBase ledstrip, RequestFramesForAnimation requestForAnimationCallback)
    {
        _requestFramesForAnimationCallback = requestForAnimationCallback;
        Ledstrip = ledstrip;

        _frameBuffer = new Stack<ReadOnlyMemory<PixelColor>>();
    }


    /// <summary>
    /// Starts a new animation of the ledstrip.
    /// </summary>
    /// <param name="frequency"> </param>
    /// <param name="cancellationToken"> </param>
    /// <returns> </returns>
    public async Task StartAsync(Frequency frequency, CancellationToken cancellationToken = default)
    {
        _stoppingToken = new CancellationTokenSource();
        _runningTask = Task.Run(RunningTaskLoop);

        Frequency = frequency;
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
            ReadOnlyMemory<PixelColor> frame = _frameBuffer.Pop();

            Ledstrip.SetColors(frame);

            CheckStackBuffer();

            // Adding a 16 ms delay. It should then run at 60 FPS about that. If we need faster use UDP.
            await Task.Delay((int)(1000 / Frequency.Hertz));
        }
    }


    private void CheckStackBuffer()
    {
        if (_requestInProgress == false && _frameBuffer.Count < StackSize * FrameBufferRequestThreshold)
        {
            _requestInProgress = true;

            Task.Run(async () => await _requestFramesForAnimationCallback.Invoke(StackSize - _frameBuffer.Count));
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


    public void AddStackBuffer(IEnumerable<ReadOnlyMemory<PixelColor>> frames)
    {
        _frameBuffer.PushRange(frames);
        _requestInProgress = false;
    }
}



public class FramesRequestEventArgs : EventArgs
{
    public int Amount { get; set; }


    /// <inheritdoc />
    public FramesRequestEventArgs(Int32 amount)
    {
        Amount = amount;
    }
}