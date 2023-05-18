using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;

using UnitsNet;



namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Service;


public interface ILedstripControlService : ILedstripService
{
    /// <summary>
    /// Starts an animation on a selected ledstrip.
    /// </summary>
    /// <param name="ledstrip"> The ledstrip on where we want to start the animation on. </param>
    /// <param name="frequency"> The frequency of the animation. </param>
    /// <param name="initialFrameBuffer"> The initial frame buffer of the animation. </param>
    /// <exception cref="InvalidOperationException"> Thrown when the ledstrip is in an invalid state. </exception>
    Task StartAnimationAsync(Ledstrip ledstrip, Frequency frequency, ReadOnlyMemory<PixelColor>[] initialFrameBuffer, CancellationToken token = default);


    /// <summary>
    /// This will pause the animation player.
    /// </summary>
    /// <param name="ledstrip"> The ledstrip on where we want to pause the animation on. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <exception cref="InvalidOperationException"> Thrown when the animation is not there or in an invalid state. </exception>
    Task PauseAnimationAsync(Ledstrip ledstrip, CancellationToken token = default);


    /// <summary>
    /// Stops an animation on an ledstrip.
    /// </summary>
    /// <param name="ledstrip"> The ledstrip that we want to stop the animation on. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <exception cref="InvalidOperationException"> Thrown when there is no animation player connected to the ledstrip. </exception>
    Task StopAnimationAsync(Ledstrip ledstrip, CancellationToken token = default);


    /// <summary>
    /// Stops all the animations that are playing.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> </returns>
    Task StopAnimations(CancellationToken token = default);


    /// <summary>
    /// Displays a single frame to the ledstrip.
    /// </summary>
    /// <param name="ledstrip"> The ledstrip we want to display the frame on. </param>
    /// <param name="frame"> The frame we want to display. </param>
    /// <exception cref="InvalidOperationException"> Thrown when the ledstrip has an active animation. </exception>
    Task DisplayFameAsync(Ledstrip ledstrip, ReadOnlyMemory<PixelColor> frame, CancellationToken token = default);


    /// <summary>
    /// Clears the ledstrip of any colors that its displaying.
    /// </summary>
    /// <param name="ledstrip"> The ledstrip we want to display the colors on. </param>
    /// <exception cref="InvalidOperationException"> Thrown when the ledstrip has an active animation or is displaying an color. </exception>
    Task ClearLedstripAsync(Ledstrip ledstrip, CancellationToken token = default);
}