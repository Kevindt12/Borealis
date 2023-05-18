using System;
using System.Linq;

using Borealis.Domain.Ledstrips;
using Borealis.Portal.Domain.Common;
using Borealis.Portal.Domain.Devices.Models;

using UnitsNet;



namespace Borealis.Portal.Domain.Connectivity.Connections;


/// <summary>
/// The handler for getting more frames for an animation that we are playing.
/// </summary>
/// <param name="amount"> The amount of frames that we want. </param>
/// <returns> The frames that we want to send back. </returns>
public delegate Task<ReadOnlyMemory<ReadOnlyMemory<PixelColor>>> FrameBufferRequestHandler(int amount);



/// <summary>
/// The connection that we have for a connection.
/// </summary>
public interface ILedstripConnection
{
    /// <summary>
    /// The request handler for getting frame buffers.
    /// </summary>
    FrameBufferRequestHandler? FrameBufferRequestHandler { get; set; }


    /// <summary>
    /// The ledstrip we are communicating with.
    /// </summary>
    Ledstrip Ledstrip { get; }


    /// <summary>
    /// Gets the status of the ledstrip.
    /// </summary>
    /// <returns> A <see cref="LedstripStatus" /> of the ledstrip. </returns>
    Task<LedstripStatus> GetLedstripStatus();


    /// <summary>
    /// Starts the animation on the ledstrip.
    /// </summary>
    /// <param name="frequency"> The <see cref="Frequency" /> of the animation that we want to play. </param>
    /// <param name="initialFrameBuffer"> The initial frame buffer that we want to send. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task StartAnimationAsync(Frequency frequency, ReadOnlyMemory<ReadOnlyMemory<PixelColor>> initialFrameBuffer, CancellationToken token = default);


    /// <summary>
    /// Pauses the animation it keeps the stack buffer. So we can start it again.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task PauseAnimationAsync(CancellationToken token = default);


    /// <summary>
    /// Stops the animation on the ledstrip.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task StopAnimationAsync(CancellationToken token = default);


    /// <summary>
    /// Sets the colors on the ledstrip.
    /// </summary>
    /// <param name="frame"> The colors we want to set. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task SetSingleFrameAsync(ReadOnlyMemory<PixelColor> frame, CancellationToken token = default);


    /// <summary>
    /// Clears the ledstrip of the current color that its displaying.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task ClearAsync(CancellationToken token = default);
}