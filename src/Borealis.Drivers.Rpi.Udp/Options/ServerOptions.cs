namespace Borealis.Drivers.Rpi.Udp.Options;


public class ServerOptions
{
    public const string Name = "ServerOptions";


    /// <summary>
    /// The Udp frame receive port.
    /// </summary>
    public int ServerPort { get; set; } = 8885;
}