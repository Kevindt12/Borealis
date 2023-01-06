using System;
using System.Drawing;
using System.Linq;

using Borealis.Domain.Animations;
using Borealis.Domain.Devices;
using Borealis.Domain.Effects;
using Borealis.Portal.Core.Exceptions;
using Borealis.Portal.Domain.Animations;
using Borealis.Portal.Domain.Connections;
using Borealis.Portal.Domain.Devices;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Devices;


internal class LedstripService : ILedstripService
{
    private readonly ILogger<LedstripService> _logger;

    private readonly DeviceContext _deviceContext;
    private readonly AnimationContext _animationContext;
    private readonly IAnimationPlayerFactory _animationPlayerFactory;


    public LedstripService(ILogger<LedstripService> logger,
                           DeviceContext deviceContext,
                           AnimationContext animationContext,
                           IAnimationPlayerFactory animationPlayerFactory
    )
    {
        _logger = logger;
        _deviceContext = deviceContext;
        _animationContext = animationContext;
        _animationPlayerFactory = animationPlayerFactory;
    }


    /// <inheritdoc />
    public bool IsLedstripBusy(Ledstrip ledstrip)
    {
        return _animationContext.Players.Any(a => a.Ledstrip.Ledstrip == ledstrip);
    }


    /// <inheritdoc />
    public async Task StartAnimationOnLedstripAsync(Ledstrip ledstrip, Animation animation)
    {
        _logger.LogDebug($"Starting animation {animation.Id} on ledstrip {ledstrip.Name}");
        ILedstripConnection? connection = _deviceContext.LedstripConnections.SingleOrDefault(c => c.Ledstrip == ledstrip);

        // Making sure that we have a connection.
        if (connection is null) throw new DeviceException("Device  is not connected.");

        // Creating the player.
        IAnimationPlayer player = _animationPlayerFactory.CreateAnimationPlayer(animation, connection);

        // Starting and saving the player.
        await _animationContext.AddAnimationPlayerAsync(player);
    }


    /// <inheritdoc />
    public async Task StopLedstripAsync(Ledstrip ledstrip)
    {
        _logger.LogDebug($"Stopping ledstrip {ledstrip.Name}");
        ILedstripConnection connection = _deviceContext.LedstripConnections.SingleOrDefault(c => c.Ledstrip == ledstrip) ?? throw new DeviceException("Device is not connected.");

        // Creating the player.
        IAnimationPlayer? player;

        if ((player = _animationContext.GetAnimationPlayerForLedstripOrDefault(connection)) != null)
        {
            await player.StopAsync();
            await _animationContext.RemoveAnimationPlayerAsync(player);
        }

        // Clear the ledstrip.
        await connection.SetSingleFrameAsync(Enumerable.Repeat((PixelColor)Color.Black, ledstrip.Length).ToArray());
    }


    /// <inheritdoc />
    public async Task TestLedstripAsync(Ledstrip ledstrip)
    {
        throw new NotImplementedException();
    }


    /// <inheritdoc />
    public async Task SetSolidColorAsync(Ledstrip ledstrip, PixelColor color, CancellationToken token = default)
    {
        // Creating the player.
        ILedstripConnection connection = _deviceContext.LedstripConnections.SingleOrDefault(c => c.Ledstrip == ledstrip) ?? throw new DeviceException("Device is not connected.");

        // Setting the ledstrip color.
        await connection.SetSingleFrameAsync(Enumerable.Repeat(color, ledstrip.Length).ToArray(), token);
    }


    /// <inheritdoc />
    public Animation? GetAnimationPlayingOnLedstripOrDefault(Ledstrip ledstrip)
    {
        IAnimationPlayer? player = _animationContext.Players.SingleOrDefault(x => x.Ledstrip.Ledstrip == ledstrip);

        if (player != null)
        {
            return player.Animation;
        }

        return null;
    }


    private byte[] SerializeColorArray(Color[] colors)
    {
        byte[] result = new byte[colors.Length * 3];

        for (int i = 0,
                 c = 0; i < result.Length; c++)
        {
            result[i] = colors[c].R;
            result[i + 1] = colors[c].G;
            result[i + 2] = colors[c].B;

            i += 3;
        }

        return result;
    }
}