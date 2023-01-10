using Borealis.Domain.Effects;



namespace Borealis.Drivers.Rpi.Udp.Commands.Actions;


public record FrameBufferQuery
{
    public required ReadOnlyMemory<PixelColor>[] Frames { get; init; }
}