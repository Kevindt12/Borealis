using System;
using System.Diagnostics;
using System.Linq;

using Borealis.Domain.Animations;
using Borealis.Domain.Effects;
using Borealis.Portal.Core.Effects;
using Borealis.Portal.Core.Exceptions;
using Borealis.Portal.Domain.Animations;
using Borealis.Portal.Domain.Connections;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Animations;


internal class AnimationPlayer : IAnimationPlayer
{
    private readonly ILogger<AnimationPlayer> _logger;

    private Task? _task;
    private CancellationTokenSource? _stoppingToken;

    private EffectEngine _effectEngine;

    public int InitialStackSize { get; set; } = 400;


    public virtual Animation Animation { get; }

    /// <inheritdoc />
    public virtual ILedstripConnection Ledstrip { get; }

    /// <inheritdoc />
    public virtual Boolean IsRunning { get; protected set; }


    public AnimationPlayer(ILogger<AnimationPlayer> logger, Animation animation, EffectEngine engine, ILedstripConnection ledstrip)
    {
        _logger = logger;
        Animation = animation;
        Ledstrip = ledstrip;

        ledstrip.FramesRequested += LedstripOnFramesRequested;

        _effectEngine = engine;
    }


    private async void LedstripOnFramesRequested(Object? sender, FramesRequestedEventArgs e)
    {
        _logger.LogTrace($"Sending {e.Amount} of frames to ledstrip {Ledstrip.Ledstrip.Name}");

        await Ledstrip.SendFramesBufferAsync(await BuildFramesPackage(e.Amount));
    }


    /// <summary>
    /// Stops the underlying task that runs the aninmation.
    /// </summary>
    /// <param name="token"> </param>
    /// <returns> </returns>
    public virtual async Task StopAsync(CancellationToken token = default)
    {
        if (!IsRunning) throw new InvalidOperationException("The animation has not been started and the task is not initialized.");

        await Ledstrip.StopAnimationAsync(token);

        IsRunning = false;
    }


    /// <inheritdoc />
    public virtual async Task StartAsync(CancellationToken token = default)
    {
        if (IsRunning) throw new InvalidOperationException("Cannot start task on a task that is already start or still waiting to be ended.");
        token.ThrowIfCancellationRequested();
        _logger.LogTrace($"Starting animation player for animation {Animation.Id}.");

        try
        {
            IsRunning = true;
            _logger.LogTrace($"Running setup of animation {Animation.Id}.");
            await _effectEngine.RunSetupAsync(token).ConfigureAwait(false);

            _logger.LogTrace("Building the Initial Frame buffer.");
            IEnumerable<ReadOnlyMemory<PixelColor>> frameBuffer = await BuildFramesPackage(InitialStackSize);

            _logger.LogTrace("The start animation on device ");
            await Ledstrip.StartAnimationAsync(Animation.Frequency, frameBuffer, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException operationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "There was a error with running the player thread.");

            throw new AnimationException("Error starting animation.", e, Animation);
        }
    }


    private async Task<IEnumerable<ReadOnlyMemory<PixelColor>>> BuildFramesPackage(int framesCount)
    {
        List<ReadOnlyMemory<PixelColor>> result = new List<ReadOnlyMemory<PixelColor>>();
        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < framesCount; i++)
        {
            result.Add(await _effectEngine.RunLoopAsync().ConfigureAwait(false));
        }

        stopwatch.Stop();

        _logger.LogTrace($"New frame package build in {stopwatch.Elapsed} for {framesCount} frames.");

        return result;
    }


    private bool _disposed;


    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed) return;

        Dispose(true);
        GC.SuppressFinalize(this);

        _disposed = true;
    }


    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _stoppingToken!.Dispose();
            _stoppingToken?.Dispose();
            _task?.Dispose();
            _effectEngine?.Dispose();
        }

        _stoppingToken = null;
        _task = null;
        _effectEngine = null!;
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (!_disposed) return;

        await DisposeAsyncCore();

        Dispose(false);
        GC.SuppressFinalize(this);

        _disposed = true;
    }


    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_task != null)
        {
            try
            {
                await _task;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error cleaning up the animation player.");
            }
        }
    }
}