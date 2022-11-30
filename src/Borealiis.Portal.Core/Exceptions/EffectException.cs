using Borealis.Domain.Effects;



namespace Borealis.Portal.Core.Exceptions;


public class EffectException : ApplicationException
{
    public Effect Effect { get; }


    /// <inheritdoc />
    public EffectException() { }


    /// <inheritdoc />
    public EffectException(String? message) : base(message) { }


    /// <inheritdoc />
    public EffectException(String? message, Exception? innerException) : base(message, innerException) { }


    /// <inheritdoc />
    public EffectException(Effect effect)
    {
        Effect = effect;
    }


    /// <inheritdoc />
    public EffectException(String? message, Effect effect) : base(message)
    {
        Effect = effect;
    }


    /// <inheritdoc />
    public EffectException(String? message, Exception? innerException, Effect effect) : base(message, innerException)
    {
        Effect = effect;
    }
}