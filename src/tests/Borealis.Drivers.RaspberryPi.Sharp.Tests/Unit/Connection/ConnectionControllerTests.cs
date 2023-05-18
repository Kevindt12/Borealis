using Borealis.Drivers.RaspberryPi.Sharp.Connection.Controllers;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Manager;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Validation;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Service;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using Xunit;



namespace Borealis.Drivers.RaspberryPi.Sharp.Tests.Unit.Connection;


public class ConnectionControllerTests
{
    private readonly Mock<ILedstripControlService> _ledstripServiceMock;
    private readonly Mock<ILedstripConfigurationService> _ledstripConfigurationServiceMock;
    private readonly Mock<IDeviceConfigurationManager> _deviceConfigurationManagerMock;
    private readonly Mock<IDeviceConfigurationValidator> _deviceConfigurationValidatorMock;

    private readonly ConnectionController _connectionController;


    public ConnectionControllerTests()
    {
        _ledstripServiceMock = new Mock<ILedstripControlService>();
        _ledstripConfigurationServiceMock = new Mock<ILedstripConfigurationService>();
        _deviceConfigurationManagerMock = new Mock<IDeviceConfigurationManager>();
        _deviceConfigurationValidatorMock = new Mock<IDeviceConfigurationValidator>();

        _connectionController = new ConnectionController(NullLogger<ConnectionController>.Instance,
                                                         _deviceConfigurationValidatorMock.Object,
                                                         _ledstripServiceMock.Object,
                                                         _ledstripConfigurationServiceMock.Object,
                                                         _deviceConfigurationManagerMock.Object);
    }


    [Fact]
    public async Task ConnectAsync_CheckIfWeCanConnect_ReturnTrueIfWeCanConnect()
    {
        // Arrange
        string concurrencyToken = "Test Token";

        _deviceConfigurationManagerMock.Setup(x => x.GetDeviceLedstripConfigurationAsync(CancellationToken.None))
                                       .Returns(Task.FromResult(new DeviceConfiguration
                                        {
                                            ConcurrencyToken = concurrencyToken
                                        }));

        // Act
        ConnectResult result = await _connectionController.ConnectAsync(concurrencyToken, CancellationToken.None);

        // Assert
        Assert.True(result.Successful);
    }


    [Fact]
    public async Task SetConfigurationAsync_InvokeWithValidParameters_EnsureThatAllMethodsAreCalled()
    {
        // Create
        List<Ledstrip> ledstrips = new List<Ledstrip>();
        string concurrencyToken = "Test Token";

        // Arrange
        _ledstripConfigurationServiceMock.Setup(x => x.CanLoadConfiguration()).Returns(true);
        _deviceConfigurationValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<DeviceConfiguration>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(DeviceConfigurationValidationResult.Success));

        // Act
        await _connectionController.SetConfigurationAsync(ledstrips, concurrencyToken, CancellationToken.None);

        // Assert
        _deviceConfigurationManagerMock.Verify(x => x.UpdateDeviceLedstripConfigurationAsync(It.IsAny<DeviceConfiguration>(), CancellationToken.None), Times.Once);
        _deviceConfigurationValidatorMock.Verify(x => x.ValidateAsync(It.IsAny<DeviceConfiguration>(), It.IsAny<CancellationToken>()), Times.Once);
        _ledstripConfigurationServiceMock.Verify(x => x.LoadConfigurationAsync(It.IsAny<DeviceConfiguration>()), Times.Once);
    }
}