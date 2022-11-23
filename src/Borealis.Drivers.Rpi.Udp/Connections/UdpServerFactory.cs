namespace Borealis.Drivers.Rpi.Udp.Connections;


public class UdpServerFactory
{
    private readonly ILoggerFactory _loggerFactory;


    /// <summary>
    /// The factory that creates udp server.
    /// </summary>
    public UdpServerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }


    /// <summary>
    /// Creates a udp server connection to be used.
    /// </summary>
    /// <param name="port"> The port on where we listen, </param>
    /// <returns> A <see cref="UdpServer" /> that can be used for connection. </returns>
    public virtual UdpServer CreateUdpServer(int port)
    {
        return new UdpServer(_loggerFactory.CreateLogger<UdpServer>(), port);
    }
}