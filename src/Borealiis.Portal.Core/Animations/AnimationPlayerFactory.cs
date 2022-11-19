using System;
using System.Linq;

using Borealis.Domain.Animations;
using Borealis.Domain.Devices;
using Borealis.Portal.Infrastructure.Connections;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Core.Animations;


internal class AnimationPlayerFactory
{
    private readonly ILoggerFactory _loggerFactory;


    public AnimationPlayerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }


    /// <summary>
    /// Creates a new animation player to be used to play animations.
    /// </summary>
    /// <exception cref="InvalidOperationException"> If the javascript is not valid. </exception>
    /// <param name="deviceConnection"> The connection to the device where would want to play the animation. </param>
    /// <param name="animation"> The animation we want to play. </param>
    /// <param name="ledstrip"> The ledstrip we want to play it on. </param>
    /// <returns> A new animation player that can be used. </returns>
    public AnimationPlayer CreateAnimationPlayer(IDeviceConnection deviceConnection, Animation animation, Ledstrip ledstrip)
    {
        return new AnimationPlayer(_loggerFactory.CreateLogger<AnimationPlayer>(), animation, deviceConnection, ledstrip);
    }
}