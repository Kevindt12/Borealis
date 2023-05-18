using Borealis.Domain.Effects;
using Borealis.Domain.Runtime;
using Borealis.Portal.Domain.Common;
using Borealis.Portal.Domain.Effects.Handlers;
using Borealis.Portal.Domain.Exceptions;

using Jint;
using Jint.Runtime;
using Jint.Runtime.Interop;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Effects.Handlers;


// Note : That we will be using async tho Jint does not use async. We want to to make sure that in the future when we want to change to something else we need to worry about implementing the async part also.



/**
 * TODO: Add Few functions base functions.
 * TODO: Make it so when we do {var s = (rgb(red, green, blue)) we have a method that also supllies us the proper values.
 * TODO: Also add a method that is hsl(hue, saturation, luminosity) that converts the hsl value to the rgb value and gives and gives back the new object.
 */
internal class EffectEngine : IEffectEngine
{
    private readonly ILogger<EffectEngine> _logger;
    private readonly JavascriptModule _baseLibrary;

    private const string LoopFunctionName = "Loop";
    private const string SetupFunctionName = "Setup";
    private const string PixelsName = "Pixels";

    private Engine _engine;

    /// <summary>
    /// The effect we want to run.
    /// </summary>
    public Effect Effect { get; }

    /// <summary>
    /// The effect file that we are using to run this effect.
    /// </summary>
    public EffectFile EffectFile => Effect.Files.Last();

    /// <summary>
    /// The engine options.
    /// </summary>
    public EffectEngineOptions Options { get; }

    /// <summary>
    /// The length of the ledstrip.
    /// </summary>
    public int PixelCount { get; }

    /// <inheritdoc />
    public bool Ready { get; }


    /// <summary>
    /// The effect engine that handles talking to the underlying runtime.
    /// </summary>
    public EffectEngine(ILogger<EffectEngine> logger,
                        Effect effect,
                        JavascriptModule baseLibrary,
                        int pixelCount,
                        EffectEngineOptions effectEngineOptions
    )
    {
        _logger = logger;
        _baseLibrary = baseLibrary;

        // Object setup.
        Effect = effect;
        PixelCount = pixelCount;
        Options = effectEngineOptions;

        _engine = new Engine(options =>
        {
            // Limit memory allocations to MB
            options.LimitMemory(effectEngineOptions.MemoryLimit);

            // Set a timeout to 5 seconds.
            options.TimeoutInterval(effectEngineOptions.TimeoutInterval);
        });
    }


    #region Initialization

    /// <summary>
    /// Initializes the engine ready for use.
    /// </summary>
    /// <param name="token"> </param>
    /// <returns> </returns>
    /// <exception cref="EffectEngineRuntimeException"> </exception>
    public virtual async Task InitializeAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        // Validate the effect.
        _logger.LogDebug($"Start creating a new Effect engine for effect {Effect.Id}.");
        ValidateEffectFile(Effect.Files.Last());

        try
        {
            InitializeBaseLibrary(_baseLibrary);

            InitializeJavascriptModules(Effect.JavascriptModules);

            InitializeEffectParameters(Effect.EffectParameters);

            InitializePixelArray(PixelCount);

            InitializeEngineOptions();

            LoadJavascriptFile(Effect.Files.Last());
        }
        catch (JintException jintException)
        {
            _logger.LogTrace(jintException, "Error while trying to execute javascript.");

            throw new EffectEngineRuntimeException(Effect, "The effect engine has had a problem executing some code.", jintException);
        }
    }


    /// <summary>
    /// Initializes the base javascript library.
    /// </summary>
    /// <param name="baseLibrary"> The base library that we want to load. </param>
    protected virtual void InitializeBaseLibrary(JavascriptModule baseLibrary)
    {
        // Loading the primary function that can be used.
        _logger.LogTrace("Loading the base library for the engine.");
        _engine.Execute(_baseLibrary.Code);

        // Loading the PixelColor Type
        _logger.LogTrace("Loading the PixelColor Type into the engine.");
        _engine.SetValue("PixelColor", TypeReference.CreateTypeReference(_engine, typeof(PixelColor)));
    }


    protected virtual void InitializeJavascriptModules(IEnumerable<JavascriptModule> modules)
    {
        _logger.LogTrace("Loading all modules.");

        foreach (JavascriptModule module in modules)
        {
            // Execute the module.
            _logger.LogTrace($"Running script for modules {module.Id}.");
            _engine.Execute(module.Code);
        }
    }


    protected virtual void InitializeEffectParameters(IEnumerable<EffectParameter> parameters)
    {
        // The effect parameters.
        _logger.LogDebug("Loading effect parameters");

        foreach (EffectParameter parameter in parameters)
        {
            _logger.LogTrace($"Loading parameter {parameter.Identifier} with value : {parameter.Value}.");
            _engine.SetValue(parameter.Identifier, parameter.Value);
        }
    }


    protected virtual void InitializePixelArray(int pixelCount)
    {
        // Setting variables.
        _logger.LogTrace("Loading the pixels array for the effect.");
        _logger.LogTrace($"Setting the ledstrip length {pixelCount} variable in the environment");
        _engine.SetValue(PixelsName, new PixelColor[pixelCount]);
        _engine.SetValue("ledstripLength", pixelCount);
    }


    protected virtual void InitializeEngineOptions()
    {
        // The output log.
        if (Options.WriteLog != null)
        {
            _logger.LogTrace("Writing delegate to the output log.");
            _engine.SetValue("log", Options.WriteLog);
        }
    }


    protected virtual void LoadJavascriptFile(EffectFile file)
    {
        // Loading the effect javascript.
        _logger.LogDebug($"Loading javascript for effect {Effect.Id}.");
        _engine.Execute(file.Javascript);
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
    private static void ValidateEffectFile(EffectFile effect)
    {
        // Making sure that its not empty.
        if (string.IsNullOrEmpty(effect.Javascript)) throw new ArgumentNullException(nameof(effect.Javascript), "The javascript cannot be empty to run it.");

        // Validating that we have both methods registered.
        if (!effect.Javascript.Contains(LoopFunctionName, StringComparison.InvariantCultureIgnoreCase)) throw new EffectEngineException("The loop method is not defined in script.");
        if (!effect.Javascript.Contains(SetupFunctionName, StringComparison.InvariantCultureIgnoreCase)) throw new EffectEngineException("The setup method is not defined in script.");
    }

    #endregion


    #region Runtime

    /// <summary>
    /// Runs the setup function in the javascript.
    /// </summary>
    /// <exception cref="EffectEngineRuntimeException"> Thrown when there is a problem running the javascript. </exception>
    /// <exception cref="OperationCanceledException"> When the operation has been cancelled by the token. </exception>
    /// <returns> </returns>
    public virtual Task RunSetupAsync(CancellationToken token = default)
    {
        if (!Ready) throw new InvalidOperationException("The engine is not initialized.");

        if (token.IsCancellationRequested) return Task.FromCanceled(token);

        try
        {
            // Running both methods to first of all check that they are working and to make sure that we have run at least a single loop.
            _logger.LogTrace($"Running the invoke methods in the javascript for effect {Effect.Id}.");

            // The two methods that we want to run and test.
            _engine.Invoke(SetupFunctionName);
            _engine.Invoke(LoopFunctionName);

            return Task.CompletedTask;
        }
        catch (JintException e)
        {
            _logger.LogTrace(e, "Error while running the setup for the effect engine.");

            // Rethrow the exception wrapped.
            return Task.FromException(new EffectEngineRuntimeException(Effect, "Error while running the setup for the effect engine.", e));
        }
    }


    /// <summary>
    /// Runs the loop function in the javascript.
    /// </summary>
    /// <exception cref="EffectEngineRuntimeException"> Thrown when there is a problem running the javascript. </exception>
    /// <returns> A <see cref="ReadOnlyMemory{PixelColor}" /> of the colors that we should display on the ledstrip. </returns>
    public virtual ValueTask<ReadOnlyMemory<PixelColor>> RunLoopAsync()
    {
        try
        {
            // Running the function.
            _engine.Invoke(LoopFunctionName);

            // Getting the items boxed.
            IEnumerable<object> boxedColors = _engine.GetValue(PixelsName).AsArray().ToObject() as object[] ?? throw new EffectEngineRuntimeException(Effect, "The pixels where null");

            // Converts the type to PixelColor and
            ReadOnlyMemory<PixelColor> colors = new ReadOnlyMemory<PixelColor>(boxedColors.OfType<PixelColor>().ToArray());

            return ValueTask.FromResult(colors);
        }
        catch (JintException e)
        {
            return ValueTask.FromException<ReadOnlyMemory<PixelColor>>(new EffectEngineRuntimeException(Effect, "Error running Loop() function in the javascript.", e));
        }
    }

    #endregion


    #region Dispsable

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