using System.Net.Sockets;

using Borealis.Drivers.Rpi.Udp.Commands;
using Borealis.Drivers.Rpi.Udp.Commands.Actions;



namespace Borealis.Drivers.Rpi.Udp.Connections;


public class PortalConnectionFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _serviceProvider;


    /// <summary>
    /// Factory for creating tcp client handlers.
    /// </summary>
    /// <param name="loggerFactory"> </param>
    /// <param name="ledstripContext"> </param>
    /// <param name="serviceProvider"> </param>
    public PortalConnectionFactory(ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
    {
        _loggerFactory = loggerFactory;
        _serviceProvider = serviceProvider;
    }


    /// <summary>
    /// Creates a tcp client handler.
    /// </summary>
    /// <param name="client"> </param>
    /// <returns> </returns>
    public PortalConnection CreateHandler(TcpClient client)
    {
        return new PortalConnection(_loggerFactory.CreateLogger<PortalConnection>(),
                                    client,
                                    _serviceProvider.GetService<IQueryHandler<ConnectCommand, ConnectedQuery>>()!,
                                    _serviceProvider.GetService<ICommandHandler<SetFrameCommand>>()!,
                                    _serviceProvider.GetService<ICommandHandler<StartAnimationCommand>>()!,
                                    _serviceProvider.GetService<ICommandHandler<StopAnimationCommand>>()!,
                                    _serviceProvider.GetService<ICommandHandler<ConfigurationCommand>>()!);
    }
}