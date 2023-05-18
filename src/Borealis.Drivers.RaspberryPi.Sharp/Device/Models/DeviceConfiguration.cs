using System;
using System.Linq;
using System.Text.Json;

using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;



namespace Borealis.Drivers.RaspberryPi.Sharp.Device.Models;


/// <summary>
/// The device configuration that holds all the data for the ledstrips.
/// </summary>
public class DeviceConfiguration
{
    /// <summary>
    /// The configuration concurrency token.
    /// </summary>
    public string ConcurrencyToken { get; set; } = string.Empty;

    /// <summary>
    /// The ledstrips that are in the configuration.
    /// </summary>
    public IReadOnlyList<Ledstrip> Ledstrips { get; set; } = new List<Ledstrip>();


    /// <summary>
    /// Generates a Json object log message to be send to the log.
    /// </summary>
    /// <returns> </returns>
    public virtual string GenerateLogMessage()
    {
        return JsonSerializer.Serialize(this,
                                        new JsonSerializerOptions
                                        {
                                            WriteIndented = true
                                        });
    }
}