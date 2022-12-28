﻿using System;
using System.Linq;

using Borealis.Domain.Devices;
using Borealis.Domain.Effects;
using Borealis.Portal.Domain.Exceptions;



namespace Borealis.Portal.Domain.Connections;


public interface ILedstripConnection
{
    /// <summary>
    /// The ledstrip we are communicating with.
    /// </summary>
    Ledstrip Ledstrip { get; }


    /// <summary>
    /// Sends a frame to the device. These frames are not verified.
    /// </summary>
    /// <remarks>
    /// Using <see cref="ValueTask" /> since this can send up to 100+ times per second per strip.
    /// This case the connection will be a UDP connection. This should be used for animations and such.
    /// High bandwidth communication to the ledstrips.
    /// </remarks>
    /// <param name="colors"> Sends the colors to the ledstrip. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <exception cref="DeviceConnectionException">
    /// When not able to send the connection,
    /// or something is wrong with the connection.
    /// </exception>
    ValueTask SendFrameAsync(ReadOnlyMemory<PixelColor> colors, CancellationToken token = default);


    /// <summary>
    /// Sets the colors on the ledstrip.
    /// </summary>
    /// <param name="colors"> The colors we want to set. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task SetLedstripPixelsAsync(ReadOnlyMemory<PixelColor> colors, CancellationToken token = default);
}