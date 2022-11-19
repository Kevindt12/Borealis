using System;
using System.Linq;

using Borealis.Domain.Effects;
using Borealis.Portal.Data.Contexts;
using Borealis.Portal.Domain.Effects;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Effects;


public class EffectManager : IEffectManager
{
    private readonly ILogger<EffectManager> _logger;
    private readonly ApplicationDbContext _context;


    public EffectManager(ILogger<EffectManager> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }


    /// <inheritdoc />
    public async Task<IEnumerable<Effect>> GetEffectsAsync(CancellationToken token = default)
    {
        _logger.LogDebug("Getting all effects from the application.");

        return await _context.Effects.ToListAsync(token);
    }


    /// <inheritdoc />
    public async Task SaveEffectAsync(Effect effect, CancellationToken token = default)
    {
        _logger.LogDebug($"Saving {effect.Name} to the database.");

        if (effect.Id == Guid.Empty)
        {
            _context.Effects.Add(effect);
        }
        else
        {
            _context.Effects.Update(effect);
        }

        await _context.SaveChangesAsync(token);
    }
}