using Borealis.Portal.Data.Contexts;
using Borealis.Portal.Domain.Devices.Models;

using Microsoft.EntityFrameworkCore;



namespace Borealis.Portal.Data.Stores;


public class DeviceStore : IDeviceStore
{
    private readonly DbContext _context;
    private readonly DbSet<Device> _set;


    public DeviceStore(ApplicationDbContext context)
    {
        _context = context;
        _set = context.Set<Device>();
    }


    /// <inheritdoc />
    public virtual async Task<Device?> FindByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _set.Include(x => x.Ports).SingleOrDefaultAsync(d => d.Id == id, token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken token = default)
    {
        return await _set.Include(x => x.Ports).ToListAsync(token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task AddAsync(Device device, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        _set.Add(device);
        await _context.SaveChangesAsync(token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task UpdateAsync(Device device, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        _set.Update(device);
        await _context.SaveChangesAsync(token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task RemoveAsync(Device device, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        _set.Remove(device);
        await _context.SaveChangesAsync(token).ConfigureAwait(false);
    }
}