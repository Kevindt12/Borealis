using System.Net.Sockets;



namespace Borealis.Drivers.Rpi.Udp.Connections;


public class PortalConnectionFactory
{
    private readonly ILoggerFactory _loggerFactory;


    /// <summary>
    /// Factory for creating tcp client handlers.
    /// </summary>
    /// <param name="loggerFactory"> </param>
    /// <param name="ledstripContext"> </param>
    public PortalConnectionFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }


    /// <summary>
    /// Creates a tcp client handler.
    /// </summary>
    /// <param name="client"> </param>
    /// <returns> </returns>
    public PortalConnection CreateHandler(TcpClient client)
    {
        return new PortalConnection(_loggerFactory.CreateLogger<PortalConnection>(), client);
    }
}