using Borealis.Portal.Domain.Connectivity.Connections;
using Borealis.Portal.Infrastructure.Communication;



namespace Borealis.Portal.Infrastructure.Connectivity.Connections;


internal interface IDeviceLedstripConnection : ILedstripConnection
{
	Task<CommunicationPacket> HandleAnimationBufferRequest(int count, CancellationToken token = default);
}