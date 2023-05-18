using System;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;

using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Exceptions;
using Borealis.Drivers.RaspberryPi.Sharp.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace Borealis.Drivers.RaspberryPi.Sharp.Device.Providers;


public class BusSettingsProvider : IBusSettingsProvider
{
    private readonly ILogger<BusSettingsProvider> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly string _fileContent;


    /// <summary>
    /// The bus settings provider.
    /// </summary>
    public BusSettingsProvider(ILogger<BusSettingsProvider> logger, IFileSystem fileSystem, IOptions<PathOptions> pathOptions)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _fileContent = LoadFileContent(pathOptions.Value.BusSettings);
    }


    /// <summary>
    /// Reads the text from the file that we where given.
    /// </summary>
    /// <param name="path"> The path of the file. </param>
    /// <returns> The content of the path. </returns>
    private string LoadFileContent(string path)
    {
        return _fileSystem.File.ReadAllText(path);
    }


    /// <inheritdoc />
    public IDictionary<int, Bus> GetBuses()
    {
        try
        {
            return JsonSerializer.Deserialize<IDictionary<int, Bus>>(_fileContent)!;
        }
        catch (JsonException jsonException)
        {
            _logger.LogDebug(jsonException, "The Json was not valid.");

            throw new InvalidSettingsException("The bus settings where invalid", jsonException);
        }
    }


    /// <inheritdoc />
    public virtual Bus GetBusSettingsById(int id)
    {
        try
        {
            IDictionary<int, Bus> settings = JsonSerializer.Deserialize<IDictionary<int, Bus>>(_fileContent)!;

            return settings[id];
        }
        catch (JsonException jsonException)
        {
            _logger.LogDebug(jsonException, "The Json was not valid.");

            throw new InvalidSettingsException("The bus settings where invalid", jsonException);
        }
        catch (KeyNotFoundException keyNotFoundException)
        {
            _logger.LogDebug(keyNotFoundException, "The key of the bus settings was not found,");

            throw new BusNotFoundException("The bus was not found", keyNotFoundException);
        }
    }
}