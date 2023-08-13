using Borealis.Networking.Protocol;
using Borealis.Networking.Transmission;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Infrastructure.Connectivity.Protocol;


public class ControllerMessageTransmitter : MessageTransmitterBase
{
	/// <inheritdoc />
	public ControllerMessageTransmitter(ILogger logger, IChannel channel) : base(logger, channel) { }
}