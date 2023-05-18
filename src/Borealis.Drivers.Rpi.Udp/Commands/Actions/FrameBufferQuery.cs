using System;
using System.Linq;



namespace Borealis.Drivers.Rpi.Commands.Actions;


public record FrameBufferQuery
{
    public required ReadOnlyMemory<PixelColor>[] Frames { get; init; }
}