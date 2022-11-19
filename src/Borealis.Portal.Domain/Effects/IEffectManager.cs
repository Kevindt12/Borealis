using Borealis.Domain.Effects;



namespace Borealis.Portal.Domain.Effects;


public interface IEffectManager
{
	Task<IEnumerable<Effect>> GetEffectsAsync(CancellationToken token = default);

	Task SaveEffectAsync(Effect effect, CancellationToken token = default);
}