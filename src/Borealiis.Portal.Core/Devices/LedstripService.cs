using System;
using System.Drawing;
using System.Linq;

using Borealis.Domain.Animations;
using Borealis.Domain.Devices;
using Borealis.Domain.Effects;
using Borealis.Portal.Core.Animations;
using Borealis.Portal.Core.Exceptions;
using Borealis.Portal.Core.Interaction;
using Borealis.Portal.Domain.Devices;
using Borealis.Portal.Infrastructure.Connections;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Devices;


internal class LedstripService : ILedstripService
{
    private readonly ILogger<LedstripService> _logger;

    private readonly DeviceContext _deviceContext;
    private readonly LedstripContext _ledstripContext;
    private readonly AnimationPlayerFactory _animationPlayerFactory;
    private readonly SolidColorInteractorFactory _solidColorInteractorFactory;


    public LedstripService(ILogger<LedstripService> logger,
                           DeviceContext deviceContext,
                           LedstripContext ledstripContext,
                           AnimationPlayerFactory animationPlayerFactory,
                           SolidColorInteractorFactory solidColorInteractorFactory
    )
    {
        _logger = logger;
        _deviceContext = deviceContext;
        _ledstripContext = ledstripContext;
        _animationPlayerFactory = animationPlayerFactory;
        _solidColorInteractorFactory = solidColorInteractorFactory;
    }


    /// <inheritdoc />
    public bool IsLedstripBusy(Device device, Ledstrip ledstrip)
    {
        return _ledstripContext.Interactors.Any(a => a.Device == device);
    }


    /// <inheritdoc />
    public async Task StartAnimationOnLedstripAsync(Device device, Ledstrip ledstrip, Animation animation)
    {
        _logger.LogDebug($"Starting animation {animation.Id} on ledstrip {ledstrip.Name}");
        IDeviceConnection? connection = _deviceContext.Connections.SingleOrDefault(c => c.Device == device);

        // Making sure that we have a connection.
        if (connection is null) throw new DeviceException($"Device {device.Name} is not connected.", device);

        // Creating the player.
        AnimationPlayer player = _animationPlayerFactory.CreateAnimationPlayer(connection, animation, ledstrip);

        // Starting and saving the player.
        await _ledstripContext.AddInteractorAndStartAsync(player);
    }


    /// <inheritdoc />
    public async Task StopLedstripAsync(Device device, Ledstrip ledstrip)
    {
        LedstripInteractorBase interactor = _ledstripContext.Interactors.SingleOrDefault(p => p.Device == device && p.Ledstrip == ledstrip) ??
                                            throw new AnimationException($"There was no animation playing on device {device.Name}, ledstrip {ledstrip.Name}.");

        await _ledstripContext.RemoveAndStopInteractorAsync(interactor);
    }


    /// <inheritdoc />
    public async Task TestLedstripAsync(Device device, Ledstrip ledstrip)
    {
        IDeviceConnection? connection = _deviceContext.Connections.SingleOrDefault(c => c.Device == device);

        // Making sure that we have a connection.
        if (connection is null) throw new DeviceException($"Device {device.Name} is not connected.", device);

        //// TODO: Switch this to a interactor
        //await connection.SendFrameAsync(new LedstripFrame
        //                                    { LedstripIndex = 0, Colors = Enumerable.Repeat(Color.Red.ToPixelColor(), ledstrip.Length).ToArray() });
    }


    /// <inheritdoc />
    public async Task SetSolidColorAsync(Device device, Ledstrip ledstrip, PixelColor color, CancellationToken token = default)
    {
        IDeviceConnection? connection = _deviceContext.Connections.SingleOrDefault(c => c.Device == device);

        // Making sure that we have a connection.
        if (connection is null) throw new DeviceException($"Device {device.Name} is not connected.", device);

        // Creating the player.
        SolidColorInteractor interactor = _solidColorInteractorFactory.CreateSolidColorInteractor(connection, ledstrip, color);

        // Starting and saving the player.
        await _ledstripContext.AddInteractorAndStartAsync(interactor);
    }


    /// <inheritdoc />
    public Animation? GetAnimationPlayingOnLedstripOrDefault(Device device, Ledstrip ledstrip)
    {
        LedstripInteractorBase? interactor = _ledstripContext.Interactors.SingleOrDefault(x => x.Device == device && x.Ledstrip == ledstrip);

        if (interactor is AnimationPlayer player)
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