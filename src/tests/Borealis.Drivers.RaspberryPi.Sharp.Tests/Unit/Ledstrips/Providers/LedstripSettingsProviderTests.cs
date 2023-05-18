using System;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using Borealis.Drivers.RaspberryPi.Sharp.Exceptions;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Providers;
using Borealis.Drivers.RaspberryPi.Sharp.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

using MicrosoftOptions = Microsoft.Extensions.Options.Options;



namespace Borealis.Drivers.RaspberryPi.Sharp.Tests.Unit.Ledstrips.Providers;


public class LedstripSettingsProviderTests : IAsyncLifetime
{
    private readonly ILogger<LedstripSettingsProvider> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IOptions<PathOptions> _pathOptions;

    private string _ledstripSettingsFileContent = string.Empty;
    private IDictionary<LedstripChip, LedstripSettings> _ledstripSettings = new Dictionary<LedstripChip, LedstripSettings>();


    public LedstripSettingsProviderTests()
    {
        _logger = Mock.Of<ILogger<LedstripSettingsProvider>>();
        _fileSystem = new FileSystem();

        _pathOptions = MicrosoftOptions.Create(new PathOptions
        {
            LedstripSettings = "TestData/ledstripsettings.json"
        });
    }


    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        _ledstripSettingsFileContent = await File.ReadAllTextAsync(_pathOptions.Value.LedstripSettings);
        _ledstripSettings = JsonSerializer.Deserialize<IDictionary<LedstripChip, LedstripSettings>>(_ledstripSettingsFileContent, options)!;
    }


    [Fact]
    public void GetLedstripSettings_Returns_CorrectSettings()
    {
        // Arrange
        LedstripSettingsProvider provider = new LedstripSettingsProvider(_logger, _fileSystem, _pathOptions);

        // Act
        LedstripSettings result = provider.GetLedstripSettings(LedstripChip.WS2812B);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.SpiMode, _ledstripSettings[LedstripChip.WS2812B].SpiMode);
        Assert.Equal(result.DataBitLength, _ledstripSettings[LedstripChip.WS2812B].DataBitLength);
        Assert.Equal(result.ClockFrequency, _ledstripSettings[LedstripChip.WS2812B].ClockFrequency);
        Assert.Equal(result.DataFlow, _ledstripSettings[LedstripChip.WS2812B].DataFlow);
        Assert.Equal(result.ChipSelectLineActiveState, _ledstripSettings[LedstripChip.WS2812B].ChipSelectLineActiveState);
    }


    [Fact]
    public void GetLedstripSettings_ThrowsException_WhenChipWasNotFound()
    {
        // Arrange
        LedstripSettingsProvider provider = new LedstripSettingsProvider(_logger, _fileSystem, _pathOptions);

        // Act and Assert
        Assert.Throws<InvalidConfigurationException>(() => provider.GetLedstripSettings(LedstripChip.SK9822));
    }


    [Fact]
    public void GetLedstripSettings_ThrowsException_WhenFileContentIsInvalid()
    {
        // Arrange
        Mock<IFileSystem> fileSystemMock = new Mock<IFileSystem>();
        string fileContent = "invalid content";
        fileSystemMock.Setup(fs => fs.File.ReadAllText(It.IsAny<string>())).Returns(fileContent);
        LedstripSettingsProvider provider = new LedstripSettingsProvider(_logger, fileSystemMock.Object, _pathOptions);

        // Act and Assert
        Assert.Throws<InvalidConfigurationException>(() => provider.GetLedstripSettings(LedstripChip.WS2812B));
    }


    [Fact]
    public void GetLedstripSettings_ThrowsException_WhenFileIsNotFound()
    {
        // Arrange Act and Assert
        Assert.Throws<FileNotFoundException>(() => new LedstripSettingsProvider(_logger,
                                                                                _fileSystem,
                                                                                MicrosoftOptions.Create(new PathOptions
                                                                                                            { LedstripSettings = "TestData/FileNotFound.json" })));
    }


    /// <inheritdoc />
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}