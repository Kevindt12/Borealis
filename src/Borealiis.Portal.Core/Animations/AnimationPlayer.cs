using System;
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


    public virtual Animation Animation { get; }

    /// <inheritdoc />
    public virtual ILedstripConnection Ledstrip { get; }

    /// <inheritdoc />
    public virtual Boolean IsRunning => _task != null && _stoppingToken != null;


    public AnimationPlayer(ILogger<AnimationPlayer> logger, Animation animation, EffectEngine engine, ILedstripConnection ledstrip)
    {
        _logger = logger;
        Animation = animation;
        Ledstrip = ledstrip;

        _effectEngine = engine;
    }


    /// <summary>
    /// Starts the underlying task that starts the animation.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> </returns>
    protected virtual async Task StartTaskAsync(CancellationToken token = default)
    {
        if (_task != null) throw new InvalidOperationException("Cannot start task on a task that is already start or still waiting to be ended.");

        _logger.LogTrace($"Starting Looping task for {Animation.Id}.");
        _stoppingToken = new CancellationTokenSource();
        _task = await Task.Factory.StartNew(TaskStartAsync, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }


    /// <summary>
    /// Stops the underlying task that runs the aninmation.
    /// </summary>
    /// <param name="token"> </param>
    /// <returns> </returns>
    public virtual async Task StopAsync(CancellationToken token = default)
    {
        if (_task == null) throw new InvalidOperationException("The animation has not been started and the task is not initialized.");

        _logger.LogTrace($"Stopping animation player {Animation.Id}");
        _stoppingToken!.Cancel();

        try
        {
            _logger.LogTrace("Stopping task and awaiting for it.");
            await _task.ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error task in cleanup for animation player.");
        }
        finally
        {
            // Clean up the task.
            _stoppingToken.Dispose();
            _stoppingToken = null;
            _task = null;
        }
    }


    /// <inheritdoc />
    public virtual async Task StartAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        _logger.LogTrace($"Starting animation player for animation {Animation.Id}.");

        try
        {
            _logger.LogTrace($"Running setup of animation {Animation.Id}.");
            await _effectEngine.RunSetupAsync(token).ConfigureAwait(false);

            _logger.LogTrace("The start task ");
            await StartTaskAsync(token).ConfigureAwait(false);
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


    private async Task TaskStartAsync()
    {
        // Checking the stopping token if we need to stop.
        while (!_stoppingToken!.Token.IsCancellationRequested)
        {
            ReadOnlyMemory<PixelColor> colors = await _effectEngine.RunLoopAsync().ConfigureAwait(false);

            await Ledstrip.SendFrameAsync(colors);

            // HACK: should become PeriodicTimer
            await Task.Delay((int)(1000 / Animation.Frequency.PerSecond));
        }
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