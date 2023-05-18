using Borealis.Domain.Ledstrips;
using Borealis.Portal.Data.Contexts;

using Microsoft.EntityFrameworkCore;



namespace Borealis.Portal.Data.Stores;


public class LedstripStore : ILedstripStore
{
    private readonly DbContext _context;
    private readonly DbSet<Ledstrip> _set;


    public LedstripStore(ApplicationDbContext context)
    {
        _context = context;
        _set = context.Set<Ledstrip>();
    }


    /// <inheritdoc />
    public virtual async Task<Ledstrip?> FindByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _set.SingleOrDefaultAsync(d => d.Id == id, token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task<IEnumerable<Ledstrip>> GetLedstripsAsync(CancellationToken token = default)
    {
        return await _set.ToListAsync(token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task AddAsync(Ledstrip ledstrip, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        _set.Add(ledstrip);
        await _context.SaveChangesAsync(token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task UpdateAsync(Ledstrip ledstrip, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        _set.Update(ledstrip);
        await _context.SaveChangesAsync(token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task RemoveAsync(Ledstrip ledstrip, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        _set.Remove(ledstrip);
        await _context.SaveChangesAsync(token).ConfigureAwait(false);
    }
}