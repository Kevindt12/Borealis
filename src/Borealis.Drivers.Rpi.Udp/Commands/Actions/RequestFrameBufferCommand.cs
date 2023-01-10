using Borealis.Drivers.Rpi.Udp.Ledstrips;



namespace Borealis.Drivers.Rpi.Udp.Commands.Actions;


public record RequestFrameBufferCommand
{
    public required int Amount { get; init; }

    public required LedstripProxyBase LedstripProxyBase { get; init; }
}