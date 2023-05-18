using System.IO.Abstractions;
using System.Text.Json;

using Borealis.Drivers.RaspberryPi.Sharp.Device.Manager;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Exceptions;
using Borealis.Drivers.RaspberryPi.Sharp.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

using MicrosoftOptions = Microsoft.Extensions.Options.Options;



namespace Borealis.Drivers.RaspberryPi.Sharp.Tests.Unit.Device;


public class DeviceConfigurationManagerTests : IAsyncLifetime
{
    private readonly ILogger<DeviceConfigurationManager> _logger;
    private readonly Mock<IFileSystem> _mockFileSystem;
    private readonly IOptions<PathOptions> _pathOptions;
    private readonly IDeviceConfigurationManager _manager;

    private DeviceConfiguration _originalDeviceConfiguration = default!;


    public DeviceConfigurationManagerTests()
    {
        _logger = Mock.Of<ILogger<DeviceConfigurationManager>>();
        _mockFileSystem = new Mock<IFileSystem>();
        _pathOptions = MicrosoftOptions.Create(new PathOptions { DeviceConfiguration = "TestData/deviceconfiguration.json" });
        _manager = new DeviceConfigurationManager(_logger, _mockFileSystem.Object, _pathOptions);
    }


    public async Task InitializeAsync()
    {
        _originalDeviceConfiguration = JsonSerializer.Deserialize<DeviceConfiguration>(await File.ReadAllTextAsync(_pathOptions.Value.DeviceConfiguration))!;
    }


    // Happy


    [Fact]
    public async Task GetDeviceLedstripConfigurationAsync_ReadConfigurationDevice_ReturnsDeviceConfigurationFromFile()
    {
        // Arrange

        _mockFileSystem.Setup(fs => fs.File.ReadAllTextAsync(_pathOptions.Value.DeviceConfiguration, default))
                       .ReturnsAsync(JsonSerializer.Serialize(_originalDeviceConfiguration));

        // Act
        DeviceConfiguration result = await _manager.GetDeviceLedstripConfigurationAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_originalDeviceConfiguration.ConcurrencyToken, result.ConcurrencyToken);
        Assert.Collection(result.Ledstrips, ls => Assert.Equal(_originalDeviceConfiguration.Ledstrips[0].Id, ls.Id));
    }


    [Fact]
    public async Task UpdateDeviceLedstripConfigurationAsync_WritesDeviceConfiguration_SuccessfullyToFile()
    {
        // Arrange
        _mockFileSystem.Setup(fs => fs.File.WriteAllTextAsync(_pathOptions.Value.DeviceConfiguration, It.IsAny<string>(), default))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

        // Act
        await _manager.UpdateDeviceLedstripConfigurationAsync(_originalDeviceConfiguration);

        // Assert
        _mockFileSystem.Verify();
    }


    // Error Handling
    [Fact]
    public async Task GetDeviceLedstripConfigurationAsync_ReadDeviceConfiguration_ThrowsInvalidConfigurationException_WhenFileIsNotValid()
    {
        // Arrange
        _mockFileSystem.Setup(fs => fs.File.ReadAllTextAsync(_pathOptions.Value.DeviceConfiguration, default))
                       .ReturnsAsync("{ invalid json");

        // Act and Assert
        await Assert.ThrowsAsync<InvalidConfigurationException>(() => _manager.GetDeviceLedstripConfigurationAsync());
    }


    [Fact]
    public async Task GetDeviceLedstripConfigurationAsync_ReadsDeviceConfiguration_ThrowsInvalidOperationException_WhenPathIsNotSet()
    {
        // Arrange
        IOptions<PathOptions> pathOptions = MicrosoftOptions.Create(new PathOptions());

        // Act and Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => new DeviceConfigurationManager(_logger, _mockFileSystem.Object, pathOptions)
                                                               .GetDeviceLedstripConfigurationAsync());
    }


    [Fact]
    public async Task GetDeviceLedstripConfigurationAsync_ReadsDeviceConfiguration_ThrowsFileNotFoundException_WhenFileIsNotFound()
    {
        // Arrange
        IOptions<PathOptions> pathOptions = MicrosoftOptions.Create(new PathOptions
                                                                        { DeviceConfiguration = "TestData/NoFileHere.json" });

        // Act and Assert
        await Assert.ThrowsAsync<InvalidConfigurationException>(() => new DeviceConfigurationManager(_logger, new FileSystem(), pathOptions)
                                                                   .GetDeviceLedstripConfigurationAsync());
    }


    /// <inheritdoc />
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}