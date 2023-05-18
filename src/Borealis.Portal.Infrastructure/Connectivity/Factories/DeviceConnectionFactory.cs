using Borealis.Portal.Domain.Connectivity.Connections;
using Borealis.Portal.Domain.Connectivity.Factories;
using Borealis.Portal.Domain.Devices.Models;
using Borealis.Portal.Infrastructure.Connectivity.Connections;
using Borealis.Portal.Infrastructure.Connectivity.Options;
using Borealis.Portal.Infrastructure.Connectivity.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace Borealis.Portal.Infrastructure.Connectivity.Factories;


internal class DeviceConnectionFactory : IDeviceConnectionFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IOptions<ConnectivityOptions> _connectivityOptions;
    private readonly MessageSerializer _messageSerializer;
    private readonly LedstripConnectionFactory _ledstripConnectionFactory;
    private readonly CommunicationHandlerFactory _communicationHandlerFactory;


    public DeviceConnectionFactory(ILoggerFactory loggerFactory,
                                   IOptions<ConnectivityOptions> connectivityOptions,
                                   MessageSerializer messageSerializer,
                                   LedstripConnectionFactory ledstripConnectionFactory,
                                   CommunicationHandlerFactory communicationHandlerFactory)
    {
        _loggerFactory = loggerFactory;
        _connectivityOptions = connectivityOptions;
        _messageSerializer = messageSerializer;
        _ledstripConnectionFactory = ledstripConnectionFactory;
        _communicationHandlerFactory = communicationHandlerFactory;
    }


    /// <inheritdoc />
    public virtual IDeviceConnection CreateConnection(Device device)
    {
        return new DeviceConnection(_loggerFactory.CreateLogger<DeviceConnection>(), _connectivityOptions, _messageSerializer, _ledstripConnectionFactory, _communicationHandlerFactory, device);
    }
}