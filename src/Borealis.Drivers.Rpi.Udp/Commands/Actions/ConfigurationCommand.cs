using System;
using System.Linq;

using Borealis.Domain.Devices;



namespace Borealis.Drivers.Rpi.Commands.Actions;


public record ConfigurationCommand
{
    public required DeviceConfiguration DeviceConfiguration { get; init; }
}