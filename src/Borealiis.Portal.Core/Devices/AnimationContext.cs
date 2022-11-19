using System;
using System.Linq;

using Borealis.Portal.Core.Animations;
using Borealis.Portal.Core.Interaction;
using Borealis.Portal.Domain.Devices;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Devices;


internal class LedstripContext : IDisposable
{
    private readonly ILogger<LedstripContext> _logger;

    private readonly List<LedstripInteractorBase> _players;


    public IReadOnlyList<LedstripInteractorBase> Interactors => _players.AsReadOnly();


    public LedstripContext(ILogger<LedstripContext> logger)
    {
        _logger = logger;
        _players = new List<LedstripInteractorBase>();
    }


    public IEnumerable<LedstripInteractorBase> GetInteractorsFromDevice(Device device)
    {
        return _players.Where(p => p.Device == device);
    }


    public async Task AddInteractorAndStartAsync(LedstripInteractorBase player)
    {
        await player.StartAsync();
        _players.Add(player);
    }


    public async Task RemoveAndStopInteractorAsync(LedstripInteractorBase player)
    {
        await player.DisposeAsync();
        _players.Remove(player);
    }


    /// <inheritdoc />
    public void Dispose()
    {
        foreach (AnimationPlayer animationPlayer in _players) { }
    }
}