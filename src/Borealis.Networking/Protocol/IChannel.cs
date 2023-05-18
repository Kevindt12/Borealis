using Borealis.Networking.Communication;
using Borealis.Networking.Connections;



namespace Borealis.Networking.Protocol;


/// <summary>
/// An channel that handles the communication between the clients.
/// </summary>
public interface IChannel
{
	/// <summary>
	/// The options for the channel.
	/// </summary>
	ChannelOptions ChannelOptions { get; }

	/// <summary>
	/// The <see cref="IConnection" /> that we will be using to communicate between the clients.
	/// </summary>
	IConnection Connection { get; }

	/// <summary>
	/// The receive handler used when we receive packets.
	/// </summary>
	ReceiveAsyncHandler? ReceiveAsyncHandler { get; set; }


	/// <summary>
	/// Opens a channel to the remote channel.
	/// </summary>
	/// <param name="token"> A token to cancel the current operation. </param>
	Task OpenChannelAsync(CancellationToken token = default);


	/// <summary>
	/// Closes the channel.
	/// </summary>
	/// <param name="token"> A token to cancel the current operation. </param>
	Task CloseChannelAsync(CancellationToken token = default);


	/// <summary>
	/// Sends a communication packet over the wire expecting and response from the remote client.
	/// </summary>
	/// <param name="packet"> The <see cref="CommunicationPacket" /> that we want to send. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> The <see cref="CommunicationPacket" /> response that we got from the client. </returns>
	ValueTask<CommunicationPacket> SendAsync(CommunicationPacket packet, CancellationToken token = default);
}