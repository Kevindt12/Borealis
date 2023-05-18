using System;
using System.Linq;

using Borealis.Domain.Effects;
using Borealis.Portal.Domain.Effects.Handlers;
using Borealis.Portal.Domain.Exceptions;



namespace Borealis.Portal.Domain.Effects.Factories;


public interface IEffectEngineFactory
{
    /// <summary>
    /// Creates a new effect engine.
    /// </summary>
    /// <param name="effect"> The effect that we want to create a engine around. </param>
    /// <param name="ledstripLength"> The length of the ledstrip. </param>
    /// <param name="options"> The optional parameters that we can send with the engine. </param>
    /// <exception cref="ArgumentNullException"> If the Javascript is empty. </exception>
    /// <exception cref="EffectEngineException"> When the Javascript is not valid. </exception>
    /// <exception cref="EffectEngineRuntimeException"> When there is a problem with running any javascript code or any problems with the underlying engine. </exception>
    Task<IEffectEngine> CreateEffectEngineAsync(Effect effect, int ledstripLength, EffectEngineOptions? options = null, CancellationToken token = default);
}