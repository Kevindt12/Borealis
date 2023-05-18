using System;
using System.Linq;



namespace Borealis.Drivers.Rpi.Options;


public class ServerOptions
{
    public const string Name = "ServerOptions";


    /// <summary>
    /// The Udp frame receive port.
    /// </summary>
    public int ServerPort { get; set; } = 8885;
}