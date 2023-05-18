using System;
using System.Linq;



namespace Borealis.Drivers.Rpi.Commands.Actions;


public record StopAnimationCommand
{
    public required byte LedstripIndex { get; init; }
}