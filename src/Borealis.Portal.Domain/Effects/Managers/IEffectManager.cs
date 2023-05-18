using Borealis.Domain.Effects;



namespace Borealis.Portal.Domain.Effects.Managers;


/// <summary>
/// Managers all the <see cref="Effect" /> stored in the application.
/// </summary>
public interface IEffectManager
{
    /// <summary>
    /// Gets all the effects that we have stored in the application.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> A <see cref="IEnumerable{T}" /> of <see cref="Effect" /> that we have stored. </returns>
    Task<IEnumerable<Effect>> GetEffectsAsync(CancellationToken token = default);


    /// <summary>
    /// Finds a single effect by its id.
    /// </summary>
    /// <param name="id"> The id of the effect. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> A <see cref="Effect" /> that we have stored or <c> null </c> if no effect was found. </returns>
    Task<Effect?> GetEffectByIdAsync(Guid id, CancellationToken token = default);


    /// <summary>
    /// Saves the effect.
    /// </summary>
    /// <param name="effect"> The effect that we want to save or create. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task SaveEffectAsync(Effect effect, CancellationToken token = default);


    /// <summary>
    /// Deletes the effect that was stored.
    /// </summary>
    /// <param name="effect"> The effect that we want to delete. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task DeleteEffectAsync(Effect effect, CancellationToken token = default);
}