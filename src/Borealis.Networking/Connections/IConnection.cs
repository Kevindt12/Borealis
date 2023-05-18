using Borealis.Networking.IO;
using Borealis.Shared.Eventing;



namespace Borealis.Networking.Connections;


/// <summary>
/// An connection with an remote client.
/// </summary>
public interface IConnection : IDisposable, IAsyncDisposable
{
	/// <summary>
	/// Event thrown when a socket disconnects from the client.
	/// </summary>
	event AsyncEventHandler<ConnectionDisconnectedEventArgs> ConnectionDisconnected;

	/// <summary>
	/// The socket that we are using for the connection.
	/// </summary>
	ISocket Socket { get; }

	/// <summary>
	/// If there is data available to be read from the connection.
	/// </summary>
	bool DataAvailable { get; }


	/// <summary>
	/// The options we will use for this connection.
	/// </summary>
	ConnectionOptions ConnectionOptions { get; }


	/// <summary>
	/// Connects the the remote client.
	/// </summary>
	/// <param name="token"> A token to cancel the current operation. </param>
	Task ConnectAsync(CancellationToken token = default);


	/// <summary>
	/// Disconnects from the remote client.
	/// </summary>
	/// <param name="token"> CancellationToken token = default </param>
	Task DisconnectAsync(CancellationToken token = default);


	/// <summary>
	/// Sends data to the remote connection.
	/// </summary>
	/// <param name="data"> The data buffer we want to send to the remote client. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken token = default);


	/// <summary>
	/// Receives data from the remote connection.
	/// </summary>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> The data that we received from the remote client. </returns>
	ValueTask<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken token = default);
}