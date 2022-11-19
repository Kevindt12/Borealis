using System;
using System.Linq;

using Borealis.Domain.Animations;
using Borealis.Portal.Data.Contexts;
using Borealis.Portal.Domain.Animations;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Animations;


public class AnimationManager : IAnimationManager
{
    private readonly ILogger<AnimationManager> _logger;
    private readonly ApplicationDbContext _context;


    public AnimationManager(ILogger<AnimationManager> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }


    /// <inheritdoc />
    public async Task<IEnumerable<Animation>> GetAnimationsAsync(CancellationToken token = default)
    {
        return await _context.Animations.ToListAsync(token);
    }


    /// <inheritdoc />
    public async Task SaveAnimationAsync(Animation animation, CancellationToken token = default)
    {
        _logger.LogDebug($"Saving {animation.Name} to the database.");

        if (animation.Id == Guid.Empty)
        {
            _context.Animations.Add(animation);
        }
        else
        {
            _context.Animations.Update(animation);
        }

        await _context.SaveChangesAsync(token);
    }


    /// <inheritdoc />
    public async Task DeleteAnimationAsync(Animation animation, CancellationToken token = default)
    {
        _context.Animations.Remove(animation);

        await _context.SaveChangesAsync(token);
    }
}