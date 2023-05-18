using System;
using System.Linq;



namespace Borealis.Drivers.Rpi.Commands.Actions;


public record SetFrameCommand
{
    public required byte LedstripIndex { get; init; }

    public required ReadOnlyMemory<PixelColor> Frame { get; init; }
}