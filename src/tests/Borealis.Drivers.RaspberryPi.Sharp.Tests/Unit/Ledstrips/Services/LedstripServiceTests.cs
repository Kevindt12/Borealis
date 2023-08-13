using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Contexts;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Factories;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Service;
using Borealis.Drivers.RaspberryPi.Sharp.Tests.Unit.Helpers;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using MicrosoftOptions = Microsoft.Extensions.Options.Options;



namespace Borealis.Drivers.RaspberryPi.Sharp.Tests.Unit.Ledstrips.Services;


public class LedstripServiceTests
{
	private readonly Mock<ILogger<LedstripService>> _loggerMock;
	private readonly Mock<ILedstripProxyFactory> _ledstripProxyFactoryMock;
	private readonly Mock<LedstripStateFactory> _ledstripStateFactoryMock;
	private readonly Mock<DisplayContext> _ledstripContextMock;

	private readonly ILedstripService _ledstripService;


	public LedstripServiceTests()
	{
		_loggerMock = new Mock<ILogger<LedstripService>>();

		_ledstripStateFactoryMock = MockFactoryHelpers.CreateLedstripStateFactoryMock();
		_ledstripContextMock = new Mock<DisplayContext>();

		_ledstripProxyFactoryMock = new Mock<ILedstripProxyFactory>();

		_ledstripService = new LedstripService(_loggerMock.Object,
											   _ledstripContextMock.Object,
											   _ledstripStateFactoryMock.Object,
											   _ledstripProxyFactoryMock.Object
											  );
	}


	[Fact]
	public void GetLedstripStatus_CheckIfIdle_WhenThereIsNothingRunningOnTheLedstrip()
	{
		// Create
		Mock<DisplayState> ledstripStateMock = new Mock<DisplayState>(MockFactoryHelpers.CreateAnimationPlayerFactoryMock().Object, NullLedstripProxy.Instance);
		_ledstripContextMock.Setup(x => x.GetLedstripStateById(It.IsAny<Guid>())).Returns(ledstripStateMock.Object);

		// Arrange
		ledstripStateMock.Setup(x => x.IsAnimationPlaying()).Returns(false);
		ledstripStateMock.Setup(x => x.HasAnimation()).Returns(false);
		ledstripStateMock.Setup(x => x.IsDisplayingFrame()).Returns(false);

		// Act
		LedstripStatus? status = _ledstripService.GetLedstripStatus(Guid.NewGuid());

		// Assert
		Assert.NotNull(status);
		Assert.Equal(LedstripStatus.Idle, status);
	}


	[Fact]
	public void GetLedstripStatus_CheckAnimationPlaying_WhenThereIsAnAnimationPlaying()
	{
		// Create
		Mock<DisplayState> ledstripStateMock = new Mock<DisplayState>(MockFactoryHelpers.CreateAnimationPlayerFactoryMock().Object, NullLedstripProxy.Instance);
		_ledstripContextMock.Setup(x => x.GetLedstripStateById(It.IsAny<Guid>())).Returns(ledstripStateMock.Object);

		// Arrange
		ledstripStateMock.Setup(x => x.IsAnimationPlaying()).Returns(true);
		ledstripStateMock.Setup(x => x.HasAnimation()).Returns(false);
		ledstripStateMock.Setup(x => x.IsDisplayingFrame()).Returns(false);

		// Act
		LedstripStatus? status = _ledstripService.GetLedstripStatus(Guid.NewGuid());

		// Assert
		Assert.NotNull(status);
		Assert.Equal(LedstripStatus.PlayingAnimation, status);
	}


	[Fact]
	public void GetLedstripStatus_CheckIfAnimationPaused_WhenThereAnAnimationButNotRunning()
	{
		// Create
		Mock<DisplayState> ledstripStateMock = new Mock<DisplayState>(MockFactoryHelpers.CreateAnimationPlayerFactoryMock().Object, NullLedstripProxy.Instance);
		_ledstripContextMock.Setup(x => x.GetLedstripStateById(It.IsAny<Guid>())).Returns(ledstripStateMock.Object);

		// Arrange
		ledstripStateMock.Setup(x => x.IsAnimationPlaying()).Returns(false);
		ledstripStateMock.Setup(x => x.HasAnimation()).Returns(true);
		ledstripStateMock.Setup(x => x.IsDisplayingFrame()).Returns(false);

		// Act
		LedstripStatus? status = _ledstripService.GetLedstripStatus(Guid.NewGuid());

		// Assert
		Assert.NotNull(status);
		Assert.Equal(LedstripStatus.PausedAnimation, status);
	}


	[Fact]
	public void GetLedstripStatus_CheckIfDisplayingFrames_WhenItsDisplaingAnFrame()
	{
		// Create
		Mock<DisplayState> ledstripStateMock = new Mock<DisplayState>(MockFactoryHelpers.CreateAnimationPlayerFactoryMock().Object, NullLedstripProxy.Instance);
		_ledstripContextMock.Setup(x => x.GetLedstripStateById(It.IsAny<Guid>())).Returns(ledstripStateMock.Object);

		// Arrange
		ledstripStateMock.Setup(x => x.IsAnimationPlaying()).Returns(false);
		ledstripStateMock.Setup(x => x.HasAnimation()).Returns(false);
		ledstripStateMock.Setup(x => x.IsDisplayingFrame()).Returns(true);

		// Act
		LedstripStatus? status = _ledstripService.GetLedstripStatus(Guid.NewGuid());

		// Assert
		Assert.NotNull(status);
		Assert.Equal(LedstripStatus.DisplayingFrame, status);
	}


	[Fact]
	public void GetLedstripStatus_ReturnsNull_WhenNoLedstripFoundById()
	{
		// Arrange
		_ledstripContextMock.Setup(x => x.GetLedstripStateById(It.IsAny<Guid>())).Returns<DisplayState>(null);

		// Act and Assert
		LedstripStatus? status = _ledstripService.GetLedstripStatus(Guid.NewGuid());

		// Assert
		Assert.Null(status);
	}
}