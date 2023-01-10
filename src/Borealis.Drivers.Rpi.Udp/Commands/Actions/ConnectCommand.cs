using System.Net;



namespace Borealis.Drivers.Rpi.Udp.Commands.Actions;


public record ConnectCommand
{
    public required string ConfigurationConcurrencyToken { get; init; }

    public required EndPoint RemoteConnection { get; init; }
}