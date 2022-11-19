namespace Borealis.Portal.Domain.Devices;


/// <summary>
/// The type of connection we want to make with the controller.
/// </summary>
public enum ConnectionType
{
    /// <summary>
    /// A Grpc connection.
    /// </summary>
    Grpc = 0,

    // A Udp connection.
    Udp = 1,

    // A Tcp connection
    Tcp = 2
}