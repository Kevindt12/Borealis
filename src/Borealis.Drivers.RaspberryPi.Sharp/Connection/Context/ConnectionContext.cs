using Borealis.Drivers.RaspberryPi.Sharp.Connection.Transmission;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Context;


public class ConnectionContext : IDisposable, IAsyncDisposable
{
	/// <summary>
	/// The current connection.
	/// </summary>
	public virtual DriverMessageTransmitter? CurrentMessageTransmitter { get; private set; }


	/// <summary>
	/// The connection context that holds the connection that we are connected to.
	/// </summary>
	public ConnectionContext() { }


	/// <summary>
	/// Sets the current connection.
	/// </summary>
	/// <param name="connection"> The current connection we want to set. </param>
	public virtual void SetCurrentTransmitter(DriverMessageTransmitter connection)
	{
		if (CurrentMessageTransmitter != null) throw new InvalidOperationException("The connection is still alive.");

		// Setting the current connection and listening to when the connection is dropped.
		CurrentMessageTransmitter = connection;
	}


	/// <summary>
	/// Clears the current connection.
	/// </summary>
	/// <param name="token"> CancellationToken token = default </param>
	/// <exception cref="InvalidOperationException"> Thrown when there is no connection to clear. </exception>
	public virtual async Task ClearCurrentConnectionAsync(CancellationToken token = default)
	{
		if (CurrentMessageTransmitter == null) throw new InvalidOperationException("There is no current connection.");

		// Cleaning the connection.
		await CurrentMessageTransmitter.DisposeAsync().ConfigureAwait(false);
		CurrentMessageTransmitter = null;
	}


	/// <inheritdoc />
	public void Dispose()
	{
		CurrentMessageTransmitter?.Dispose();
		CurrentMessageTransmitter = null;
	}


	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		if (CurrentMessageTransmitter != null)
		{
			await ClearCurrentConnectionAsync().ConfigureAwait(false);
		}
	}
}