using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

using Borealis.Drivers.RaspberryPi.Sharp.Exceptions;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Providers;


public class LedstripSettingsProvider : ILedstripSettingsProvider
{
    private readonly ILogger<LedstripSettingsProvider> _logger;
    private readonly IFileSystem _fileSystem;


    private readonly string _fileContent;


    public LedstripSettingsProvider(ILogger<LedstripSettingsProvider> logger, IFileSystem fileSystem, IOptions<PathOptions> pathOptions)
    {
        _logger = logger;
        _fileSystem = fileSystem;

        _fileContent = LoadContent(pathOptions.Value.LedstripSettings);
    }


    /// <summary>
    /// Loads the content from disk for the ledstrip settings.
    /// </summary>
    /// <param name="path"> The path of the ledstrip settings. </param>
    /// <returns> The file content. </returns>
    private String LoadContent(string path)
    {
        return _fileSystem.File.ReadAllText(path);
    }


    /// <inheritdoc />
    public virtual LedstripSettings GetLedstripSettings(LedstripChip chip)
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        try
        {
            // Loading the json from the content.
            IDictionary<LedstripChip, LedstripSettings> content = JsonSerializer.Deserialize<IDictionary<LedstripChip, LedstripSettings>>(_fileContent, options)!;

            // Returning the settings for a given chip.
            return content[chip];
        }
        catch (JsonException jsonException)
        {
            _logger.LogDebug(jsonException, "The Json was not valid.");

            throw new InvalidConfigurationException("The ledstrip settings where invalid", jsonException);
        }
        catch (KeyNotFoundException keyNotFoundException)
        {
            _logger.LogDebug(keyNotFoundException, "The ledstrip chip was not found in the settings.");

            throw new InvalidConfigurationException("The chip was not in the settings", keyNotFoundException);
        }
    }
}