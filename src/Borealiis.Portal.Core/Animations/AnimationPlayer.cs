using System;
using System.Diagnostics;
using System.Linq;

using Borealis.Domain.Effects;
using Borealis.Domain.Ledstrips;
using Borealis.Portal.Core.Exceptions;
using Borealis.Portal.Domain.Animations;
using Borealis.Portal.Domain.Common;
using Borealis.Portal.Domain.Connectivity.Connections;
using Borealis.Portal.Domain.Effects.Handlers;
using Borealis.Portal.Domain.Exceptions;

using Microsoft.Extensions.Logging;

using DeviceException = Borealis.Portal.Domain.Exceptions.DeviceException;



namespace Borealis.Portal.Core.Animations;


internal class AnimationPlayer : IAnimationPlayer
{
    private const int InitialFrameBufferSize = 200;

    private readonly ILogger<AnimationPlayer> _logger;
    private readonly IEffectEngine _effectEngine;
    private readonly ILedstripConnection _ledstripConnection;

    private bool _initialized;
    private bool _started;

    /// <inheritdoc />
    public virtual Effect Effect { get; }

    /// <inheritdoc />
    public virtual Ledstrip Ledstrip { get; }

    /// <inheritdoc />
    public virtual Boolean IsRunning { get; protected set; }


    public AnimationPlayer(ILogger<AnimationPlayer> logger, IEffectEngine engine, ILedstripConnection connection)
    {
        _logger = logger;
        _effectEngine = engine;
        _ledstripConnection = connection;

        connection.FrameBufferRequestHandler = RequestForFrameBufferHandler;

        Ledstrip = connection.Ledstrip;
        Effect = _effectEngine.Effect;
    }


    /// <summary>
    /// Handles when the connection with the ledstrip has dropped. The connection will then dispose and we should stop the animation then.
    /// </summary>
    /// <param name="sender"> The <see cref="ILedstripConnection" /> that is sending this disposal message. </param>
    /// <param name="e"> A <see cref="EventArgs.Empty" /> </param>
    private void HandleOnLedstripConnectionDisposing(Object? sender, EventArgs e)
    {
        // Check if we are playing a animation.
        IsRunning = false;
    }


    /// <summary>
    /// Initializes the effect engine and makes it ready to play the animation..
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <exception cref="OperationCanceledException"> When the operation has been cancelled by the token. </exception>
    /// <exception cref="EffectEngineRuntimeException"> The effect engine has had a exception while running the code. </exception>
    protected virtual async Task EnsureEffectEngineInitializationAsync(CancellationToken token = default)
    {
        if (_initialized) return;

        try
        {
            // Initialize the base engine.
            _logger.LogTrace("Initializing the effect engine.");
            await _effectEngine.InitializeAsync(token).ConfigureAwait(false);

            // initialize the effect that we want to run.
            _logger.LogTrace($"Running setup of animation {Effect.Id}.");
            await _effectEngine.RunSetupAsync(token).ConfigureAwait(false);

            _initialized = true;
        }
        catch (EffectEngineRuntimeException e)
        {
            throw new AnimationException("There was a problem running the startup on the effect engine.", e);
        }
    }


    /// <summary>
    /// The handler for sending new frame buffer bundles.
    /// </summary>
    /// <remarks>
    /// Note that its supposed to throw exception and not catch them here. Because any exception here will be caught in the
    /// <see cref="ILedstripConnection" /> and handled also send to the device so it can stop the animation.
    /// </remarks>
    /// <param name="amount"> The amount of frames requested. </param>
    /// <returns> A array of <see cref="ReadOnlyMemory{T}" /> that represents the each frame. </returns>
    protected virtual async Task<ReadOnlyMemory<ReadOnlyMemory<PixelColor>>> RequestForFrameBufferHandler(Int32 amount)
    {
        _logger.LogTrace($"Creating new frame buffer for ledstrip {Ledstrip.Name}");

        // build the package and return.
        return await BuildFramesPackage(amount).ConfigureAwait(false);
    }


    /// <inheritdoc />
    /// <exception cref="InvalidOperationException"> Thrown when the animation has already started. </exception>
    /// <exception cref="OperationCanceledException"> When the operation has been cancelled. </exception>
    /// <exception cref="Exceptions.DeviceException"> Thrown when there is a that the device experienced. </exception>
    /// <exception cref="DeviceCommunicationException"> Thrown when the device has problems with the communication. </exception>
    /// <exception cref="DeviceConnectionException"> Thrown when there is a problem with the connection with the device. </exception>
    /// <exception cref="Exceptions.AnimationException"> Thrown when there is something that went wrong with the playing the animation. </exception>
    public virtual async Task StartAsync(CancellationToken token = default)
    {
        if (IsRunning) throw new InvalidOperationException("Cannot start task on a task that is already start or still waiting to be ended.");
        token.ThrowIfCancellationRequested();

        // Ensure that we have the effect engine ready to be used.
        await EnsureEffectEngineInitializationAsync(token).ConfigureAwait(false);

        try
        {
            _logger.LogDebug($"Starting animation player for animation {Effect.Id}.");
            ReadOnlyMemory<ReadOnlyMemory<PixelColor>> frameBuffer = await BuildFramesPackage(InitialFrameBufferSize);

            _logger.LogTrace("Starting animation on device, sending the initial frame buffer.");
            await _ledstripConnection.StartAnimationAsync(Effect.Frequency, frameBuffer, token).ConfigureAwait(false);

            // Indicate that we are running and that we have told the device that we have run at least once.
            IsRunning = true;
            _started = true;
        }
        catch (DeviceException deviceException)
        {
            _logger.LogTrace(deviceException, "Error while starting the animation on the device.");

            throw;
        }
        catch (EffectEngineException e)
        {
            _logger.LogError(e, "There was a problem creating the frame buffer.");

            throw new AnimationException("Error starting animation.", e);
        }
    }


    /// <inheritdoc />
    /// <exception cref="Exceptions.DeviceException"> Thrown when there is a that the device experienced. </exception>
    /// <exception cref="Domain.Exceptions.DeviceCommunicationException"> Thrown when the device has problems with the communication. </exception>
    /// <exception cref="Domain.Exceptions.DeviceConnectionException"> Thrown when there is a problem with the connection with the device. </exception>
    public virtual async Task PauseAsync(CancellationToken token = default)
    {
        try
        {
            // Only when we ware running then we send also to the device that we are stopping..
            if (IsRunning)
            {
                _logger.LogDebug("Stopping the animation on the ledstrip side. Thereby sending a request to the device to stop the animation.");
                await _ledstripConnection.PauseAnimationAsync(token).ConfigureAwait(false);
            }
        }
        catch (DeviceException deviceException)
        {
            _logger.LogTrace(deviceException, "Unable to stop animation.");

            throw;
        }
        finally
        {
            // Always setting the running state to False.
            IsRunning = false;
        }
    }


    /// <summary>
    /// Creates a bundle of frames for the driver to display.
    /// </summary>
    /// <param name="framesCount"> The amount of frames requested. </param>
    /// <exception cref="EffectEngineRuntimeException"> Thrown when there is a problem running the javascript. </exception>
    /// <returns> A <see cref="ReadOnlyMemory{T}[]" /> array that contains all the frames. </returns>
    protected virtual async Task<ReadOnlyMemory<ReadOnlyMemory<PixelColor>>> BuildFramesPackage(int framesCount)
    {
        // Creating the result array and starting a stopwatch to see how long it takes to create this.
        ReadOnlyMemory<PixelColor>[] result = new ReadOnlyMemory<PixelColor>[framesCount];
        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < framesCount; i++)
        {
            result[i] = await _effectEngine.RunLoopAsync().ConfigureAwait(false);
        }

        // Stopping and logging the time taken to create the bundle of frames.
        stopwatch.Stop();
        _logger.LogTrace($"New frame package build in {stopwatch.Elapsed} for {framesCount} frames.");

        return result;
    }


    private bool _disposed;


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        await DisposeAsyncCore();

        _disposed = true;
    }


    protected virtual async ValueTask DisposeAsyncCore()
    {
        // Only run if the animation has run on the device.
        if (_started)
        {
            // Stop the animation on the ledstrip. This will also remove the stack buffer from the device.
            await _ledstripConnection.StopAnimationAsync();
        }

        _effectEngine.Dispose();
    }
}