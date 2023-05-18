using System;
using System.Linq;

using Borealis.Domain.Effects;



namespace Borealis.Portal.Domain.Exceptions;


public class EffectEngineException : ApplicationException
{
    public Effect Effect { get; set; }


    public EffectEngineException() { }


    public EffectEngineException(string? message) : base(message) { }


    public EffectEngineException(string? message, Exception? innerException) : base(message, innerException) { }


    /// <inheritdoc />
    public EffectEngineException(Effect effect)
    {
        Effect = effect;
    }


    /// <inheritdoc />
    public EffectEngineException(Effect effect, String? message) : base(message)
    {
        Effect = effect;
    }


    /// <inheritdoc />
    public EffectEngineException(Effect effect, String? message, Exception? innerException) : base(message, innerException)
    {
        Effect = effect;
    }
}