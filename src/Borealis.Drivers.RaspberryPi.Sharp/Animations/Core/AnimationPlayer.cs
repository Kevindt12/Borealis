using System;
using System.Collections.Concurrent;
using System.Linq;

using Borealis.Drivers.RaspberryPi.Sharp.Animations.Options;
using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Services;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Proxies;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using UnitsNet;



namespace Borealis.Drivers.RaspberryPi.Sharp.Animations.Core;


public delegate Task<IEnumerable<ReadOnlyMemory<PixelColor>>> RequestFrameBufferAsync(int amount);



public class AnimationPlayer : IAnimationPlayer
{
	private readonly ILogger<AnimationPlayer> _logger;
	private readonly IConnectionService _connectionService;
	private readonly AnimationOptions _animationOptions;

	// Fram buffer.
	private readonly ConcurrentStack<ReadOnlyMemory<PixelColor>> _frameBuffer;

	// Frame requesting.
	private bool _requestInProgress;

	// Animation loop
	private CancellationTokenSource? _stoppingToken;
	private Thread? _runningThread;


	/// <summary>
	/// The ledstrip that we want to play the animation on.
	/// </summary>
	public LedstripProxyBase Ledstrip { get; }


	/// <summary>
	/// The Threshold that needs to be passed before the request is send to get more frame buffers.
	/// </summary>
	public virtual double FrameBufferRequestThreshold => _animationOptions.FrameBufferRequestThreshold;

	/// <summary>
	/// The stack size of the frame buffer.
	/// </summary>
	public virtual int StackSize => _animationOptions.TargetStackSize;


	/// <summary>
	/// The frequency we want to play the animation at.
	/// </summary>
	public virtual Frequency Frequency { get; set; }

	/// <summary>
	/// Indicates if the animation is running or not.
	/// </summary>
	public virtual bool IsRunning => _stoppingToken != null;


	public AnimationPlayer(ILogger<AnimationPlayer> logger,
						   IOptions<AnimationOptions> animationOptions,
						   IConnectionService connectionService,
						   LedstripProxyBase ledstrip)
	{
		_logger = logger;
		_connectionService = connectionService;
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
		_logger.LogTrace($"Animation player starting request to get frames. Requesting {targetAmount} frames. StackSize {StackSize}, Frame buffer Count {_frameBuffer.Count}");

		try
		{
			// Starting the request for the frames.

			ReadOnlyMemory<PixelColor>[] frames = await _connectionService.RequestFrameBufferAsync(Ledstrip.Ledstrip, targetAmount).ConfigureAwait(false);

			// Adding the frames to the stack.
			_logger.LogTrace($"Adding {frames.Length} frames to the stack buffer and indicating that we are not having it in progress anymore.");
			_frameBuffer.PushRange(frames);
		}

		//catch (PortalException e)
		//{
		//    _logger.LogWarning(e, "There was a problem with the connection.");
		//    await StopAsync().ConfigureAwait(false);
		//}
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
	public virtual Task StopAsync(CancellationToken token = default)
	{
		if (_stoppingToken == null) return Task.FromException(new InvalidOperationException("Cannot stop a animation that has already stopped."));

		_logger.LogDebug("Stopping animation player.");
		_stoppingToken?.Cancel();
		_stoppingToken?.Dispose();

		_runningThread?.Join(100);

		_stoppingToken = null;
		_logger.LogTrace($"Stopped the animation player for {Ledstrip.Id}");

		Ledstrip.Clear();

		return Task.CompletedTask;
	}


	/// <inheritdoc />
	public void Dispose()
	{
		_stoppingToken?.Dispose();
		_frameBuffer.Clear();
	}
}