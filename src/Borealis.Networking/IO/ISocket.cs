using System.Net;



namespace Borealis.Networking.IO;


/// <summary>
/// The socket connection that we have with an remote client.
/// </summary>
public interface ISocket : IDisposable
{
	/// <summary>
	/// The remote endpoint we want to connect to.
	/// </summary>
	EndPoint RemoteEndPoint { get; }


	/// <summary>
	/// The data that is waiting in the receive buffer.
	/// </summary>
	int DataAvailable { get; }


	/// <summary>
	/// The flag indicating that we have an connection with an remote client.
	/// </summary>
	bool Connected { get; }


	/// <summary>
	/// Starts the connection with the remote endpoint.
	/// </summary>
	/// <param name="token"> A token to cancel the current operation. </param>
	Task ConnectAsync(CancellationToken token = default);


	/// <summary>
	/// Disconnects from the remote endpoint.
	/// </summary>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> </returns>
	Task DisconnectAsync(CancellationToken token = default);


	/// <summary>
	/// Sends a packet to the remote endpoint.
	/// </summary>
	/// <param name="data"> The data we want to send. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> A <see cref="int" /> with the amount of data send to the client. </returns>
	ValueTask<int> SendAsync(ReadOnlyMemory<byte> data, CancellationToken token = default);


	/// <summary>
	/// Receives data from the remote endpoint.
	/// </summary>
	/// <param name="data"> The data buffer that we want to store the data in. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> The amount of data that was stored into the buffer. </returns>
	ValueTask<int> ReceiveAsync(Memory<byte> data, CancellationToken token = default);


	/// <summary>
	/// Keeps at the socket buffer.
	/// </summary>
	/// <remarks>
	/// Note this wont take data from the buffer.
	/// </remarks>
	/// <param name="data"> The buffer that we want to read. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> How many bytes we read from the buffer. </returns>
	ValueTask<int> PeekAsync(Memory<byte> data, CancellationToken token = default);
}