using Borealis.Domain.Effects;



namespace Borealis.Domain.Animations;


public class AnimationEffect : Effect
{
    /// <summary>
    /// The frequency of the animation that we want to play.
    /// </summary>
    public int Frequency { get; set; }


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