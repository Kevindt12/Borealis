using Borealis.Domain.Effects;



namespace Borealis.Domain.Animations;


/// <summary>
/// This is a copy of a effect.
/// </summary>
public class AnimationEffect : Effect
{
    /// <summary>
    /// The effect that is attached toa animation.
    /// </summary>
    public AnimationEffect() { }


    /// <summary>
    /// The effect that is attached toa animation.
    /// </summary>
    /// <param name="effect"> The effect we want ot copy from. </param>
    public AnimationEffect(Effect effect)
    {
        Id = Guid.Empty;

        Javascript = effect.Javascript;
        Name = effect.Name;
        Description = effect.Description;
    }
}