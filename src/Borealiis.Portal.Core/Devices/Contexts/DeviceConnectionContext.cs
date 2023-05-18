using System;
using System.Linq;

using Borealis.Portal.Domain.Connectivity.Connections;
using Borealis.Portal.Domain.Devices.Models;



namespace Borealis.Portal.Core.Devices.Contexts;


public class DeviceConnectionContext : IAsyncDisposable
{
	private readonly List<IDeviceConnection> _connections;


	public DeviceConnectionContext()
	{
		_connections = new List<IDeviceConnection>();
	}


	public bool IsDeviceConnected(Device device)
	{
		return _connections.Any(x => x.Device == device);
	}


	public IDeviceConnection? GetDeviceConnection(Device device)
	{
		return _connections.SingleOrDefault(x => x.Device == device);
	}


	/// <summary>
	/// Starts tracking the connection.
	/// </summary>
	/// <param name="connection"> </param>
	public void TrackConnection(IDeviceConnection connection)
	{
		_connections.Add(connection);
	}


	/// <summary>
	/// Disposes of a connection.
	/// </summary>
	/// <param name="connection"> The connection we want to dispose of. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	public async Task DisposeOfConnectionAsync(IDeviceConnection connection, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();

		bool disposed = false;

		if (connection is IAsyncDisposable asyncDisposable)
		{
			await asyncDisposable.DisposeAsync().ConfigureAwait(false);

			disposed = true;
		}

		if (connection is IDisposable disposable && !disposed)
		{
			disposable.Dispose();
		}

		_connections.Remove(connection);
	}


	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		foreach (IDeviceConnection deviceConnection in _connections)
		{
			await DisposeOfConnectionAsync(deviceConnection).ConfigureAwait(false);
		}
	}
}