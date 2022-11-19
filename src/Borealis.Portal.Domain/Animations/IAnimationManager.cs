using Borealis.Domain.Animations;



namespace Borealis.Portal.Domain.Animations;


/// <summary>
/// The manager for the <see cref="Animation" /> aggregate
/// </summary>
public interface IAnimationManager
{
    /// <summary>
    /// Gets all the animations we know of.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> All the <see cref="Animation" /> we have. </returns>
    Task<IEnumerable<Animation>> GetAnimationsAsync(CancellationToken token = default);


    /// <summary>
    /// Saves a animation
    /// </summary>
    /// <remarks>
    /// If the <see cref="Animation.Id" /> is <see cref="Guid.Empty" /> then we will add the animation to the application.
    /// </remarks>
    /// <param name="animation"> The <see cref="Animation" /> we want to save. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task SaveAnimationAsync(Animation animation, CancellationToken token = default);


    /// <summary>
    /// Deletes a animation from the application.
    /// </summary>
    /// <param name="animation"> The animation we want to delete. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task DeleteAnimationAsync(Animation animation, CancellationToken token = default);
}