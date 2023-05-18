using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;



namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Providers;


/// <summary>
/// Provides the settings for the ledstrip connection.
/// </summary>
public interface ILedstripSettingsProvider
{
    /// <summary>
    /// Gets the settings by the given chip.
    /// </summary>
    /// <param name="chip"> </param>
    /// <returns> </returns>
    LedstripSettings GetLedstripSettings(LedstripChip chip);
}