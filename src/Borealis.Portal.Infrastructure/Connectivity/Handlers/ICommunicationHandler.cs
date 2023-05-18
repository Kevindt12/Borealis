using System.Net.Sockets;

using Borealis.Portal.Infrastructure.Communication;



namespace Borealis.Portal.Infrastructure.Connectivity.Handlers;


/// <summary>
/// The handler that does all the low level tcp connection items.
/// </summary>
internal interface ICommunicationHandler : IDisposable, IAsyncDisposable
{
	/// <summary>
	/// The Tcp client that we are connected to.
	/// </summary>
	TcpClient TcpClient { get; }


	/// <summary>
	/// Sends a packet and waits to receive the packet.
	/// </summary>
	/// <param name="packet"> The <see cref="CommunicationPacket" /> that we want to send. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> The <see cref="CommunicationPacket" /> reply that we got from the device. </returns>
	Task<CommunicationPacket> SendWithReplyAsync(CommunicationPacket packet, CancellationToken token = default);
}