using Borealis.Domain.Effects;
using Borealis.Domain.Runtime;
using Borealis.Portal.Core.Effects.Handlers;
using Borealis.Portal.Domain.Effects.Factories;
using Borealis.Portal.Domain.Effects.Handlers;
using Borealis.Portal.Domain.Effects.Options;
using Borealis.Portal.Domain.Exceptions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace Borealis.Portal.Core.Effects.Factories;


internal class EffectEngineFactory : IEffectEngineFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly JavascriptFilePathsOptions _javascriptFilePathOptions;


    /// <summary>
    /// The factory that is responsible for creating effect engines.
    /// </summary>
    public EffectEngineFactory(ILoggerFactory loggerFactory, IOptionsSnapshot<JavascriptFilePathsOptions> javascriptFilePathOptions)
    {
        _loggerFactory = loggerFactory;
        _javascriptFilePathOptions = javascriptFilePathOptions.Value;
    }


    /// <summary>
    /// Creates a new effect engine.
    /// </summary>
    /// <param name="effect"> The effect that we want to create a engine around. </param>
    /// <param name="ledstripLength"> The length of the ledstrip. </param>
    /// <param name="options"> The optional parameters that we can send with the engine. </param>
    /// <exception cref="ArgumentNullException"> If the Javascript is empty. </exception>
    /// <exception cref="EffectEngineException"> When the Javascript is not valid. </exception>
    public async Task<IEffectEngine> CreateEffectEngineAsync(Effect effect, int ledstripLength, EffectEngineOptions? options = null, CancellationToken token = default)
    {
        JavascriptModule module = new JavascriptModule(await File.ReadAllTextAsync(_javascriptFilePathOptions.JavascriptBaseLibraryPath, token).ConfigureAwait(false));

        return new EffectEngine(_loggerFactory.CreateLogger<EffectEngine>(), effect, module, ledstripLength, options ?? new EffectEngineOptions());
    }
}