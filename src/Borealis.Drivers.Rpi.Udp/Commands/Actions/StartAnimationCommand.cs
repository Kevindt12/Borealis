using Borealis.Domain.Effects;

using UnitsNet;



namespace Borealis.Drivers.Rpi.Udp.Commands.Actions;


public record StartAnimationCommand
{
    public required byte LedstripIndex { get; init; }

    public required Frequency Frequency { get; init; }

    public required ReadOnlyMemory<PixelColor>[] InitialFrameBuffer { get; init; }
}