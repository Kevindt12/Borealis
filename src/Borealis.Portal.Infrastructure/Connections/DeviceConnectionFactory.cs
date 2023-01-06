using Borealis.Portal.Domain.Connections;
using Borealis.Portal.Domain.Devices;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Infrastructure.Connections;


internal class DeviceConnectionFactory : IDeviceConnectionFactory
{
    private readonly ILoggerFactory _loggerFactory;


    public DeviceConnectionFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }


    /// <inheritdoc />
    /// <exception cref="NotImplementedException"> When the Connection type selected has not been implemented. </exception>
    public async Task<IDeviceConnection> CreateConnectionAsync(Device device, CancellationToken token = default)
    {
        return await CreateCombinedConnection(device, token);
    }


    protected virtual async Task<TcpDeviceConnection> CreateCombinedConnection(Device device, CancellationToken token = default)
    {
        return await TcpDeviceConnection.CreateConnectionAsync(_loggerFactory.CreateLogger<TcpDeviceConnection>(), device, token);
    }
}