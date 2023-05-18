using Borealis.Drivers.RaspberryPi.Sharp.Animations.Factories;
using Borealis.Drivers.RaspberryPi.Sharp.Animations.Handlers;
using Borealis.Drivers.RaspberryPi.Sharp.Animations.Options;
using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Services;
using Borealis.Drivers.RaspberryPi.Sharp.Exceptions;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Contexts;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Proxies;
using Borealis.Drivers.RaspberryPi.Sharp.Tests.Unit.Helpers;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using UnitsNet;

using Xunit;



namespace Borealis.Drivers.RaspberryPi.Sharp.Tests.Unit.Ledstrips.Context;


public class LedstripStateTests
{
    private readonly Mock<AnimationPlayerFactory> _animationPlayerFactoryMock;
    private readonly Mock<LedstripProxyBase> _ledstripProxyBaseMock;


    private readonly LedstripState _ledstripState;


    public LedstripStateTests()
    {
        _animationPlayerFactoryMock = MockFactoryHelpers.CreateAnimationPlayerFactoryMock();
        _ledstripProxyBaseMock = new Mock<LedstripProxyBase>(new Ledstrip());

        _ledstripState = new LedstripState(_animationPlayerFactoryMock.Object, _ledstripProxyBaseMock.Object);
    }


    [Fact]
    public async Task StartAnimationAsync_CallStartAnimation_WhenCorrectParametersCalled()
    {
        // Create
        Mock<AnimationPlayer> animationPlayerMock = new Mock<AnimationPlayer>(Mock.Of<ILogger<AnimationPlayer>>(), Mock.Of<IOptions<AnimationOptions>>(), Mock.Of<IConnectionService>(), _ledstripProxyBaseMock.Object);
        Frequency frequency = Frequency.FromHertz(20);
        ReadOnlyMemory<PixelColor>[] initialFrameBuffer = new ReadOnlyMemory<PixelColor>[1];
        CancellationToken token = CancellationToken.None;

        // Arrange
        animationPlayerMock.Setup(x => x.StartAsync(It.IsAny<Frequency>(), It.IsAny<ReadOnlyMemory<PixelColor>[]>(), CancellationToken.None));
        _animationPlayerFactoryMock.Setup(x => x.CreateAnimationPlayer(It.IsAny<LedstripProxyBase>())).Returns(animationPlayerMock.Object);

        // Act
        await _ledstripState.StartAnimationAsync(frequency, initialFrameBuffer, token);

        // Assert
        animationPlayerMock.Verify(x => x.StartAsync(It.IsAny<Frequency>(), It.IsAny<ReadOnlyMemory<PixelColor>[]>(), CancellationToken.None));
        Assert.True(_ledstripState.HasAnimation());
    }


    [Fact]
    public async Task StartAnimationAsync_ThrowsException_HandleExceptionAndClearTheAnimation()
    {
        // Create
        Mock<AnimationPlayer> animationPlayerMock = new Mock<AnimationPlayer>(Mock.Of<ILogger<AnimationPlayer>>(), Mock.Of<IOptions<AnimationOptions>>(), Mock.Of<IConnectionService>(), _ledstripProxyBaseMock.Object);
        Frequency frequency = Frequency.FromHertz(20);
        ReadOnlyMemory<PixelColor>[] initialFrameBuffer = new ReadOnlyMemory<PixelColor>[1];
        CancellationToken token = CancellationToken.None;

        // Arrange
        animationPlayerMock.Setup(x => x.StartAsync(It.IsAny<Frequency>(), It.IsAny<ReadOnlyMemory<PixelColor>[]>(), CancellationToken.None)).Throws<Exception>();
        _animationPlayerFactoryMock.Setup(x => x.CreateAnimationPlayer(It.IsAny<LedstripProxyBase>())).Returns(animationPlayerMock.Object);

        // Assert And Act
        await Assert.ThrowsAsync<Exception>(() => _ledstripState.StartAnimationAsync(frequency, initialFrameBuffer, token));

        animationPlayerMock.Verify(x => x.StartAsync(It.IsAny<Frequency>(), It.IsAny<ReadOnlyMemory<PixelColor>[]>(), CancellationToken.None), Times.Once);
        Assert.False(_ledstripState.HasAnimation());
    }


    public async Task StartAnimationAsync_AnimationAlreadyRunning_ThrowExceptionInvalidState()
    {
        // Create
        Mock<AnimationPlayer> animationPlayerMock = new Mock<AnimationPlayer>(Mock.Of<ILogger<AnimationPlayer>>(), Mock.Of<IOptions<AnimationOptions>>(), Mock.Of<IConnectionService>(), _ledstripProxyBaseMock.Object);
        Frequency frequency = Frequency.FromHertz(20);
        ReadOnlyMemory<PixelColor>[] initialFrameBuffer = new ReadOnlyMemory<PixelColor>[1];
        CancellationToken token = CancellationToken.None;

        // Arrange
        animationPlayerMock.Setup(x => x.StartAsync(It.IsAny<Frequency>(), It.IsAny<ReadOnlyMemory<PixelColor>[]>(), CancellationToken.None));
        _animationPlayerFactoryMock.Setup(x => x.CreateAnimationPlayer(It.IsAny<LedstripProxyBase>())).Returns(animationPlayerMock.Object);

        await _ledstripState.StartAnimationAsync(frequency, initialFrameBuffer, token);

        // Act and Assert
        await Assert.ThrowsAsync<InvalidLedstripStateException>(() => _ledstripState.StartAnimationAsync(frequency, initialFrameBuffer, token));
    }


    [Fact]
    public async Task PauseAnimationAsync_PausesAnimation_WhenAnimationIsRunning()
    {
        // Create
        Mock<AnimationPlayer> animationPlayerMock = new Mock<AnimationPlayer>(Mock.Of<ILogger<AnimationPlayer>>(), Mock.Of<IOptions<AnimationOptions>>(), Mock.Of<IConnectionService>(), _ledstripProxyBaseMock.Object);
        Frequency frequency = Frequency.FromHertz(20);
        ReadOnlyMemory<PixelColor>[] initialFrameBuffer = new ReadOnlyMemory<PixelColor>[1];
        CancellationToken token = CancellationToken.None;

        // Arrange
        animationPlayerMock.Setup(x => x.StopAsync(CancellationToken.None));
        animationPlayerMock.SetupGet(x => x.IsRunning).Returns(true);
        _animationPlayerFactoryMock.Setup(x => x.CreateAnimationPlayer(It.IsAny<LedstripProxyBase>())).Returns(animationPlayerMock.Object);

        await _ledstripState.StartAnimationAsync(frequency, initialFrameBuffer, token);

        // Act
        await _ledstripState.PauseAnimationAsync(token);

        // Assert
        animationPlayerMock.Verify(ap => ap.StopAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task PauseAnimationAsync_ThrowsInvalidLedstripStateException_WhenNoAnimationIsConnected()
    {
        // Arrange

        // Act & Assert
        await Assert.ThrowsAsync<InvalidLedstripStateException>(() => _ledstripState.PauseAnimationAsync());
    }


    [Fact]
    public async Task PauseAnimationAsync_ThrowsInvalidLedstripStateException_WhenAnimationIsNotPlaying()
    {
        // Arrange
        Mock<AnimationPlayer> animationPlayerMock = new Mock<AnimationPlayer>(Mock.Of<ILogger<AnimationPlayer>>(), Mock.Of<IOptions<AnimationOptions>>(), Mock.Of<IConnectionService>(), _ledstripProxyBaseMock.Object);
        Frequency frequency = Frequency.FromHertz(20);
        ReadOnlyMemory<PixelColor>[] initialFrameBuffer = new ReadOnlyMemory<PixelColor>[1];
        CancellationToken token = CancellationToken.None;

        // Arrange
        animationPlayerMock.Setup(x => x.StopAsync(CancellationToken.None));
        animationPlayerMock.SetupGet(x => x.IsRunning).Returns(false);
        _animationPlayerFactoryMock.Setup(x => x.CreateAnimationPlayer(It.IsAny<LedstripProxyBase>())).Returns(animationPlayerMock.Object);

        // Start Animation and pause it.
        await _ledstripState.StartAnimationAsync(frequency, initialFrameBuffer, token);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidLedstripStateException>(() => _ledstripState.PauseAnimationAsync(token));
    }


    [Fact]
    public async Task PauseAnimationAsync_CleansUpAnimation_WhenStoppingAnimationThrowsException()
    {
        // Create
        Mock<AnimationPlayer> animationPlayerMock = new Mock<AnimationPlayer>(Mock.Of<ILogger<AnimationPlayer>>(), Mock.Of<IOptions<AnimationOptions>>(), Mock.Of<IConnectionService>(), _ledstripProxyBaseMock.Object);
        CancellationToken token = CancellationToken.None;
        Frequency frequency = Frequency.FromHertz(20);
        ReadOnlyMemory<PixelColor>[] initialFrameBuffer = new ReadOnlyMemory<PixelColor>[1];

        // Arrange
        animationPlayerMock.Setup(x => x.StopAsync(CancellationToken.None)).Throws<Exception>();
        animationPlayerMock.SetupGet(x => x.IsRunning).Returns(true);
        _animationPlayerFactoryMock.Setup(x => x.CreateAnimationPlayer(It.IsAny<LedstripProxyBase>())).Returns(animationPlayerMock.Object);

        await _ledstripState.StartAnimationAsync(frequency, initialFrameBuffer, token);

        // Assert And Act
        await Assert.ThrowsAsync<Exception>(() => _ledstripState.PauseAnimationAsync(token));

        animationPlayerMock.Verify(x => x.StopAsync(CancellationToken.None), Times.Once);
        Assert.False(_ledstripState.HasAnimation());
    }


    [Fact]
    public async Task StopAnimationAsync_StopPlayingAnimationAndClear_WhenAnimationIsPlaying()
    {
        // Create
        Mock<AnimationPlayer> animationPlayerMock = new Mock<AnimationPlayer>(Mock.Of<ILogger<AnimationPlayer>>(), Mock.Of<IOptions<AnimationOptions>>(), Mock.Of<IConnectionService>(), _ledstripProxyBaseMock.Object);
        Frequency frequency = Frequency.FromHertz(20);
        ReadOnlyMemory<PixelColor>[] initialFrameBuffer = new ReadOnlyMemory<PixelColor>[1];
        CancellationToken token = CancellationToken.None;

        // Arrange
        animationPlayerMock.Setup(x => x.StopAsync(CancellationToken.None));
        animationPlayerMock.SetupGet(x => x.IsRunning).Returns(true);

        _animationPlayerFactoryMock.Setup(x => x.CreateAnimationPlayer(It.IsAny<LedstripProxyBase>())).Returns(animationPlayerMock.Object);
        await _ledstripState.StartAnimationAsync(frequency, initialFrameBuffer, token);

        // Act
        await _ledstripState.StopAnimationAsync(token);

        // Assert
        animationPlayerMock.Verify(ap => ap.StopAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.False(_ledstripState.HasAnimation());
    }


    [Fact]
    public async Task StopAnimationAsync_ClearTheAnimationPlayer_WhenAnimationIsNotRunningByThereIsAnAnimation()
    {
        // Create
        Mock<AnimationPlayer> animationPlayerMock = new Mock<AnimationPlayer>(Mock.Of<ILogger<AnimationPlayer>>(), Mock.Of<IOptions<AnimationOptions>>(), Mock.Of<IConnectionService>(), _ledstripProxyBaseMock.Object);
        Frequency frequency = Frequency.FromHertz(20);
        ReadOnlyMemory<PixelColor>[] initialFrameBuffer = new ReadOnlyMemory<PixelColor>[1];
        CancellationToken token = CancellationToken.None;

        // Arrange
        animationPlayerMock.Setup(x => x.StopAsync(CancellationToken.None));
        animationPlayerMock.SetupGet(x => x.IsRunning).Returns(false);

        _animationPlayerFactoryMock.Setup(x => x.CreateAnimationPlayer(It.IsAny<LedstripProxyBase>())).Returns(animationPlayerMock.Object);
        await _ledstripState.StartAnimationAsync(frequency, initialFrameBuffer, token);

        // Act
        await _ledstripState.StopAnimationAsync(token);

        // Assert
        animationPlayerMock.Verify(ap => ap.StopAsync(It.IsAny<CancellationToken>()), Times.Never);
        Assert.False(_ledstripState.HasAnimation());
    }


    [Fact]
    public async Task StopAnimationAsync_ThrowsException_WhenNoAnimationAttached()
    {
        // Act and Assert
        await Assert.ThrowsAsync<InvalidLedstripStateException>(() => _ledstripState.StopAnimationAsync(CancellationToken.None));
    }


    [Fact]
    public async Task StopAnimationAsync_CleansUpAnimation_WhenStopAnimationThrewAnException()
    {
        // Create
        Mock<AnimationPlayer> animationPlayerMock = new Mock<AnimationPlayer>(Mock.Of<ILogger<AnimationPlayer>>(), Mock.Of<IOptions<AnimationOptions>>(), Mock.Of<IConnectionService>(), _ledstripProxyBaseMock.Object);
        CancellationToken token = CancellationToken.None;
        Frequency frequency = Frequency.FromHertz(20);
        ReadOnlyMemory<PixelColor>[] initialFrameBuffer = new ReadOnlyMemory<PixelColor>[1];

        // Arrange
        animationPlayerMock.Setup(x => x.StopAsync(CancellationToken.None)).Throws<Exception>();
        animationPlayerMock.SetupGet(x => x.IsRunning).Returns(true);
        _animationPlayerFactoryMock.Setup(x => x.CreateAnimationPlayer(It.IsAny<LedstripProxyBase>())).Returns(animationPlayerMock.Object);

        await _ledstripState.StartAnimationAsync(frequency, initialFrameBuffer, token);

        // Assert And Act
        await Assert.ThrowsAsync<Exception>(() => _ledstripState.StopAnimationAsync(token));

        animationPlayerMock.Verify(x => x.StopAsync(CancellationToken.None), Times.Once);
        Assert.False(_ledstripState.HasAnimation());
    }


    [Fact]
    public async Task DisplayFrame_StartsDisplayingFrame_WhenStateIsCorrect()
    {
        // Create
        ReadOnlyMemory<PixelColor> frame = new ReadOnlyMemory<PixelColor>();

        // Arrange
        _ledstripProxyBaseMock.Setup(x => x.SetColors(It.IsAny<ReadOnlyMemory<PixelColor>>()));

        //  Act
        _ledstripState.DisplayFrame(frame);

        // Assert
        _ledstripProxyBaseMock.Verify(x => x.SetColors(It.IsAny<ReadOnlyMemory<PixelColor>>()), Times.Once);
        Assert.True(_ledstripState.IsBusy());
    }


    [Fact]
    public async Task DisplayFrame_ThrowInvalidLedstripStateException_WhenAnimationIsRunning()
    {
        // Create
        Mock<AnimationPlayer> animationPlayerMock = new Mock<AnimationPlayer>(Mock.Of<ILogger<AnimationPlayer>>(), Mock.Of<IOptions<AnimationOptions>>(), Mock.Of<IConnectionService>(), _ledstripProxyBaseMock.Object);
        CancellationToken token = CancellationToken.None;
        Frequency frequency = Frequency.FromHertz(20);
        ReadOnlyMemory<PixelColor>[] initialFrameBuffer = new ReadOnlyMemory<PixelColor>[1];

        // Arrange
        animationPlayerMock.Setup(x => x.StartAsync(It.IsAny<Frequency>(), It.IsAny<ReadOnlyMemory<PixelColor>[]>(), CancellationToken.None));
        _animationPlayerFactoryMock.Setup(x => x.CreateAnimationPlayer(It.IsAny<LedstripProxyBase>())).Returns(animationPlayerMock.Object);

        await _ledstripState.StartAnimationAsync(frequency, initialFrameBuffer, token);

        // Assert And Act
        Assert.Throws<InvalidLedstripStateException>(() => _ledstripState.DisplayFrame(initialFrameBuffer[0]));
    }


    [Fact]
    public void ClearFrame_ClearsTheLedstripFrame_WhenStateIsValid()
    {
        // Arrange
        _ledstripProxyBaseMock.Setup(x => x.Clear());

        //  Act
        _ledstripState.ClearFrame();

        // Assert
        _ledstripProxyBaseMock.Verify(x => x.Clear(), Times.Once);
        Assert.False(_ledstripState.IsBusy());
    }


    [Fact]
    public async Task ClearFrame_ThrowInvalidLedstripStateException_WhenAnimationIsRunning()
    {
        // Create
        Mock<AnimationPlayer> animationPlayerMock = new Mock<AnimationPlayer>(Mock.Of<ILogger<AnimationPlayer>>(), Mock.Of<IOptions<AnimationOptions>>(), Mock.Of<IConnectionService>(), _ledstripProxyBaseMock.Object);
        CancellationToken token = CancellationToken.None;
        Frequency frequency = Frequency.FromHertz(20);
        ReadOnlyMemory<PixelColor>[] initialFrameBuffer = new ReadOnlyMemory<PixelColor>[1];

        // Arrange
        animationPlayerMock.Setup(x => x.StartAsync(It.IsAny<Frequency>(), It.IsAny<ReadOnlyMemory<PixelColor>[]>(), CancellationToken.None));
        _animationPlayerFactoryMock.Setup(x => x.CreateAnimationPlayer(It.IsAny<LedstripProxyBase>())).Returns(animationPlayerMock.Object);

        await _ledstripState.StartAnimationAsync(frequency, initialFrameBuffer, token);

        // Assert And Act
        Assert.Throws<InvalidLedstripStateException>(() => _ledstripState.ClearFrame());
    }
}