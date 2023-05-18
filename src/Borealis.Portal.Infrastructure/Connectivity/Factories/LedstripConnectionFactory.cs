using Borealis.Domain.Ledstrips;
using Borealis.Portal.Infrastructure.Connectivity.Connections;
using Borealis.Portal.Infrastructure.Connectivity.Handlers;
using Borealis.Portal.Infrastructure.Connectivity.Serialization;

using Microsoft.Extensions.Logging;



namespace Borealis.Portal.Infrastructure.Connectivity.Factories;


internal class LedstripConnectionFactory
{
	private readonly ILoggerFactory _loggerFactory;
	private readonly MessageSerializer _messageSerializer;


	public LedstripConnectionFactory(ILoggerFactory loggerFactory, MessageSerializer messageSerializer)
	{
		_loggerFactory = loggerFactory;
		_messageSerializer = messageSerializer;
	}


	public virtual IDeviceLedstripConnection Create(Ledstrip ledstrip, ICommunicationHandler communicationHandler)
	{
		return new LedstripConnection(_loggerFactory.CreateLogger<LedstripConnection>(), _messageSerializer, communicationHandler, ledstrip);
	}
}