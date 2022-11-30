using System;
using System.Linq;

using Borealis.Portal.Domain.Animations;
using Borealis.Portal.Domain.Connections;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Devices;


internal class AnimationContext : IDisposable, IAsyncDisposable
{
    private readonly ILogger<AnimationContext> _logger;

    private readonly List<IAnimationPlayer> _players;


    public IReadOnlyList<IAnimationPlayer> Players => _players.AsReadOnly();


    public AnimationContext(ILogger<AnimationContext> logger)
    {
        _logger = logger;
        _players = new List<IAnimationPlayer>();
    }


    public virtual IAnimationPlayer? GetAnimationPlayerForLedstripOrDefault(ILedstripConnection ledstrip)
    {
        return _players.SingleOrDefault(x => x.Ledstrip == ledstrip);
    }


    public virtual async Task RemoveAllAnimationPlayersFromDevice(IDeviceConnection deviceConnection)
    {
        IEnumerable<IAnimationPlayer> toBeRemoved = _players.Where(x => deviceConnection.LedstripConnections.Contains(x.Ledstrip)).ToList();

        foreach (IAnimationPlayer player in toBeRemoved)
        {
            _logger.LogTrace($"Stopping animation player {player.Ledstrip.Ledstrip.Name}, {player.Ledstrip.Ledstrip.Name}.");
            await RemoveAnimationPlayerAsync(player);
        }
    }


    public async Task AddAnimationPlayerAsync(IAnimationPlayer player)
    {
        await player.StartAsync();
        _players.Add(player);
    }


    public async Task RemoveAnimationPlayerAsync(IAnimationPlayer player)
    {
        await player.DisposeAsync();
        _players.Remove(player);
    }


    private bool _disposed;


    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed) return;

        Dispose(true);
        GC.SuppressFinalize(this);
    }


    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (IAnimationPlayer player in _players)
            {
                player.Dispose();
            }
        }
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (!_disposed) return;

        await DisposeAsyncCore();

        Dispose(false);
        GC.SuppressFinalize(this);
    }


    protected virtual async ValueTask DisposeAsyncCore()
    {
        foreach (IAnimationPlayer player in _players)
        {
            await player.DisposeAsync();
        }
    }
}