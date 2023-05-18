using System;
using System.Linq;

using Borealis.Portal.Domain.Connectivity.Models;
using Borealis.Portal.Domain.Devices.Models;



namespace Borealis.Portal.Domain.Connectivity.Connections;


/// <summary>
/// A connection with the device.
/// </summary>
public interface IDeviceConnection : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// A event that is thrown when we are disposing of the device connection.
    /// </summary>
    event EventHandler Disposing;

    /// <summary>
    /// The device we want to connect to.
    /// </summary>
    Device Device { get; }


    /// <summary>
    /// The ledstrip connection that are based on te configuration of the device.
    /// </summary>
    IReadOnlyList<ILedstripConnection> LedstripConnections { get; }


    /// <summary>
    /// A flag indicating that the configuration on the device and the portal is the same.
    /// </summary>
    bool IsConfigurationValid { get; }


    /// <summary>
    /// Starts the connection with the device.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> A <see cref="DeviceConnectionResult" /> indicating that the connection was made successfully. </returns>
    Task<DeviceConnectionResult> ConnectAsync(CancellationToken token = default);


    /// <summary>
    /// Uploads the current device configuration to the driver.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task UploadConfigurationAsync(CancellationToken token = default);


    /// <summary>
    /// Gets the status of the ledstrips from the driver.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> A <see cref="DeviceStatus" /> stating the statuses of the ledstrip. </returns>
    Task<DeviceStatus> RequestStatusAsync(CancellationToken token = default);
}