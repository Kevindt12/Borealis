using Borealis.Portal.Domain.Devices;
using Borealis.Portal.Domain.Exceptions;

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
        return device.ConnectionType switch
        {
            ConnectionType.Grpc => await CreateGrpcConnection(device, token),
            ConnectionType.Udp  => await CreateUdpConnection(device, token),
            _                   => throw new NotImplementedException("The connection type selected was not supported.")
        };
    }


    // TODO: Make sure that we catch the right excpetions. This is to broad.


    protected virtual async Task<GrpcDeviceConnection> CreateGrpcConnection(Device device, CancellationToken token = default)
    {
        GrpcDeviceConnection connection = default!;

        try
        {
            connection = await GrpcDeviceConnection.CreateAsync(_loggerFactory.CreateLogger<GrpcDeviceConnection>(), device);

            return connection;
        }
        catch (Exception e)
        {
            // Making sure that we have disposed of the connection.
            await connection.DisposeAsync();

            throw new DeviceConnectionException("Unable to create a connection with device.", device);
        }
    }


    protected virtual async Task<UdpDeviceConnection> CreateUdpConnection(Device device, CancellationToken token = default)
    {
        UdpDeviceConnection? connection = default;

        try
        {
            connection = await UdpDeviceConnection.CreateConnectionAsync(_loggerFactory.CreateLogger<UdpDeviceConnection>(), device);

            return connection;
        }
        catch (Exception e)
        {
            // Making sure that we have disposed to the connection.
            if (connection != null)
            {
                await connection.DisposeAsync();
            }

            throw new DeviceConnectionException("Unable to create a connection with device.", device);
        }
    }
}