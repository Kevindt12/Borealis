using System;

using Borealis.Shared.Extensions;

using System.Linq;
using System.Text.Json;

using Borealis.Domain.Devices;
using Borealis.Portal.Domain.Exceptions;



namespace Borealis.Drivers.Rpi.Services;


public class SettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private readonly IConfiguration _configuration;


    /// <summary>
    /// The service that handles everything to do with the settigns file.
    /// </summary>
    /// <param name="logger"> </param>
    /// <param name="configuration"> </param>
    public SettingsService(ILogger<SettingsService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }


    /// <summary>
    /// Reads the settings from the settings file where we keep the information about the ledstrips.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> A <see cref="DeviceConfiguration" /> object where we have all the settings adn the concurrency token. </returns>
    /// <exception cref="InvalidOperationException"> Thrown when there was no settings path specified. </exception>
    /// <exception cref="InvalidDeviceConfigurationException"> Thrown when unable to read the file that was specified. </exception>
    /// <exception cref="IOException"> Thrown when there is a problem with reading the file. </exception>
    /// <exception cref="FileNotFoundException"> Thrown when we cant find the path to the settings file defined in the appsettings.js </exception
    public virtual async Task<DeviceConfiguration> ReadLedstripSettingsAsync(CancellationToken token = default)
    {
        _logger.LogDebug("Getting the settings path from the appsettings.json.");
        string settingsPath = GetSettingsPath();

        try
        {
            // Reading the json.
            _logger.LogInformation($"Settings path {settingsPath}. Start reading Json.");
            string json = await File.ReadAllTextAsync(settingsPath, token).ConfigureAwait(false);

            // Deserializing the Json
            DeviceConfiguration configuration = JsonSerializer.Deserialize<DeviceConfiguration>(json)!;
            _logger.LogDebug($"Settings have been read {configuration.LogToJson()}");

            return configuration;
        }
        catch (JsonException jsonException)
        {
            _logger.LogTrace(jsonException, "Unable to read the json that was specified in the appsettings.js.");

            throw new InvalidDeviceConfigurationException("Unable to read json file.", jsonException);
        }
    }


    /// <summary>
    /// Writes the new settings to the file specified in the appsettings.json
    /// </summary>
    /// <param name="deviceConfiguration"> The <see cref="DeviceConfiguration" /> that we want to write to disk. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <exception cref="IOException"> Thrown when there is an error writing to disk. </exception>
    /// <exception cref="InvalidOperationException"> When the settings path has not been filled in. </exception>
    /// <exception cref="FileNotFoundException"> When the file could not be found. </exception>
    public virtual async Task WriteLedstripSettingsAsync(DeviceConfiguration deviceConfiguration, CancellationToken token = default)
    {
        _logger.LogDebug("Getting the settings path from the appsettings.json.");
        string settingsPath = GetSettingsPath();

        // Logging where we have found the settings file.
        _logger.LogInformation($"Settings file found at {settingsPath}.");

        // Serializing the json.
        string json = JsonSerializer.Serialize(deviceConfiguration);
        _logger.LogDebug($"Serialized the settings. {json}.");

        try
        {
            _logger.LogDebug("Writing the settings file.");
            await File.WriteAllTextAsync(settingsPath, json, token);

            _logger.LogInformation("File has been write to disk,");
        }
        catch (IOException e)
        {
            _logger.LogError(e, "A error has occurred while writing the settings file.");

            throw;
        }
    }


    /// <summary>
    /// Checks and gets the settings file.
    /// </summary>
    /// <returns> A <see cref="String" /> that is the path to the settings file. </returns>
    /// <exception cref="InvalidOperationException"> When the settings path has not been filled in. </exception>
    /// <exception cref="FileNotFoundException"> When the file could not be found. </exception>
    private string GetSettingsPath()
    {
        // Getting the ledstrip path.
        string settingsRelativePath = _configuration.GetValue<string>("SettingsPath") ?? throw new InvalidOperationException("There was no path to configuration file.");

        // Getting the path and checking that there is a file there.
        string settingsAbsolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, settingsRelativePath);

        if (!File.Exists(settingsAbsolutePath)) throw new FileNotFoundException("There was no settings file found.");

        return settingsAbsolutePath;
    }
}