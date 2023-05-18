using Borealis.Domain.Effects;
using Borealis.Portal.Domain.Connectivity.Connections;



namespace Borealis.Portal.Domain.Animations;


/// <summary>
/// The animation player factory used to create animation players.
/// </summary>
public interface IAnimationPlayerFactory
{
    /// <summary>
    /// Creates an animation player.
    /// </summary>
    /// <param name="animation"> The animation we want to play. </param>
    /// <param name="ledstrip"> The ledstrip we want to play it on. </param>
    /// <returns> an <see cref="IAnimationPlayer" /> that is ready to be used. </returns>
    Task<IAnimationPlayer> CreateAnimationPlayerAsync(Effect effect, ILedstripConnection ledstrip, CancellationToken token = default);
}