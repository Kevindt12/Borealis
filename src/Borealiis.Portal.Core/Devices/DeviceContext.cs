using System;
using System.Linq;

using Borealis.Portal.Domain.Connections;



namespace Borealis.Portal.Core.Devices;


public class DeviceContext : IDisposable
{
    private readonly List<IDeviceConnection> _connections;


    public IReadOnlyList<IDeviceConnection> Connections => _connections;


    public IReadOnlyList<ILedstripConnection> LedstripConnections => _connections.SelectMany(x => x.LedstripConnections).ToList();


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