using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Contexts;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;



namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Service;


public interface ILedstripService
{
    /// <summary>
    /// Gets the ledstrip by a given id.
    /// </summary>
    /// <param name="ledstripId"> The ledstrip id. </param>
    /// <returns> The ledstrip that was found or null if there was none found. </returns>
    Ledstrip? GetLedstripById(Guid ledstripId);


    /// <summary>
    /// Gets the status of an ledstrip.
    /// </summary>
    /// <param name="ledstrip"> </param>
    /// <returns> </returns>
    LedstripStatus? GetLedstripStatus(Ledstrip ledstrip);


    /// <summary>
    /// Gets the status of an ledstrip.
    /// </summary>
    /// <param name="ledstrip"> </param>
    /// <returns> </returns>
    LedstripStatus? GetLedstripStatus(Guid ledstripId);
}