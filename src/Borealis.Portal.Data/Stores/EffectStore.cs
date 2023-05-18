using Borealis.Domain.Effects;
using Borealis.Portal.Data.Contexts;

using Microsoft.EntityFrameworkCore;



namespace Borealis.Portal.Data.Stores;


public class EffectStore : IEffectStore
{
    private readonly DbContext _context;
    private readonly DbSet<Effect> _set;


    public EffectStore(ApplicationDbContext context)
    {
        _context = context;
        _set = context.Set<Effect>();
    }


    /// <inheritdoc />
    public virtual async Task<Effect?> FindByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _set.Include(x => x.Files).SingleOrDefaultAsync(d => d.Id == id, token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task<IEnumerable<Effect>> GetEffectsAsync(CancellationToken token = default)
    {
        return await _set.Include(x => x.Files).ToListAsync(token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task AddAsync(Effect effect, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        _set.Add(effect);
        await _context.SaveChangesAsync(token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task UpdateAsync(Effect effect, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        _set.Update(effect);
        await _context.SaveChangesAsync(token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task RemoveAsync(Effect effect, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        _set.Remove(effect);
        await _context.SaveChangesAsync(token).ConfigureAwait(false);
    }
}