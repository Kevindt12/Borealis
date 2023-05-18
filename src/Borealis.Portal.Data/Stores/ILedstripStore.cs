using Borealis.Domain.Ledstrips;



namespace Borealis.Portal.Data.Stores;


public interface ILedstripStore
{
    Task<Ledstrip?> FindByIdAsync(Guid id, CancellationToken token = default);

    Task<IEnumerable<Ledstrip>> GetLedstripsAsync(CancellationToken token = default);

    Task AddAsync(Ledstrip ledstrip, CancellationToken token = default);

    Task UpdateAsync(Ledstrip ledstrip, CancellationToken token = default);

    Task RemoveAsync(Ledstrip ledstrip, CancellationToken token = default);
}