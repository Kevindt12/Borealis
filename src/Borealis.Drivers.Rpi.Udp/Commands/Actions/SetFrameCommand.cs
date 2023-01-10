using Borealis.Domain.Effects;



namespace Borealis.Drivers.Rpi.Udp.Commands.Actions;


public record SetFrameCommand
{
    public required byte LedstripIndex { get; init; }

    public required ReadOnlyMemory<PixelColor> Frame { get; init; }
}