using Borealis.Domain.Effects;



namespace Borealis.Portal.Core.Exceptions;


public class EffectEngineException : ApplicationException
{
    public Effect Effect { get; set; }

    public string Code { get; set; }


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


    /// <inheritdoc />
    public EffectEngineException(String code, Effect effect)
    {
        Effect = effect;
        Code = code;
    }


    /// <inheritdoc />
    public EffectEngineException(String code, Effect effect, String? message) : base(message)
    {
        Effect = effect;
        Code = code;
    }


    /// <inheritdoc />
    public EffectEngineException(String code, Effect effect, String? message, Exception? innerException) : base(message, innerException)
    {
        Effect = effect;
        Code = code;
    }
}