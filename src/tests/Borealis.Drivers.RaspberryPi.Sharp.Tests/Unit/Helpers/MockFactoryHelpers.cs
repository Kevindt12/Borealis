using Borealis.Drivers.RaspberryPi.Sharp.Animations.Factories;
using Borealis.Drivers.RaspberryPi.Sharp.Animations.Options;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Services;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Factories;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;



namespace Borealis.Drivers.RaspberryPi.Sharp.Tests.Unit.Helpers;


public class MockFactoryHelpers
{
    public static Mock<LedstripStateFactory> CreateLedstripStateFactoryMock()
    {
        Mock<AnimationPlayerFactory> animationPlayerFactoryMock = new Mock<AnimationPlayerFactory>(Mock.Of<ILoggerFactory>(), Mock.Of<IOptions<AnimationOptions>>(), Mock.Of<IConnectionService>());

        return new Mock<LedstripStateFactory>(animationPlayerFactoryMock.Object);
    }


    public static Mock<AnimationPlayerFactory> CreateAnimationPlayerFactoryMock()
    {
        return new Mock<AnimationPlayerFactory>(Mock.Of<ILoggerFactory>(), Mock.Of<IOptions<AnimationOptions>>(), Mock.Of<IConnectionService>());
    }
}