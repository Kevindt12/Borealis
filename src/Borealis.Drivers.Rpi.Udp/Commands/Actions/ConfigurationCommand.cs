using Borealis.Domain.Devices;



namespace Borealis.Drivers.Rpi.Udp.Commands.Actions;


public record ConfigurationCommand
{
    public required LedstripSettings LedstripSettings { get; init; }
}