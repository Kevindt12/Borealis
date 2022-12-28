using System.Net.Sockets;

using Borealis.Drivers.Rpi.Udp.Contexts;



namespace Borealis.Drivers.Rpi.Udp.Connections;


public class TcpClientHandlerFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly LedstripContext _ledstripContext;


    /// <summary>
    /// Factory for creating tcp client handlers.
    /// </summary>
    /// <param name="loggerFactory"> </param>
    /// <param name="ledstripContext"> </param>
    public TcpClientHandlerFactory(ILoggerFactory loggerFactory, LedstripContext ledstripContext)
    {
        _loggerFactory = loggerFactory;
        _ledstripContext = ledstripContext;
    }


    /// <summary>
    /// Creates a tcp client handler.
    /// </summary>
    /// <param name="client"> </param>
    /// <returns> </returns>
    public TcpClientConnection CreateHandler(TcpClient client)
    {
        return new TcpClientConnection(_loggerFactory.CreateLogger<TcpClientConnection>(), client, _ledstripContext);
    }
}