using Borealis.Portal.Domain.Devices;



namespace Borealis.Portal.Infrastructure.Connections;


/// <summary>
/// The factory that creates connections with device.
/// </summary>
public interface IDeviceConnectionFactory
{
    /// <summary>
    /// Creates a connection with the device.
    /// </summary>
    /// <remarks>
    /// This should also make sure that the connection is established.
    /// </remarks>
    /// <param name="device"> The device we want to connect to. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> A <see cref="IDeviceConnection" /> that is connected with the device. </returns>
    Task<IDeviceConnection> CreateConnectionAsync(Device device, CancellationToken token = default);
}