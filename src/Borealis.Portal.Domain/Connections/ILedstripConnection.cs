using System;
using System.Linq;

using Borealis.Domain.Devices;
using Borealis.Domain.Effects;

using UnitsNet;



namespace Borealis.Portal.Domain.Connections;


public interface ILedstripConnection
{
    public event EventHandler<FramesRequestedEventArgs> FramesRequested;


    /// <summary>
    /// The ledstrip we are communicating with.
    /// </summary>
    Ledstrip Ledstrip { get; }


    /// <summary>
    /// Sends buffer of frames to the device. Returning a <see cref="int" /> meaning the buffer size that is currently at the device.
    /// </summary>
    /// <param name="frames"> The frames that we want to send. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> A <see cref="int" /> indicating the amount pf frames that are currently on the stack. </returns>
    Task SendFramesBufferAsync(IEnumerable<ReadOnlyMemory<PixelColor>> frames, CancellationToken token = default);


    /// <summary>
    /// Starts the animation on the ledstrip.
    /// </summary>
    /// <param name="token"> </param>
    /// <returns> </returns>
    Task StartAnimationAsync(Frequency frequency, IEnumerable<ReadOnlyMemory<PixelColor>> initialFrameBuffer, CancellationToken token = default);


    /// <summary>
    /// Stops the animation on the ledstrip.
    /// </summary>
    /// <param name="token"> </param>
    /// <returns> </returns>
    Task StopAnimationAsync(CancellationToken token = default);


    /// <summary>
    /// Sets the colors on the ledstrip.
    /// </summary>
    /// <param name="colors"> The colors we want to set. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task SetSingleFrameAsync(ReadOnlyMemory<PixelColor> colors, CancellationToken token = default);


    void InvokeRequestForFFrames(int amount);
}



public class FramesRequestedEventArgs : EventArgs
{
    public int Amount { get; set; }


    /// <inheritdoc />
    public FramesRequestedEventArgs(Int32 amount)
    {
        Amount = amount;
    }
}