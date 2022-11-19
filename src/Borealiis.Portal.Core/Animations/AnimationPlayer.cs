using System;
using System.Drawing;
using System.Linq;

using Borealis.Domain.Animations;
using Borealis.Domain.Devices;
using Borealis.Domain.Effects;
using Borealis.Portal.Core.Exceptions;
using Borealis.Portal.Core.Interaction;
using Borealis.Portal.Infrastructure.Connections;

using Jint;
using Jint.Native;
using Jint.Runtime.Interop;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Animations;


internal class AnimationPlayer : LedstripInteractorBase
{
    private readonly ILogger<AnimationPlayer> _logger;

    private readonly Engine _engine;

    private Task? _task;
    private CancellationTokenSource? _stoppingToken;

    public bool IsPlaying => _task != null;


    public Animation Animation { get; }


    public AnimationPlayer(ILogger<AnimationPlayer> logger, Animation animation, IDeviceConnection deviceConnection, Ledstrip ledstrip) : base(logger, deviceConnection, ledstrip)
    {
        _logger = logger;
        Animation = animation;

        _engine = CreateJavascriptEngine();

        LoadJavascriptEnvironment();
        LoadJavascript(animation.Effect.Javascript);
    }


    private Engine CreateJavascriptEngine()
    {
        return new Engine(options =>
        {
            // TODO: Add memory limits.
        });
    }


    /// <inheritdoc />
    protected override async Task OnStartAsync(CancellationToken token)
    {
        _logger.LogDebug($"Starting animation on ledstrip {Ledstrip.Name}, animation {Animation.Name}");

        // Loads all the enviroment variables in the javascript engine.
        LoadJavascriptEnvironment();

        try
        {
            LoadJavascript(Animation.Effect.Javascript);
        }
        catch (Exception e)
        {
            throw new AnimationException("There was a problem loading the javascript.", e);
        }

        await StartTaskAsync(token);
    }


    /// <summary>
    /// Resumes playing the ledstrip.
    /// </summary>
    public async Task ResumeAsync(CancellationToken token = default)
    {
        _logger.LogDebug($"Resuming animation on ledstrip {Ledstrip.Name}, animation {Animation.Name}");
        await StartTaskAsync(token);
    }


    /// <summary>
    /// Stops playing on the ledstrips and blacks it out.
    /// </summary>
    public virtual async Task PauseAsync(CancellationToken token = default)
    {
        _logger.LogDebug($"Pausing animation on ledstrip {Ledstrip.Name}, animation {Animation.Name}");
        await StopTaskAsync(token);

        await SendColors(Enumerable.Repeat((PixelColor)Color.Black, Ledstrip.Length).ToArray());
    }


    /// <summary>
    /// Starts the underlying task that starts the animation.
    /// </summary>
    /// <param name="token"> </param>
    /// <returns> </returns>
    protected virtual async Task StartTaskAsync(CancellationToken token = default)
    {
        if (_task != null) throw new InvalidOperationException("Cannot start task on a task that is already start or still waiting to be ended.");

        _stoppingToken = new CancellationTokenSource();
        _task = Task.Factory.StartNew(StartAnimationAsync, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }


    /// <summary>
    /// Stops the underlying task that runs the aninmation.
    /// </summary>
    /// <param name="token"> </param>
    /// <returns> </returns>
    protected virtual async Task StopTaskAsync(CancellationToken token = default)
    {
        if (_task == null) throw new InvalidOperationException("The animation has not been started and the task is not initialized.");
        _stoppingToken!.Cancel();

        await _task;

        _stoppingToken = null;
        _task = null;
    }


    private async Task StartAnimationAsync()
    {
        try
        {
            LoadAnimationParameters(Animation);

            RunSetup();

            await StartLoopAsync();
        }
        catch (OperationCanceledException operationCanceledException) { }
        catch (Exception e)
        {
            _logger.LogError(e, "There was a error with running the player thread.");

            throw new AnimationException("Error starting animation.", e, Animation);
        }
    }


    private void RunSetup()
    {
        JsValue setupFunctionDelegate = _engine.GetValue("setup");

        setupFunctionDelegate.Invoke();
    }


    private async Task StartLoopAsync()
    {
        JsValue loopFunction = _engine.GetValue("loop");

        while (!_stoppingToken!.Token.IsCancellationRequested)
        {
            // Invokes the function.
            loopFunction.Invoke();

            // Now reading the colors form the colors array.
            PixelColor[] colors = _engine.GetValue("Pixels").TryCast<PixelColor[]>();
            await SendColors(colors);

            await Task.Delay(Animation.Frequency / 1, _stoppingToken.Token);
        }
    }


    /// <summary>
    /// Loads the javascript enviroment and
    /// </summary>
    private void LoadJavascriptEnvironment()
    {
        // Adding the color object.
        _engine.SetValue("PixelColor", TypeReference.CreateTypeReference(_engine, typeof(PixelColor)));

        // Setting the main color array thatw e need to set in the scripts.
        _engine.SetValue("Pixels", new PixelColor[Ledstrip.Length]);
    }


    /// <summary>
    /// Loads the javascript into the player.
    /// </summary>
    /// <param name="javascript"> </param>
    /// <exception cref="InvalidOperationException"> </exception>
    private void LoadJavascript(string javascript)
    {
        // Validating that we have both methods registered.
        if (!javascript.Contains("loop", StringComparison.InvariantCultureIgnoreCase)) throw new InvalidOperationException("The loop method is not defined in script.");
        if (!javascript.Contains("setup", StringComparison.InvariantCultureIgnoreCase)) throw new InvalidOperationException("The setup method is not defined in script.");

        _engine.Execute(javascript);
    }


    private void LoadAnimationParameters(Animation animation)
    {
        foreach (EffectParameter parameter in animation.Effect.EffectParameters)
        {
            _engine.SetValue(parameter.Identifier, parameter.Value);
        }
    }


    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (disposing)
        {
            _stoppingToken!.Dispose();
            _stoppingToken?.Dispose();
            _task?.Dispose();
        }

        _stoppingToken = null;
        _task = null;

        base.Dispose(disposing);
    }
}