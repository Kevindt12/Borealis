using System;
using System.Linq;

using Borealis.Domain.Effects;



namespace Borealis.Portal.Domain.Exceptions;


public class EffectEngineRuntimeException : EffectEngineException
{
    public virtual string Code { get; init; }


    /// <inheritdoc />
    public EffectEngineRuntimeException() { }


    /// <inheritdoc />
    public EffectEngineRuntimeException(String? message) : base(message) { }


    /// <inheritdoc />
    public EffectEngineRuntimeException(String? message, Exception? innerException) : base(message, innerException) { }


    /// <inheritdoc />
    public EffectEngineRuntimeException(Effect effect) : base(effect) { }


    /// <inheritdoc />
    public EffectEngineRuntimeException(Effect effect, String? message) : base(effect, message) { }


    /// <inheritdoc />
    public EffectEngineRuntimeException(Effect effect, String? message, Exception? innerException) : base(effect, message, innerException) { }


    /// <inheritdoc />
    public EffectEngineRuntimeException(String code, Effect effect) : base(effect)
    {
        Code = code;
    }


    /// <inheritdoc />
    public EffectEngineRuntimeException(String code, Effect effect, String? message) : base(effect, message)
    {
        Code = code;
    }


    /// <inheritdoc />
    public EffectEngineRuntimeException(String code, Effect effect, String? message, Exception? innerException) : base(effect, message, innerException)
    {
        Code = code;
    }
}