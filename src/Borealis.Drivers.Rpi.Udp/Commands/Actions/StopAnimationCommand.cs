namespace Borealis.Drivers.Rpi.Udp.Commands.Actions;


public record StopAnimationCommand
{
    public required byte LedstripIndex { get; init; }
}