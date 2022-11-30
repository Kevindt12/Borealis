﻿using Borealis.Domain.Effects;
using Borealis.Domain.Runtime;
using Borealis.Portal.Core.Exceptions;

using Jint;
using Jint.Runtime;
using Jint.Runtime.Interop;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Effects;


public class EffectEngine : IDisposable
{
    private readonly ILogger<EffectEngine> _logger;

    private const string LoopFunctionName = "Loop";
    private const string SetupFunctionName = "Setup";
    private const string PixelsName = "Pixels";

    private Engine _engine = null!;


    /// <summary>
    /// The effect we want to run.
    /// </summary>
    public Effect Effect { get; }

    /// <summary>
    /// The engine options.
    /// </summary>
    public EffectEngineOptions Options { get; }


    /// <summary>
    /// The length of the ledstrip.
    /// </summary>
    public int LedstripLength { get; }


    internal EffectEngine(ILogger<EffectEngine> logger, Effect effect, Int32 ledstripLength, EffectEngineOptions? options = null)
    {
        _logger = logger;

        // Object setup.
        Effect = effect;
        LedstripLength = ledstripLength;
        Options = options ?? new EffectEngineOptions();

        // Starting the engine.
        ValidateEffect(effect);

        // Start creating the runtime.
        CreateJavascriptRuntime();

        // Starts the initialization for the engine.
        InitializeJavascriptEngineAsync();
    }


    /// <summary>
    /// Creates the javascript engine.
    /// </summary>
    protected virtual void CreateJavascriptRuntime()
    {
        _engine = new Engine(options =>
        {
            // Limit memory allocations to MB
            options.LimitMemory(128_000_000);

            // Set a timeout to 5 seconds.
            options.TimeoutInterval(TimeSpan.FromSeconds(30));
        });
    }


    /// <summary>
    /// Starts up the entire javascript engine to be used for the effect.
    /// </summary>
    /// <exception cref="EffectEngineException"> When there is a problem loading a module. </exception>
    /// <exception cref="EffectEngineException"> When there is a problem loading the javascript in the engine. </exception>
    protected virtual void InitializeJavascriptEngineAsync()
    {
        _logger.LogDebug("Loading modules.");
        InitializeModules();

        // Initialize the Types
        _logger.LogDebug("Initializing Types.");
        InitializeTypes();

        // Initialize the variables
        _logger.LogDebug("Loading variables into the environment.");
        InitializeVariables();

        // Load the javascript.
        _logger.LogDebug("Loading javascript.");
        LoadJavascript();
    }


    /// <summary>
    /// Initializes the modules.
    /// </summary>
    /// <exception cref="EffectEngineException"> When there is a problem loading a module. </exception>
    protected virtual void InitializeModules()
    {
        foreach (JavascriptModule module in Effect.JavascriptModules)
        {
            try
            {
                // Execute the module.
                _logger.LogTrace($"Running script for modules {module.Id}.");
                _engine.Execute(module.Code);
            }
            catch (JintException e)
            {
                throw new EffectEngineException(module.Code, Effect, $"A modules {module.Id} was not able to be loaded.");
            }
        }
    }


    /// <summary>
    /// Loads the types that we can use and create in javascript.
    /// </summary>
    protected virtual void InitializeTypes()
    {
        // Loading the PixelColor Type
        _logger.LogTrace("Loading the PixelColor Type into the engine.");
        _engine.SetValue("PixelColor", TypeReference.CreateTypeReference(_engine, typeof(PixelColor)));
    }


    /// <summary>
    /// Loads all the variables that can be called in javascript.
    /// </summary>
    protected virtual void InitializeVariables()
    {
        // The effect parameters.
        foreach (EffectParameter parameter in Effect.EffectParameters)
        {
            _logger.LogDebug($"Loading parameter {parameter.Identifier} with value : {parameter.Value}.");
            _engine.SetValue(parameter.Identifier, parameter.Value);
        }

        // The pixels array.
        _logger.LogTrace("Loading the pixels array for the effect.");
        _engine.SetValue(PixelsName, new PixelColor[LedstripLength]);

        // The ledstrip count.
        _logger.LogTrace($"Setting the ledstrip length {LedstripLength} variable in the environment");
        _engine.SetValue("ledstripLength", LedstripLength);

        // The output log.
        if (Options.WriteLog != null)
        {
            _logger.LogTrace("Writing delegate to the output log.");
            _engine.SetValue("log", Options.WriteLog);
        }
    }


    /// <summary>
    /// Loads the javascript of the effect.
    /// </summary>
    /// <exception cref="EffectEngineException"> When there is a problem loading the javascript in the engine. </exception>
    protected virtual void LoadJavascript()
    {
        try
        {
            _logger.LogTrace($"Loading javascript for effect {Effect.Id}.");
            _engine.Execute(Effect.Javascript);
        }
        catch (JintException e)
        {
            _logger.LogTrace(e, "Javascript was unable to be loaded.");

            throw new EffectEngineException(Effect.Javascript, Effect, "Unable to run the javascript of the effect.", e);
        }
    }


    /// <summary>
    /// Runs the setup function in the javascript.
    /// </summary>
    /// <returns> </returns>
    public async Task RunSetupAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        // Get the function delegate and run it.
        _logger.LogTrace($"Running the invoke method on the javascript for effect {Effect.Id}.");
        _engine.Invoke(SetupFunctionName);
    }


    /// <summary>
    /// Runs the loop function in the javascript.
    /// </summary>
    /// <returns> A <see cref="ReadOnlyMemory{PixelColor}" /> of the colors that we should display on the ledstrip. </returns>
    public async ValueTask<ReadOnlyMemory<PixelColor>> RunLoopAsync()
    {
        // Get the function delegate and run it.
        _engine.Invoke(LoopFunctionName);

        PixelColor[] colors = ((object[])_engine.GetValue(PixelsName).ToObject()).Cast<PixelColor>().ToArray();

        return colors;
    }


    /// <summary>
    /// Validates and makes sure that we have the basic things to run the effect.
    /// </summary>
    /// <remarks>
    /// Note that we are not validating the javascript completely. Because the engine needs to be already created for that.
    /// Because we want to prevent loading everything and then finding out that the javascript is missing a function,
    /// </remarks>
    /// <param name="effect"> The effect we want to validate. </param>
    /// <exception cref="ArgumentNullException"> If the Javascript is empty. </exception>
    /// <exception cref="EffectException"> When the Javascript is not valid. </exception>
    protected virtual void ValidateEffect(Effect effect)
    {
        // Making sure that its not empty.
        if (String.IsNullOrEmpty(effect.Javascript)) throw new ArgumentNullException(nameof(effect.Javascript), "The javascript cannot be empty to run it.");

        // Validating that we have both methods registered.
        if (!effect.Javascript.Contains(LoopFunctionName, StringComparison.InvariantCultureIgnoreCase)) throw new EffectException("The loop method is not defined in script.");
        if (!effect.Javascript.Contains(SetupFunctionName, StringComparison.InvariantCultureIgnoreCase)) throw new EffectException("The setup method is not defined in script.");
    }


    #region MyRegion

    private bool _disposed;


    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        Dispose(true);
        GC.SuppressFinalize(this);
    }


    protected virtual void Dispose(bool disposing)
    {
        if (disposing) { }

        _engine = null!;
    }

    #endregion
}