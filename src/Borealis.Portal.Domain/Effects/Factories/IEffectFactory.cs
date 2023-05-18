using Borealis.Domain.Effects;



namespace Borealis.Portal.Domain.Effects.Factories;


/// <summary>
/// The factory that is responsible for creating effects.
/// </summary>
public interface IEffectFactory
{
    /// <summary>
    /// Creates a new effect that can be used.
    /// </summary>
    /// <param name="effectName"> The effect name. </param>
    /// <returns> A <see cref="Effect" /> that is ready to be programmed. </returns>
    Effect CreateEffect(string effectName);
}