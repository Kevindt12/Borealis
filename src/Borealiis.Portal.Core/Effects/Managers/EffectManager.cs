using System;
using System.Linq;

using Borealis.Domain.Effects;
using Borealis.Portal.Data.Stores;
using Borealis.Portal.Domain.Effects.Managers;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Effects.Managers;


internal class EffectManager : IEffectManager
{
    private readonly ILogger<EffectManager> _logger;
    private readonly IEffectStore _store;


    public EffectManager(ILogger<EffectManager> logger, IEffectStore store)
    {
        _logger = logger;
        _store = store;
    }


    /// <inheritdoc />
    public async Task<IEnumerable<Effect>> GetEffectsAsync(CancellationToken token = default)
    {
        return await _store.GetEffectsAsync(token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public async Task<Effect?> GetEffectByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _store.FindByIdAsync(id, token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public async Task SaveEffectAsync(Effect effect, CancellationToken token = default)
    {
        _logger.LogDebug($"Saving {effect.Name} to the database.");

        if (effect.Id == Guid.Empty)
        {
            await _store.AddAsync(effect, token).ConfigureAwait(false);
        }
        else
        {
            await _store.UpdateAsync(effect, token).ConfigureAwait(false);
        }
    }


    /// <inheritdoc />
    public async Task DeleteEffectAsync(Effect effect, CancellationToken token = default)
    {
        await _store.RemoveAsync(effect, token).ConfigureAwait(false);
    }
}