using System;
using System.Linq;

using Borealis.Domain.Devices;
using Borealis.Portal.Domain.Devices;
using Borealis.Portal.Domain.Exceptions;



namespace Borealis.Portal.Domain.Connections;


/// <summary>
/// A connection with the device.
/// </summary>
public interface IDeviceConnection : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// The device we want to connect to.
    /// </summary>
    Device Device { get; }


    /// <summary>
    /// The ledstrip connection that are based on te configuration of the device.
    /// </summary>
    IReadOnlyList<ILedstripConnection> LedstripConnections { get; }


    /// <summary>
    /// Sends a new configuration to the device. This configuration will then be set by the device.
    /// </summary>
    /// <param name="ledstripSettings"> The configuration we want to send. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <exception cref="DeviceConnectionException"> When not able to send to the device or when not able to set the configuration. </exception>
    Task SendConfigurationAsync(LedstripSettings ledstripSettings, CancellationToken token = default);
}