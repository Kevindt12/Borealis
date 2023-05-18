using System;
using System.Collections.Concurrent;
using System.Linq;

using Borealis.Drivers.Rpi.Commands;
using Borealis.Drivers.Rpi.Commands.Actions;
using Borealis.Drivers.Rpi.Exceptions;
using Borealis.Drivers.Rpi.Ledstrips;
using Borealis.Drivers.Rpi.Options;

using Microsoft.Extensions.Options;

using UnitsNet;



namespace Borealis.Drivers.Rpi.Animations;


public class AnimationPlayer
{
    private readonly ILogger<AnimationPlayer> _logger;
    private readonly IQueryHandler<RequestFrameBufferCommand, FrameBufferQuery> _requestFramesHandler;
    private readonly AnimationOptions _animationOptions;

    private readonly ConcurrentStack<ReadOnlyMemory<PixelColor>> _frameBuffer;

    private CancellationTokenSource? _stoppingToken;
    private Thread? _runningThread;

    private bool _requestInProgress;

    /// <summary>
    /// The ledstrip that we want to play the animation on.
    /// </summary>
    public LedstripProxyBase Ledstrip { get; }


    /// <summary>
    /// The Threshold that needs to be passed before the request is send to get more frame buffers.
    /// </summary>
    public double FrameBufferRequestThreshold => _animationOptions.FrameBufferRequestThreshold;

    /// <summary>
    /// The stack size of the frame buffer.
    /// </summary>
    public int StackSize => _animationOptions.TargetStackSize;


    /// <summary>
    /// The frequency we want to play the animation at.
    /// </summary>
    public Frequency Frequency { get; set; }

    public bool IsRunning => _stoppingToken != null;


    public AnimationPlayer(ILogger<AnimationPlayer> logger,
                           IQueryHandler<RequestFrameBufferCommand, FrameBufferQuery> requestFramesHandler,
                           IOptions<AnimationOptions> animationOptions,
                           LedstripProxyBase ledstrip
    )
    {
        _logger = logger;
        _requestFramesHandler = requestFramesHandler;
        _animationOptions = animationOptions.Value;
        Ledstrip = ledstrip;

        // Initializing the stack buffer.
        _frameBuffer = new ConcurrentStack<ReadOnlyMemory<PixelColor>>();
    }


    /// <summary>
    /// Starts a new animation of the ledstrip.
    /// </summary>
    /// <param name="frequency"> The frequency at which we want to play the animation at. </param>
    /// <param name="initialFrameBuffer">
    /// The <see cref="ReadOnlyMemory{T}" /> of
    /// <see cref="PixelColor" /> the frames that we want to start with.
    /// </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <exception cref="InvalidOperationException"> Thrown when the animation has already started. </exception>
    /// <exception cref="OperationCanceledException"> When the token has requested to stop the current operation. </exception>
    public virtual Task StartAsync(Frequency frequency, ReadOnlyMemory<PixelColor>[] initialFrameBuffer, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        if (IsRunning) throw new InvalidOperationException("The animation has already started.");

        Frequency = frequency;

        _logger.LogDebug($"Clearing the stack buffer and initializing the new stack buffer, with {initialFrameBuffer.Length} frames.");
        _frameBuffer.Clear();
        _frameBuffer.PushRange(initialFrameBuffer);

        // Starting the looping task.
        _logger.LogDebug($"Starting animation player at {frequency.Hertz}Hz, with initial frame buffer size of {_frameBuffer.Count}.");
        _stoppingToken = new CancellationTokenSource();

        _runningThread = CreateThread(AnimationLoopingThreadHandler);
        _runningThread.Start();

        return Task.CompletedTask;
    }


    private Thread CreateThread(Action loop)
    {
        return new Thread(() => loop())
        {
            IsBackground = true,
            Priority = ThreadPriority.Highest,
            Name = $"{Guid.NewGuid()} - Animation Player"
        };
    }


    /// <summary>
    /// The handler that is going to loop the animation and play it.
    /// </summary>
    private void AnimationLoopingThreadHandler()
    {
        try
        {
            // Calculating the wait time.
            int waitTime = (int)(1000 / Frequency.Hertz);
            _logger.LogTrace($"Calculated wait time is {waitTime}ms.");

            // Looping till we get data.
            while (!_stoppingToken!.Token.IsCancellationRequested)
            {
                // Getting the frame.
                if (!_frameBuffer.TryPop(out ReadOnlyMemory<PixelColor> frame))
                {
                    _logger.LogError("Frame buffer of animation player is empty. Stopping the player.");

                    // Clears the ledstrip of the current frame.
                    Ledstrip.Clear();

                    break;
                }

                // Setting the frame on the ledstrip.
                Ledstrip.SetColors(frame);

                // Check the stack buffer.
                CheckStackBuffer();

                // Start the delay untill the next frame.
                Thread.Sleep(waitTime);
            }
        }
        catch (IOException e)
        {
            _logger.LogError(e, "Error while playing animation,");

            _stoppingToken!.Cancel();
            _stoppingToken = null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while playing animation,");

            throw;
        }
    }


    /// <summary>
    /// Checks the frame buffer.
    /// </summary>
    protected virtual void CheckStackBuffer()
    {
        // Checking if we can request new stack buffer.
        if (_requestInProgress == false && _frameBuffer.Count < StackSize * FrameBufferRequestThreshold)
        {
            _requestInProgress = true;

            _logger.LogDebug("Starting new task to get the frames from the portal.");
            Task.Factory.StartNew(StartRequestForFramesAsync, _stoppingToken!.Token, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).ConfigureAwait(false);
        }
    }


    /// <summary>
    /// Starts the process of requesting frames from the portal.
    /// </summary>
    /// <returns> </returns>
    protected async Task StartRequestForFramesAsync()
    {
        // Calculating the target amount of frames.
        int targetAmount = StackSize - _frameBuffer.Count;
        _logger.LogTrace($"Animation player starting request to get frames. Requesting {targetAmount} frames. StackSize {StackSize}, FramebufferCount {_frameBuffer.Count}");

        try
        {
            // Starting the request for the frames.
            FrameBufferQuery result = await _requestFramesHandler.Execute(new RequestFrameBufferCommand
                                                                  {
                                                                      Amount = targetAmount,
                                                                      LedstripProxyBase = Ledstrip
                                                                  })
                                                                 .ConfigureAwait(false);

            // Addding the frames to the stack.
            _logger.LogTrace($"Adding {result.Frames.Length} frames to the stack buffer and indicating that we are not having it in progress anymore.");
            _frameBuffer.PushRange(result.Frames);
        }
        catch (PortalException e)
        {
            _logger.LogWarning(e, "There was a problem with the connection.");
            await StopAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "There was a unknown error in the receiving of a frame buffer.");
            await StopAsync().ConfigureAwait(false);

            throw;
        }
        finally
        {
            // Reset the flag that indicates that we are requesting for frames from the portal.
            _requestInProgress = false;
        }
    }


    /// <summary>
    /// Stops the animation that is playing on the ledstrip.
    /// </summary>
    /// <exception cref="InvalidOperationException"> The animation that we where playing. </exception>
    public Task StopAsync()
    {
        if (_stoppingToken == null) return Task.FromException(new InvalidOperationException("Cannot stop a animation that has already stopped."));

        _logger.LogDebug("Stopping animation player.");
        _stoppingToken?.Cancel();
        _stoppingToken?.Dispose();

        if (_runningThread != null)
        {
            _runningThread.Join(100);
        }

        _stoppingToken = null;
        _logger.LogTrace($"Stopped the animation player for {Ledstrip.Id}");

        return Task.CompletedTask;
    }
}