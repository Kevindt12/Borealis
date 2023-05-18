using System;
using System.Linq;

using Borealis.Domain.Effects;
using Borealis.Domain.Ledstrips;
using Borealis.Portal.Domain.Common;
using Borealis.Portal.Domain.Devices.Models;



namespace Borealis.Portal.Domain.Ledstrips.Services;


public interface ILedstripService
{
    /// <summary>
    /// Gets the status of the ledstrip.
    /// </summary>
    /// <param name="ledstrip"> The ledstrip that we want to get a status of. </param>
    /// <returns> A <see cref="LedstripStatus" /> that tells us what the ledstrip is doing.. </returns>
    LedstripStatus? GetLedstripStatus(Ledstrip ledstrip);


    /// <summary>
    /// Starts playing an animation on a ledstrip.
    /// </summary>
    /// <param name="ledstrip"> The ledstrip that we want to play an animation on. </param>
    /// <param name="effect"> The effect that we want to play </param>
    Task AttachEffectToLedstripAsync(Ledstrip ledstrip, Effect effect, CancellationToken token = default);


    /// <summary>
    /// Starts the animation that was attached to the ledstrip.
    /// </summary>
    /// <param name="ledstrip"> </param>
    /// <param name="token"> </param>
    /// <returns> </returns>
    Task StartAnimationAsync(Ledstrip ledstrip, CancellationToken token = default);


    /// <summary>
    /// Stops displaying or playing anything on the ledstrip that we have selected.
    /// </summary>
    /// <param name="ledstrip"> The ledstrip that we want to stop displaying anything on. </param>
    /// <returns> </returns>
    Task StopLedstripAsync(Ledstrip ledstrip, CancellationToken token = default);


    /// <summary>
    /// Tests the ledstrip with a display of red green blue.
    /// </summary>
    /// <param name="ledstrip"> The ledstrip that we want to test. </param>
    /// <returns> </returns>
    Task TestLedstripAsync(Ledstrip ledstrip, CancellationToken token = default);


    /// <summary>
    /// Sets a single color on a ledstrip.
    /// </summary>
    /// <param name="ledstrip"> The <see cref="Ledstrip" /> that we want to set the color on. </param>
    /// <param name="color"> The color we want to display on the ledstrip. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task SetSolidColorAsync(Ledstrip ledstrip, PixelColor color, CancellationToken token = default);


    /// <summary>
    /// Gets the attached effect to the ledstrip.
    /// </summary>
    /// <param name="ledstrip"> The strip that we want to get the effect of. </param>
    /// <returns> The <see cref="Effect" /> copy of that is currently tracked. This is not the one from the database but from the animation player. </returns>
    Effect? GetAttachedEffect(Ledstrip ledstrip);


    /// <summary>
    /// Gets the color that was being displayed on the ledstrip.
    /// </summary>
    /// <param name="ledstrip"> The ledstrip that is displaying the color. </param>
    /// <returns> The <see cref="PixelColor" /> or <see cref="null" /> of there was no color being displayed. </returns>
    PixelColor? GetDisplayingColorOnLedstrip(Ledstrip ledstrip);
}