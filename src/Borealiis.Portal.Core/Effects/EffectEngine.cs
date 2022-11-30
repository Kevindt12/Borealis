using Borealis.Domain.Effects;
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


    /// <summary>
    /// The effect engine that handles talking to the underlying runtime.
    /// </summary>
    internal EffectEngine(ILogger<EffectEngine> logger, Effect effect, Int32 ledstripLength, EffectEngineOptions options)
    {
        _logger = logger;

        // Object setup.
        Effect = effect;
        LedstripLength = ledstripLength;
        Options = options;

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
            options.LimitMemory(Options.MemoryLimit);

            // Set a timeout to 5 seconds.
            options.TimeoutInterval(Options.TimeoutInterval);
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
                throw new EffectEngineRuntimeException(module.Code, Effect, $"A modules {module.Id} was not able to be loaded.");
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

            throw new EffectEngineRuntimeException(Effect, "Unable to run the javascript of the effect.", e);
        }
    }


    /// <summary>
    /// Runs the setup function in the javascript.
    /// </summary>
    /// <returns> </returns>
    public virtual async Task RunSetupAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        try
        {
            // Running both methods to first of all check that they are working and to make sure that we have run at least a single loop.
            _logger.LogTrace($"Running the invoke methods in the javascript for effect {Effect.Id}.");

            // TODO: Big change this will of cause throw a AggregateException
            await Task.Run(() =>
                           {
                               // The two methods that we want to run and test.
                               _engine.Invoke(SetupFunctionName);
                               _engine.Invoke(LoopFunctionName);
                           },
                           token)
                      .ConfigureAwait(false);
        }
        catch (JintException e)
        {
            _logger.LogTrace(e, "Error while running the setup for the effect engine.");

            // Rethrow the exception wrapped.
            throw new EffectEngineRuntimeException(Effect, "Error while running the setup for the effect engine.", e);
        }
    }


    /// <summary>
    /// Runs the loop function in the javascript.
    /// </summary>
    /// <returns> A <see cref="ReadOnlyMemory{PixelColor}" /> of the colors that we should display on the ledstrip. </returns>
    public virtual ValueTask<ReadOnlyMemory<PixelColor>> RunLoopAsync()
    {
        try
        {
            // Running the function.
            _engine.Invoke(LoopFunctionName);

            // Getting the pixel array wrapping it in a memory and sending it back as a read only.
            ReadOnlyMemory<PixelColor> colors = new ReadOnlyMemory<PixelColor>(_engine.GetValue(PixelsName).AsArray().AsInstance<PixelColor[]>());

            return ValueTask.FromResult(colors);
        }
        catch (JintException e)
        {
            throw new EffectEngineRuntimeException(Effect, "Error running Loop() function in the javascript.", e);
        }
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
    /// <exception cref="EffectEngineException"> When the Javascript is not valid. </exception>
    protected virtual void ValidateEffect(Effect effect)
    {
        // Making sure that its not empty.
        if (String.IsNullOrEmpty(effect.Javascript)) throw new ArgumentNullException(nameof(effect.Javascript), "The javascript cannot be empty to run it.");

        // Validating that we have both methods registered.
        if (!effect.Javascript.Contains(LoopFunctionName, StringComparison.InvariantCultureIgnoreCase)) throw new EffectEngineException("The loop method is not defined in script.");
        if (!effect.Javascript.Contains(SetupFunctionName, StringComparison.InvariantCultureIgnoreCase)) throw new EffectEngineException("The setup method is not defined in script.");
    }


    #region MyRegion

    private bool _disposed;


    /// <inheritdoc />
    public void Dispose()
    {
        // Checking if we have disposed.
        if (_disposed) return;

        // Disposing.
        Dispose(true);
        GC.SuppressFinalize(this);

        // Setting flag.
        _disposed = true;
    }


    protected virtual void Dispose(bool disposing)
    {
        if (disposing) { }

        // Cleaning the engine by setting it to null so the GC can come collect it.
        _engine = null!;
    }

    #endregion
}