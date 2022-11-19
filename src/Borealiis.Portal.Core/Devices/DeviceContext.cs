using System;
using System.Linq;

using Borealis.Portal.Infrastructure.Connections;



namespace Borealis.Portal.Core.Devices;


public class DeviceContext : IDisposable
{
    private readonly List<IDeviceConnection> _connections;


    public IReadOnlyList<IDeviceConnection> Connections => _connections;


    public DeviceContext()
    {
        _connections = new List<IDeviceConnection>();
    }


    public void AddDeviceConnection(IDeviceConnection connection)
    {
        _connections.Add(connection);
    }


    public async Task RemoveDeviceConnectionAsync(IDeviceConnection connection)
    {
        _connections.Remove(connection);
        await connection.DisposeAsync();
    }


    /// <inheritdoc />
    public void Dispose() { }
}