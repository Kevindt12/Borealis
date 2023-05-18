namespace Borealis.Networking.Connections;


public class ConnectionOptions
{
	/// <summary>
	/// The keep alive connection options that we will use to see if an connection is still alive or not.
	/// </summary>
	public KeepAliveOptions KeepAlive { get; set; } = new KeepAliveOptions();


	/// <summary>
	/// Disposes the connection when the connection disconnects.
	/// </summary>
	public bool DisposeOnDisconnection { get; set; } = false;


	/// <summary>
	/// The options for the connection that we want to use.
	/// </summary>
	public ConnectionOptions() { }
}