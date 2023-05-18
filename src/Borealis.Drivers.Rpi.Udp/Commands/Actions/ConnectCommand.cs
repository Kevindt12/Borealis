using System;
using System.Linq;
using System.Net;



namespace Borealis.Drivers.Rpi.Commands.Actions;


public record ConnectCommand
{
    public required string ConfigurationConcurrencyToken { get; init; }

    public required EndPoint RemoteConnection { get; init; }
}