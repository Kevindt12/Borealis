using System;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;

using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Exceptions;
using Borealis.Drivers.RaspberryPi.Sharp.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace Borealis.Drivers.RaspberryPi.Sharp.Device.Manager;


public class DeviceConfigurationManager : IDeviceConfigurationManager
{
    private readonly ILogger<DeviceConfigurationManager> _logger;
    private readonly IFileSystem _fileSystem;

    private readonly string _deviceConfigurationPath;


    public DeviceConfigurationManager(ILogger<DeviceConfigurationManager> logger, IFileSystem fileSystem, IOptions<PathOptions> pathOptions)
    {
        _logger = logger;
        _fileSystem = fileSystem;

        _deviceConfigurationPath = pathOptions.Value.DeviceConfiguration ?? throw new InvalidOperationException("The key paths is missing from the appsettings.json.");
    }


    /// <inheritdoc />
    public virtual async Task UpdateDeviceLedstripConfigurationAsync(DeviceConfiguration deviceConfiguration, CancellationToken token = default)
    {
        _logger.LogTrace("Saving the new device configuration to disk.");

        // Check that the buses are available to be used.
        await WriteDeviceConfigurationAsync(deviceConfiguration, token).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public virtual async Task<DeviceConfiguration> GetDeviceLedstripConfigurationAsync(CancellationToken token = default)
    {
        _logger.LogTrace("Getting the device configuration from disk.");

        return await ReadDeviceConfigurationAsync(token).ConfigureAwait(false);
    }


    /// <summary>
    /// Reads the device configuration file.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> The device configuration that was read. </returns>
    /// <exception cref="FileNotFoundException"> When the file is not found. </exception>
    /// <exception cref="InvalidOperationException"> Thrown when the configuration path of the device was not set. </exception>
    /// <exception cref="InvalidConfigurationException"> When the configuration file is not valid. </exception>
    protected virtual async Task<DeviceConfiguration> ReadDeviceConfigurationAsync(CancellationToken token = default)
    {
        try
        {
            string fileContent = await _fileSystem.File.ReadAllTextAsync(_deviceConfigurationPath, token).ConfigureAwait(false);

            return JsonSerializer.Deserialize<DeviceConfiguration>(fileContent)!;
        }
        catch (FileNotFoundException fileNotFoundException)
        {
            throw new InvalidConfigurationException("The configuration was not found", fileNotFoundException);
        }
        catch (JsonException jsonException)
        {
            throw new InvalidConfigurationException("The device configuration was not valid.", jsonException);
        }
    }


    /// <summary>
    /// Writes the configuration file to disk.
    /// </summary>
    /// <param name="deviceConfiguration"> The configuration file that we want to write. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <exception cref="InvalidConfigurationException"> Thrown when the configuration path of the device was not set. </exception>
    protected virtual async Task WriteDeviceConfigurationAsync(DeviceConfiguration deviceConfiguration, CancellationToken token = default)
    {
        string fileContent = JsonSerializer.Serialize(deviceConfiguration,
                                                      new JsonSerializerOptions
                                                      {
                                                          WriteIndented = true
                                                      });

        _logger.LogTrace($"Writing new configuration to {_deviceConfigurationPath}");
        await _fileSystem.File.WriteAllTextAsync(_deviceConfigurationPath, fileContent, token).ConfigureAwait(false);
    }
}