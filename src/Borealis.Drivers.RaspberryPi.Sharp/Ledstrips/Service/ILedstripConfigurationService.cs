using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;



namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Service;


/// <summary>
/// Handles the configuration of the ledstrips.
/// </summary>
public interface ILedstripConfigurationService : ILedstripService
{
	/// <summary>
	/// Loads a configuration into the context.
	/// </summary>
	/// <param name="configuration"> The <see cref="DeviceConfiguration" /> that contains the ledstrips that we want to load. </param>
	/// <returns> </returns>
	/// <exception cref="InvalidOperationException"> Thrown when the ledstrip are busy. </exception>
	Task LoadConfigurationAsync(DeviceConfiguration configuration);
}