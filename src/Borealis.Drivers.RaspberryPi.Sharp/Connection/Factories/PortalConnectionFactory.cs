using System.Net.Sockets;

using Borealis.Drivers.RaspberryPi.Sharp.Communication.Serialization;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Controllers;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Core;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Factories;


public class PortalConnectionFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IOptionsSnapshot<PortalConnectionOptions> _portalConnectionOptions;
    private readonly MessageSerializer _messageSerializer;


    public PortalConnectionFactory(ILoggerFactory loggerFactory, IOptionsSnapshot<PortalConnectionOptions> portalConnectionOptions, MessageSerializer messageSerializer)
    {
        _loggerFactory = loggerFactory;
        _portalConnectionOptions = portalConnectionOptions;
        _messageSerializer = messageSerializer;
    }


    public PortalConnection Create(TcpClient client, ConnectionController controller)
    {
        PortalConnection connection = new PortalConnection(_loggerFactory.CreateLogger<PortalConnection>(),
                                                           _portalConnectionOptions,
                                                           client,
                                                           _messageSerializer,
                                                           controller);

        return connection;
    }
}