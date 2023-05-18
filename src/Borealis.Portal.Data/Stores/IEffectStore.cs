using Borealis.Domain.Effects;



namespace Borealis.Portal.Data.Stores;


public interface IEffectStore
{
    Task<Effect?> FindByIdAsync(Guid id, CancellationToken token = default);

    Task<IEnumerable<Effect>> GetEffectsAsync(CancellationToken token = default);

    Task AddAsync(Effect effect, CancellationToken token = default);

    Task UpdateAsync(Effect effect, CancellationToken token = default);

    Task RemoveAsync(Effect effect, CancellationToken token = default);
}