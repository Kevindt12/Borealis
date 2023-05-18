using System.Net;



namespace Borealis.Networking.IO;


/// <summary>
/// A socket server used to allow clients to connect with us.
/// </summary>
public interface ISocketServer : IDisposable
{
	/// <summary>
	/// The local <see cref="EndPoint" /> of this server/
	/// </summary>
	EndPoint LocalEndPoint { get; }


	/// <summary>
	/// Indicates that this socket is running and listening for clients.
	/// </summary>
	bool IsRunning { get; }


	/// <summary>
	/// Starts the socket and allows clients to connect to this socket.
	/// </summary>
	/// <param name="token"> A token to cancel the current operation. </param>
	Task StartAsync(CancellationToken token = default);


	/// <summary>
	/// Stops the socket and clears all awaiting sockets.
	/// </summary>
	/// <param name="token"> A token to cancel the current operation. </param>
	Task StopAsync(CancellationToken token = default);


	/// <summary>
	/// Listens for a socket to be connected.
	/// </summary>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> A <see cref="ISocket" /> that we can use to communicate with the remote connection. </returns>
	Task<ISocket> AcceptSocketAsync(CancellationToken token = default);
}