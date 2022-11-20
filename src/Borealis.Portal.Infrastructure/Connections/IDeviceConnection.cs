using Borealis.Domain.Communication.Messages;
using Borealis.Portal.Domain.Devices;



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
    /// Sends a frame to the device.
    /// </summary>
    /// <remarks>
    /// Using <see cref="ValueTask" /> since this can send up to 100+ times per second per strip.
    /// </remarks>
    /// <param name="frameMessage"> The frame we want to send. </param>
    ValueTask SendFrameAsync(FrameMessage frameMessage);
}