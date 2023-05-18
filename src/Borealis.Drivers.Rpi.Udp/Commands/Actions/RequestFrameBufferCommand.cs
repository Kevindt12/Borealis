using System;
using System.Linq;

using Borealis.Drivers.Rpi.Ledstrips;



namespace Borealis.Drivers.Rpi.Commands.Actions;


public record RequestFrameBufferCommand
{
    public required int Amount { get; init; }

    public required LedstripProxyBase LedstripProxyBase { get; init; }
}