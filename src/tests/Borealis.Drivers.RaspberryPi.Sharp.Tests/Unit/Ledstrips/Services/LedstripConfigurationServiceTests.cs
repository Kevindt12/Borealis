using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Contexts;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Factories;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Proxies;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Service;
using Borealis.Drivers.RaspberryPi.Sharp.Tests.Unit.Helpers;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using MicrosoftOptions = Microsoft.Extensions.Options.Options;



namespace Borealis.Drivers.RaspberryPi.Sharp.Tests.Unit.Ledstrips.Services;


public class LedstripConfigurationServiceTests
{
    private readonly Mock<ILogger<LedstripService>> _loggerMock;
    private readonly Mock<ILedstripProxyFactory> _ledstripProxyFactoryMock;
    private readonly Mock<LedstripStateFactory> _ledstripStateFactoryMock;
    private readonly Mock<LedstripContext> _ledstripContextMock;

    private readonly ILedstripConfigurationService _ledstripConfigurationService;


    public LedstripConfigurationServiceTests()
    {
        _loggerMock = new Mock<ILogger<LedstripService>>();

        _ledstripStateFactoryMock = MockFactoryHelpers.CreateLedstripStateFactoryMock();
        _ledstripContextMock = new Mock<LedstripContext>();

        _ledstripProxyFactoryMock = new Mock<ILedstripProxyFactory>();

        _ledstripConfigurationService = new LedstripService(_loggerMock.Object,
                                                            _ledstripContextMock.Object,
                                                            _ledstripStateFactoryMock.Object,
                                                            _ledstripProxyFactoryMock.Object
                                                           );
    }


    [Fact]
    public void CanLoadConfiguration_ShouldReturnTrue_WhenNoLedstripHaveAnimations()
    {
        // Arrange
        _ledstripContextMock.Setup(x => x.HasAnimations()).Returns(false);

        // Act
        bool result = _ledstripConfigurationService.CanLoadConfiguration();

        // Assert
        Assert.True(result);
    }


    [Fact]
    public void CanLoadConfiguration_CheckingAnimationAreRunning_ReturnFalseIfNotRunning()
    {
        // Arrange
        _ledstripContextMock.Setup(x => x.HasAnimations()).Returns(true);

        // Act
        bool result = _ledstripConfigurationService.CanLoadConfiguration();

        // Assert
        Assert.False(result);
    }


    [Fact]
    public async Task LoadConfigurationAsync_LoadWhileEmptyConfiguration_ShouldLoadConfigurationDirectly()
    {
        // Create
        DeviceConfiguration deviceConfiguration = new DeviceConfiguration
        {
            Ledstrips = new List<Ledstrip>
            {
                new Ledstrip()
            }
        };

        // Arrange
        Mock<LedstripState> ledstripStateMock = new Mock<LedstripState>(MockFactoryHelpers.CreateAnimationPlayerFactoryMock().Object, NullLedstripProxy.Instance);

        _ledstripContextMock.Setup(x => x.HasAnimations()).Returns(false);
        _ledstripContextMock.Setup(x => x.IsEmpty()).Returns(true);
        _ledstripProxyFactoryMock.Setup(x => x.CreateLedstripProxy(It.IsAny<Ledstrip>())).Returns(NullLedstripProxy.Instance);
        _ledstripStateFactoryMock.Setup(x => x.Create(It.IsAny<LedstripProxyBase>())).Returns(ledstripStateMock.Object);

        // Act
        await _ledstripConfigurationService.LoadConfigurationAsync(deviceConfiguration);

        // Assert
        _ledstripContextMock.Verify(x => x.Clear(), Times.Never);
        _ledstripContextMock.Verify(x => x.LoadLedstrips(It.IsAny<IEnumerable<LedstripState>>()), Times.Once);
    }


    [Fact]
    public async Task LoadConfigurationAsync_LoadWhileLedstripsActive_ClearLedstripsAndLoadThenNewConfiguration()
    {
        // Create
        DeviceConfiguration deviceConfiguration = new DeviceConfiguration
        {
            Ledstrips = new List<Ledstrip>
            {
                new Ledstrip()
            }
        };

        // Arrange
        Mock<LedstripState> ledstripStateMock = new Mock<LedstripState>(MockFactoryHelpers.CreateAnimationPlayerFactoryMock().Object, NullLedstripProxy.Instance);

        _ledstripContextMock.Setup(x => x.HasAnimations()).Returns(false);
        _ledstripContextMock.Setup(x => x.IsEmpty()).Returns(false);

        _ledstripProxyFactoryMock.Setup(x => x.CreateLedstripProxy(It.IsAny<Ledstrip>())).Returns(NullLedstripProxy.Instance);
        _ledstripStateFactoryMock.Setup(x => x.Create(It.IsAny<LedstripProxyBase>())).Returns(ledstripStateMock.Object);

        // Act
        await _ledstripConfigurationService.LoadConfigurationAsync(deviceConfiguration);

        // Assert
        _ledstripContextMock.Verify(x => x.Clear(), Times.Once);
        _ledstripContextMock.Verify(x => x.LoadLedstrips(It.IsAny<IEnumerable<LedstripState>>()), Times.Once);
    }


    [Fact]
    public async Task LoadConfigurationAsync_ThrowsException_BecauseAnimationsAreRunning()
    {
        // Create
        DeviceConfiguration deviceConfiguration = new DeviceConfiguration
        {
            Ledstrips = new List<Ledstrip>
            {
                new Ledstrip()
            }
        };

        // Arrange
        _ledstripContextMock.Setup(x => x.HasAnimations()).Returns(true);

        // Act And Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _ledstripConfigurationService.LoadConfigurationAsync(deviceConfiguration));

        _ledstripContextMock.Verify(x => x.Clear(), Times.Never);
        _ledstripContextMock.Verify(x => x.LoadLedstrips(It.IsAny<IEnumerable<LedstripState>>()), Times.Never);
    }
}