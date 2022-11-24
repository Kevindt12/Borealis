using Borealis.Domain.Communication.Messages;
using Borealis.Portal.Domain.Devices;
using Borealis.Portal.Domain.Exceptions;



namespace Borealis.Portal.Infrastructure.Connections;


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
    /// Sends a frame to the device. These frames are not verified.
    /// </summary>
    /// <remarks>
    /// Using <see cref="ValueTask" /> since this can send up to 100+ times per second per strip.
    /// This case the connection will be a UDP connection. This should be used for animations and such.
    /// High bandwidth communication to the ledstrips.
    /// </remarks>
    /// <param name="frameMessage"> The frame we want to send. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <exception cref="DeviceConnectionException">
    /// When not able to send the connection,
    /// or something is wrong with the connection.
    /// </exception>
    ValueTask SendFrameAsync(FrameMessage frameMessage, CancellationToken token = default);


    /// <summary>
    /// Sends a frame to the device. This case the communication wil be verified to make sure the driver got it.
    /// </summary>
    /// <remarks>
    /// This will use a TCP connection to send the frame.
    /// </remarks>
    /// <param name="frameMessage"> The frame we want to send. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <exception cref="DeviceConnectionException">
    /// When not able to send the connection,
    /// or something is wrong with the connection.
    /// </exception>
    Task SendConfirmedFrameAsync(FrameMessage frameMessage, CancellationToken token = default);


    /// <summary>
    /// Sends a new configuration to the device. This configuration will then be set by the device.
    /// </summary>
    /// <param name="configuration"> The configuration we want to send. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <exception cref="DeviceConnectionException"> When not able to send to the device or when not able to set the configuration. </exception>
    Task SendConfigurationAsync(ConfigurationMessage configuration, CancellationToken token = default);
}