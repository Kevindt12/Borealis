using System.Net.Sockets;

using Borealis.Portal.Infrastructure.Connectivity.Handlers;
using Borealis.Portal.Infrastructure.Connectivity.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace Borealis.Portal.Infrastructure.Connectivity.Factories;


internal class CommunicationHandlerFactory
{
	private readonly ILoggerFactory _loggerFactory;
	private readonly IOptions<ConnectivityOptions> _connectivityOptions;
	private readonly IOptions<KeepAliveOptions> _keepAliveOptions;


	public CommunicationHandlerFactory(ILoggerFactory loggerFactory, IOptions<ConnectivityOptions> connectivityOptions, IOptions<KeepAliveOptions> keepAliveOptions)
	{
		_loggerFactory = loggerFactory;
		_connectivityOptions = connectivityOptions;
		_keepAliveOptions = keepAliveOptions;
	}


	public virtual ICommunicationHandler Create(TcpClient client, ReceivedHandler handler)
	{
		return new CommunicationHandler(_loggerFactory.CreateLogger<CommunicationHandler>(), _connectivityOptions, _keepAliveOptions, client, handler);
	}
}