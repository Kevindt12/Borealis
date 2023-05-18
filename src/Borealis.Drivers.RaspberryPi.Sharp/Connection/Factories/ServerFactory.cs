using Borealis.Drivers.RaspberryPi.Sharp.Connection.Context;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Core;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Managers;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Factories;


public class ServerFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConnectionManager _connectionManager;
    private readonly ConnectionContext _connectionContext;
    private readonly IOptions<ServerOptions> _serverOptions;


    public ServerFactory(ILoggerFactory loggerFactory, IConnectionManager connectionManager, ConnectionContext connectionContext, IOptions<ServerOptions> serverOptions)
    {
        _loggerFactory = loggerFactory;
        _connectionManager = connectionManager;
        _connectionContext = connectionContext;
        _serverOptions = serverOptions;
    }


    /// <summary>
    /// Creates a server.
    /// </summary>
    /// <returns> </returns>
    /// <exception cref="NotImplementedException"> </exception>
    public virtual Server CreateServer()
    {
        return new Server(_loggerFactory.CreateLogger<Server>(), _connectionManager, _connectionContext, _serverOptions);
    }
}