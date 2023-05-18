using Borealis.Portal.Domain.Connectivity.Connections;
using Borealis.Portal.Domain.Devices.Models;



namespace Borealis.Portal.Domain.Connectivity.Factories;


/// <summary>
/// The factory that creates the connection with the device.
/// </summary>
public interface IDeviceConnectionFactory
{
    /// <summary>
    /// Creates the connection handler for the connection with the device.
    /// </summary>
    /// <param name="device"> </param>
    /// <param name="client"> </param>
    /// <returns> </returns>
    IDeviceConnection CreateConnection(Device device);
}