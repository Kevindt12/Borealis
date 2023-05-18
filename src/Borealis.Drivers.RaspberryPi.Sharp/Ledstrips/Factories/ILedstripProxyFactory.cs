using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Proxies;



namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Factories;


public interface ILedstripProxyFactory
{
    /// <summary>
    /// Creates the ledstrip proxy that should connect to the hardware to be able to control the ledstrip.
    /// </summary>
    /// <param name="ledstrip"> The ledstrip that we want to create an hardware proxy of. </param>
    /// <returns> A <see cref="LedstripProxyBase" /> instance that we can use to talk to the hardware. </returns>
    LedstripProxyBase CreateLedstripProxy(Ledstrip ledstrip);
}