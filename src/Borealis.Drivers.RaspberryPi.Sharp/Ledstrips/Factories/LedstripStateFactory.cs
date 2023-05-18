using Borealis.Drivers.RaspberryPi.Sharp.Animations.Factories;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Contexts;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Proxies;



namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Factories;


public class LedstripStateFactory
{
    private readonly AnimationPlayerFactory _animationPlayerFactory;


    public LedstripStateFactory(AnimationPlayerFactory animationPlayerFactory)
    {
        _animationPlayerFactory = animationPlayerFactory;
    }


    public virtual LedstripState Create(LedstripProxyBase ledstripProxy)
    {
        return new LedstripState(_animationPlayerFactory, ledstripProxy);
    }
}