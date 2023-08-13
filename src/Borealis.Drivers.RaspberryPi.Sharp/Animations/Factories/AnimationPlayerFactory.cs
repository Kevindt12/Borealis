using System;
using System.Linq;

using Borealis.Drivers.RaspberryPi.Sharp.Animations.Core;
using Borealis.Drivers.RaspberryPi.Sharp.Animations.Options;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Services;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Proxies;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace Borealis.Drivers.RaspberryPi.Sharp.Animations.Factories;


public class AnimationPlayerFactory : IAnimationPlayerFactory
{
	private readonly ILoggerFactory _loggerFactory;
	private readonly IOptions<AnimationOptions> _animationOptions;
	private readonly IConnectionService _connectionService;


	public AnimationPlayerFactory(ILoggerFactory loggerFactory, IOptions<AnimationOptions> animationOptions, IConnectionService connectionService)
	{
		_loggerFactory = loggerFactory;
		_animationOptions = animationOptions;
		_connectionService = connectionService;
	}


	/// <summary>
	/// Creates a new animation player to be used to show and play animations.
	/// </summary>
	/// <param name="ledstripProxyBase"> The ledstrip on where we want to play the animation on. </param>
	/// <returns> A <see cref="AnimationPlayer" /> that is ready to start playing animations on ledstrips. </returns>
	public virtual IAnimationPlayer CreateAnimationPlayer(LedstripProxyBase ledstripProxyBase)
	{
		AnimationPlayer player = new AnimationPlayer(_loggerFactory.CreateLogger<AnimationPlayer>(), _animationOptions, _connectionService, ledstripProxyBase);

		return player;
	}
}