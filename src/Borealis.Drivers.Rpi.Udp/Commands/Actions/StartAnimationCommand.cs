using System;
using System.Linq;

using UnitsNet;



namespace Borealis.Drivers.Rpi.Commands.Actions;


public record StartAnimationCommand
{
    public required byte LedstripIndex { get; init; }

    public required Frequency Frequency { get; init; }

    public required ReadOnlyMemory<PixelColor>[] InitialFrameBuffer { get; init; }
}