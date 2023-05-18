using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;



namespace Borealis.Drivers.RaspberryPi.Sharp.Device.Providers;


/// <summary>
/// The provider for the bus settings.
/// </summary>
public interface IBusSettingsProvider
{
    /// <summary>
    /// Gets all the buses that we know of.
    /// </summary>
    /// <returns> </returns>
    IDictionary<int, Bus> GetBuses();


    /// <summary>
    /// Gets the bus settings for an given id.
    /// </summary>
    /// <param name="id"> The id of the bus. </param>
    /// <returns> The bus settings. </returns>
    Bus GetBusSettingsById(int id);
}