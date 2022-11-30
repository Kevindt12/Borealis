namespace Borealis.Portal.Domain.Devices;


/// <summary>
/// The type of connection we want to make with the controller.
/// </summary>
public enum ConnectionType
{
    // A Udp connection.
    TcpUdp = 0,

    // A Tcp connection
    TcpOnly = 1
}