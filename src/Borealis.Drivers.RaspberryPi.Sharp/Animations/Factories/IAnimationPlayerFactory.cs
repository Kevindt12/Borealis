using Borealis.Drivers.RaspberryPi.Sharp.Animations.Core;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Proxies;



namespace Borealis.Drivers.RaspberryPi.Sharp.Animations.Factories;


public interface IAnimationPlayerFactory
{
	IAnimationPlayer CreateAnimationPlayer(LedstripProxyBase ledstripProxy);
}