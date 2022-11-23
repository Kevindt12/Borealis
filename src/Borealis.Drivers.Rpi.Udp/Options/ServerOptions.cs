namespace Borealis.Drivers.Rpi.Udp.Options;


public class ServerOptions
{
    public const string Name = "ServerOptions";


    /// <summary>
    /// The Udp frame receive port.
    /// </summary>
    public int UdpServerPort { get; set; } = 8885;


    /// <summary>
    /// The tcp server.
    /// </summary>
    public int TcpServerPort { get; set; } = 8889;
}