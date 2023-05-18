using System.IO.Abstractions;
using System.Text.Json;

using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Providers;
using Borealis.Drivers.RaspberryPi.Sharp.Exceptions;
using Borealis.Drivers.RaspberryPi.Sharp.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

using MicrosoftOptions = Microsoft.Extensions.Options.Options;



namespace Borealis.Drivers.RaspberryPi.Sharp.Tests.Unit.Device;


public class BusSettingsProviderTests : IAsyncLifetime
{
    private readonly ILogger<BusSettingsProvider> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IOptions<PathOptions> _pathOptions;

    private string _busSettingsFileContent = string.Empty;
    private IDictionary<int, Bus> _busSettings = new Dictionary<Int32, Bus>();


    public BusSettingsProviderTests()
    {
        _logger = Mock.Of<ILogger<BusSettingsProvider>>();
        _fileSystem = new FileSystem();

        _pathOptions = MicrosoftOptions.Create(new PathOptions
        {
            BusSettings = "TestData/bussettings.json"
        });
    }


    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        _busSettingsFileContent = await File.ReadAllTextAsync(_pathOptions.Value.BusSettings);
        _busSettings = JsonSerializer.Deserialize<IDictionary<int, Bus>>(_busSettingsFileContent)!;
    }


    [Fact]
    public void GetBusSettingsById_Returns_CorrectBusSettings()
    {
        // Arrange
        IBusSettingsProvider provider = new BusSettingsProvider(_logger, _fileSystem, _pathOptions);

        // Act
        Bus result = provider.GetBusSettingsById(0);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.SpiBusId, _busSettings[0].SpiBusId);
        Assert.Equal(result.SpiChipSelectId, _busSettings[0].SpiChipSelectId);
    }


    [Fact]
    public void GetBusSettingsById_ThrowsException_WhenBusNotFound()
    {
        // Arrange
        IBusSettingsProvider provider = new BusSettingsProvider(_logger, _fileSystem, _pathOptions);

        // Act and Assert
        Assert.Throws<BusNotFoundException>(() => provider.GetBusSettingsById(2));
    }


    [Fact]
    public void GetBusSettingsById_ThrowsException_WhenFileContentIsInvalid()
    {
        // Arrange
        Mock<IFileSystem> fileSystemMock = new Mock<IFileSystem>();
        String fileContent = "invalid content";
        fileSystemMock.Setup(fs => fs.File.ReadAllText(It.IsAny<string>())).Returns(fileContent);
        IBusSettingsProvider provider = new BusSettingsProvider(_logger, fileSystemMock.Object, _pathOptions);

        // Act and Assert
        Assert.Throws<InvalidSettingsException>(() => provider.GetBusSettingsById(1));
    }


    [Fact]
    public void GetBusSettingsById_ThrowsException_WhenFileIsNotFound()
    {
        // Arrange Act and Assert
        Assert.Throws<FileNotFoundException>(() => new BusSettingsProvider(_logger,
                                                                           _fileSystem,
                                                                           MicrosoftOptions.Create(new PathOptions
                                                                                                       { BusSettings = "TestData/FileNotFound.json" })));
    }


    /// <inheritdoc />
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}