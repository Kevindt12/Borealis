using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Exceptions;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Contexts;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Factories;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Service;
using Borealis.Drivers.RaspberryPi.Sharp.Tests.Unit.Helpers;

using Microsoft.Extensions.Logging;

using Moq;

using UnitsNet;

using Xunit;



namespace Borealis.Drivers.RaspberryPi.Sharp.Tests.Unit.Ledstrips.Services;


public class LedstripControlServiceTests
{
    private readonly Mock<ILogger<LedstripService>> _loggerMock;
    private readonly Mock<ILedstripProxyFactory> _ledstripProxyFactoryMock;
    private readonly Mock<LedstripStateFactory> _ledstripStateFactoryMock;
    private readonly Mock<LedstripContext> _ledstripContextMock;

    private readonly ILedstripControlService _ledstripControlService;


    public LedstripControlServiceTests()
    {
        _loggerMock = new Mock<ILogger<LedstripService>>();

        _ledstripStateFactoryMock = MockFactoryHelpers.CreateLedstripStateFactoryMock();
        _ledstripContextMock = new Mock<LedstripContext>();

        _ledstripProxyFactoryMock = new Mock<ILedstripProxyFactory>();

        _ledstripControlService = new LedstripService(_loggerMock.Object,
                                                      _ledstripContextMock.Object,
                                                      _ledstripStateFactoryMock.Object,
                                                      _ledstripProxyFactoryMock.Object
                                                     );
    }


    [Fact]
    public async Task StartAnimationAsync_InvokesLedstripStateStartAnimation_WhenValidParametersArePassed()
    {
        // Create
        Ledstrip ledstrip = new Ledstrip { Id = Guid.NewGuid() };
        Frequency frequency = Frequency.FromHertz(20);
        ReadOnlyMemory<PixelColor>[] initialFrameBuffer = new ReadOnlyMemory<PixelColor>[1];
        CancellationToken token = CancellationToken.None;

        // Arrange
        Mock<LedstripState> ledstripStateMock = new Mock<LedstripState>(MockFactoryHelpers.CreateAnimationPlayerFactoryMock().Object, NullLedstripProxy.Instance);
        ledstripStateMock.Setup(state => state.StartAnimationAsync(It.IsAny<Frequency>(), It.IsAny<ReadOnlyMemory<PixelColor>[]>(), token));

        _ledstripContextMock.Setup(x => x.GetLedstripStateById(It.IsAny<Guid>())).Returns(ledstripStateMock.Object);

        // Act
        await _ledstripControlService.StartAnimationAsync(ledstrip, frequency, initialFrameBuffer, token);

        // Assert
        ledstripStateMock.Verify(x => x.StartAnimationAsync(It.IsAny<Frequency>(), It.IsAny<ReadOnlyMemory<PixelColor>[]>(), token), Times.Once);
    }


    [Fact]
    public async Task StartAnimationAsync_ThrowsExceptionWhenLedstripCannotBeFound_InvalidLedstripIdPassed()
    {
        // Create
        Ledstrip ledstrip = new Ledstrip { Id = Guid.NewGuid() };
        Frequency frequency = Frequency.FromHertz(20);
        ReadOnlyMemory<PixelColor>[] initialFrameBuffer = new ReadOnlyMemory<PixelColor>[1];
        CancellationToken token = CancellationToken.None;

        // Arrange
        _ledstripContextMock.Setup(x => x.GetLedstripStateById(It.IsAny<Guid>())).Returns<LedstripState>(null);

        // Act and Assert
        await Assert.ThrowsAsync<LedstripNotFoundException>(() => _ledstripControlService.StartAnimationAsync(ledstrip, frequency, initialFrameBuffer, token));
    }


    [Fact]
    public async Task PauseAnimationAsync_InvokesStopAnimation_WhenValidParametersArePassed()
    {
        // Create
        Ledstrip ledstrip = new Ledstrip { Id = Guid.NewGuid() };
        CancellationToken token = CancellationToken.None;

        // Arrange
        Mock<LedstripState> ledstripStateMock = new Mock<LedstripState>(MockFactoryHelpers.CreateAnimationPlayerFactoryMock().Object, NullLedstripProxy.Instance);
        ledstripStateMock.Setup(state => state.HasAnimation()).Returns(true);

        _ledstripContextMock.Setup(x => x.GetLedstripStateById(It.IsAny<Guid>())).Returns(ledstripStateMock.Object);

        // Act
        await _ledstripControlService.PauseAnimationAsync(ledstrip, token);

        // Assert
        ledstripStateMock.Verify(x => x.PauseAnimationAsync(token), Times.Once);
    }


    [Fact]
    public async Task PauseAnimationAsync_ThrowsExceptionWhenLedstripCannotBeFound_InvalidLedstripIdPassed()
    {
        // Create
        Ledstrip ledstrip = new Ledstrip { Id = Guid.NewGuid() };
        CancellationToken token = CancellationToken.None;

        // Arrange
        _ledstripContextMock.Setup(x => x.GetLedstripStateById(It.IsAny<Guid>())).Returns<LedstripState>(null);

        // Act and Assert
        await Assert.ThrowsAsync<LedstripNotFoundException>(() => _ledstripControlService.PauseAnimationAsync(ledstrip, token));
    }


    [Fact]
    public async Task StopAnimationAsync_InvokesClearAnimationPlayerOnTheLedstripState_WhenValidParametersArePassed()
    {
        // Create
        Ledstrip ledstrip = new Ledstrip { Id = Guid.NewGuid() };
        CancellationToken token = CancellationToken.None;

        // Arrange
        Mock<LedstripState> ledstripStateMock = new Mock<LedstripState>(MockFactoryHelpers.CreateAnimationPlayerFactoryMock().Object, NullLedstripProxy.Instance);
        ledstripStateMock.Setup(state => state.HasAnimation()).Returns(true);

        _ledstripContextMock.Setup(x => x.GetLedstripStateById(It.IsAny<Guid>())).Returns(ledstripStateMock.Object);

        // Act
        await _ledstripControlService.PauseAnimationAsync(ledstrip, token);

        // Assert
        ledstripStateMock.Verify(x => x.PauseAnimationAsync(token), Times.Once);
    }


    [Fact]
    public async Task StopAnimationAsync_ThrowsExceptionWhenLedstripCannotBeFound_InvalidLedstripIdPassed()
    {
        // Create
        Ledstrip ledstrip = new Ledstrip { Id = Guid.NewGuid() };
        CancellationToken token = CancellationToken.None;

        // Arrange
        _ledstripContextMock.Setup(x => x.GetLedstripStateById(It.IsAny<Guid>())).Returns<LedstripState>(null);

        // Act and Assert
        await Assert.ThrowsAsync<LedstripNotFoundException>(() => _ledstripControlService.PauseAnimationAsync(ledstrip, token));
    }


    [Fact]
    public async Task DisplayFrameAsync_InvokesTheSetColorsOnTheLedstripState_WhenValidParametersArePassed()
    {
        // Create
        Ledstrip ledstrip = new Ledstrip { Id = Guid.NewGuid() };
        ReadOnlyMemory<PixelColor> frame = new ReadOnlyMemory<PixelColor>();
        CancellationToken token = CancellationToken.None;

        // Arrange
        Mock<LedstripState> ledstripStateMock = new Mock<LedstripState>(MockFactoryHelpers.CreateAnimationPlayerFactoryMock().Object, NullLedstripProxy.Instance);
        ledstripStateMock.Setup(state => state.HasAnimation()).Returns(false);

        _ledstripContextMock.Setup(x => x.GetLedstripStateById(It.IsAny<Guid>())).Returns(ledstripStateMock.Object);

        // Act
        await _ledstripControlService.DisplayFameAsync(ledstrip, frame, token);

        // Assert
        ledstripStateMock.Verify(x => x.DisplayFrame(It.IsAny<ReadOnlyMemory<PixelColor>>()), Times.Once);
    }


    [Fact]
    public async Task DisplayFrameAsync_ThrowsExceptionWhenLedstripCannotBeFound_InvalidLedstripIdPassed()
    {
        // Create
        Ledstrip ledstrip = new Ledstrip { Id = Guid.NewGuid() };
        ReadOnlyMemory<PixelColor> frame = new ReadOnlyMemory<PixelColor>();
        CancellationToken token = CancellationToken.None;

        // Arrange
        _ledstripContextMock.Setup(x => x.GetLedstripStateById(It.IsAny<Guid>())).Returns<LedstripState>(null);

        // Act and Assert
        await Assert.ThrowsAsync<LedstripNotFoundException>(() => _ledstripControlService.DisplayFameAsync(ledstrip, frame, token));
    }


    [Fact]
    public async Task ClearLedstripAsync_InvokesTheClearColors_WhenValidParametersArePassed()
    {
        // Arrange
        Ledstrip ledstrip = new Ledstrip { Id = Guid.NewGuid() };
        Frequency frequency = Frequency.FromHertz(20);
        ReadOnlyMemory<PixelColor> initialFrameBuffer = new ReadOnlyMemory<PixelColor>();
        CancellationToken token = CancellationToken.None;

        Mock<LedstripState> ledstripStateMock = new Mock<LedstripState>(MockFactoryHelpers.CreateAnimationPlayerFactoryMock().Object, NullLedstripProxy.Instance);
        ledstripStateMock.Setup(state => state.ClearFrame());

        _ledstripContextMock.Setup(x => x.GetLedstripStateById(It.IsAny<Guid>())).Returns(ledstripStateMock.Object);

        // Act
        await _ledstripControlService.ClearLedstripAsync(ledstrip, token);

        // Assert
        ledstripStateMock.Verify(x => x.ClearFrame(), Times.Once);
    }


    [Fact]
    public async Task ClearLedstripAsync_ThrowsExceptionWhenLedstripCannotBeFound_InvalidLedstripIdPassed()
    {
        // Create
        Ledstrip ledstrip = new Ledstrip { Id = Guid.NewGuid() };
        CancellationToken token = CancellationToken.None;

        // Arrange
        _ledstripContextMock.Setup(x => x.GetLedstripStateById(It.IsAny<Guid>())).Returns<LedstripState>(null);

        // Act and Assert
        await Assert.ThrowsAsync<LedstripNotFoundException>(() => _ledstripControlService.ClearLedstripAsync(ledstrip, token));
    }
}