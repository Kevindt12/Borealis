namespace Borealis.Networking.Protocol;


public class ChannelOptions
{
	/// <summary>
	/// The timeout when we are waiting to receive an item from the client.
	/// </summary>
	public TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(10);


	/// <summary>
	/// The options for the channel that we have created.
	/// </summary>
	public ChannelOptions() { }
}