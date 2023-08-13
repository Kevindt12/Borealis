using Borealis.Networking.Connections;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Managers;


/// <summary>
/// The service that handles the connection and the action that can be taken on the connection.
/// </summary>
public interface IConnectionManager
{
	/// <summary>
	/// Sets the current connection.
	/// </summary>
	/// <param name="connection"> The current <see cref="IConnection" />. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	Task SetCurrentConnectionAsync(IConnection connection, CancellationToken token = default);


	/// <summary>
	/// Checks if we have a connection.
	/// </summary>
	/// <returns> A bool indicating that we have an connection. </returns>
	bool HasConnection();


	/// <summary>
	/// Disconnects from the portal.
	/// </summary>
	/// <returns> </returns>
	/// <exception cref="InvalidOperationException"> </exception>
	Task DisconnectAsync(CancellationToken token = default);
}