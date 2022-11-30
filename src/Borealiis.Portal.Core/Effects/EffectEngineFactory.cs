using Borealis.Domain.Effects;
using Borealis.Portal.Core.Exceptions;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Effects;


public class EffectEngineFactory
{
    private readonly ILoggerFactory _loggerFactory;


    /// <summary>
    /// The factory that is responsible for creating effect engines.
    /// </summary>
    public EffectEngineFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }


    /// <summary>
    /// Creates a new effect engine.
    /// </summary>
    /// <param name="effect"> The effect that we want to create a engine around. </param>
    /// <param name="ledstripLength"> The length of the ledstrip. </param>
    /// <param name="options"> The optional parameters that we can send with the engine. </param>
    /// <exception cref="ArgumentNullException"> If the Javascript is empty. </exception>
    /// <exception cref="EffectException"> When the Javascript is not valid. </exception>
    /// <returns> A <see cref="EffectException" /> that is ready to be used. </returns>
    public EffectEngine CreateEffectEngine(Effect effect, int ledstripLength, EffectEngineOptions? options = null)
    {
        return new EffectEngine(_loggerFactory.CreateLogger<EffectEngine>(), effect, ledstripLength, options ?? new EffectEngineOptions());
    }
}