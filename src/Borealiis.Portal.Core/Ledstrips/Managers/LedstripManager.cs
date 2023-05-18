using Borealis.Domain.Ledstrips;
using Borealis.Portal.Data.Stores;
using Borealis.Portal.Domain.Ledstrips.Managers;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Ledstrips.Managers;


internal class LedstripManager : ILedstripManager
{
    private readonly ILogger<LedstripManager> _logger;
    private readonly ILedstripStore _store;


    public LedstripManager(ILogger<LedstripManager> logger, ILedstripStore store)
    {
        _logger = logger;
        _store = store;
    }


    /// <inheritdoc />
    public virtual async Task<IEnumerable<Ledstrip>> GetLedstripsAsync(CancellationToken token = default)
    {
        return await _store.GetLedstripsAsync(token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task<IEnumerable<Ledstrip>> GetUnoccupiedLedstripsAsync(CancellationToken token = default)
    {
        return await _store.GetLedstripsAsync(token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task SaveAsync(Ledstrip ledstrip, CancellationToken token = default)
    {
        if (ledstrip.Id == Guid.Empty)
        {
            await _store.AddAsync(ledstrip, token).ConfigureAwait(false);
        }
        else
        {
            await _store.UpdateAsync(ledstrip, token).ConfigureAwait(false);
        }
    }


    /// <inheritdoc />
    public virtual async Task DeleteAsync(Ledstrip ledstrip, CancellationToken token = default)
    {
        await _store.RemoveAsync(ledstrip, token).ConfigureAwait(false);
    }
}