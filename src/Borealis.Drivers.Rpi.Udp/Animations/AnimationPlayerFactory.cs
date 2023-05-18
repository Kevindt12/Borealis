using System;
using System.Linq;

using Borealis.Drivers.Rpi.Commands;
using Borealis.Drivers.Rpi.Commands.Actions;
using Borealis.Drivers.Rpi.Ledstrips;
using Borealis.Drivers.Rpi.Options;

using Microsoft.Extensions.Options;



namespace Borealis.Drivers.Rpi.Animations;


public class AnimationPlayerFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<AnimationOptions> _animationOptions;


    public AnimationPlayerFactory(ILoggerFactory loggerFactory, IServiceProvider serviceProvider, IOptions<AnimationOptions> animationOptions)
    {
        _loggerFactory = loggerFactory;
        _serviceProvider = serviceProvider;
        _animationOptions = animationOptions;
    }


    /// <summary>
    /// Creates a new animation player to be used to show and play animations.
    /// </summary>
    /// <param name="ledstripProxy"> The ledstrip on where we want to play the animation on. </param>
    /// <returns> </returns>
    public AnimationPlayer CreateAnimationPlayer(LedstripProxyBase ledstripProxy)
    {
        return new AnimationPlayer(_loggerFactory.CreateLogger<AnimationPlayer>(), _serviceProvider.GetService<IQueryHandler<RequestFrameBufferCommand, FrameBufferQuery>>()!, _animationOptions, ledstripProxy);
    }
}