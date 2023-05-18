using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Services;


/// <summary>
/// Handles communication with the current connection.
/// </summary>
public interface IConnectionService
{
    /// <summary>
    /// Requests a frame buffer from the current connection.
    /// </summary>
    /// <param name="ledstrip"> The <see cref="Ledstrip" /> that we are playing the animation on. </param>
    /// <param name="amount"> The amount of frame we want to get. </param>
    /// <returns> A <see cref="PixelColor" /> array of array with all the frames that we want. </returns>
    Task<ReadOnlyMemory<PixelColor>[]> RequestFrameBufferAsync(Ledstrip ledstrip, int amount, CancellationToken token = default);
}