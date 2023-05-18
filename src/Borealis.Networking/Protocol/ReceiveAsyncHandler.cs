using System.Net;

using Borealis.Networking.Communication;



namespace Borealis.Networking.Protocol;


/// <summary>
/// The handler for receiving <see cref="CommunicationPacket" />s from remote a
/// <see cref="EndPoint" />
/// </summary>
/// <param name="receivedPacket">
/// The <see cref="CommunicationPacket" /> received from the remote
/// <see cref="EndPoint" />.
/// </param>
/// <param name="token"> A token to cancel the current operation. </param>
/// <returns>
/// The reply <see cref="CommunicationPacket" /> that we want to send to the remote
/// <see cref="EndPoint" />.
/// </returns>
public delegate ValueTask<CommunicationPacket> ReceiveAsyncHandler(CommunicationPacket receivedPacket, CancellationToken token = default);