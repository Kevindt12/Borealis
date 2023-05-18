using Borealis.Networking.IO;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace Borealis.Networking.Connections;


internal class ConnectionFactory : IConnectionFactory
{
	private readonly ILoggerFactory _loggerFactory;
	private readonly IOptions<ConnectionOptions> _options;


	public ConnectionFactory(ILoggerFactory loggerFactory, IOptions<ConnectionOptions> options)
	{
		_loggerFactory = loggerFactory;
		_options = options;
	}


	/// <inheritdoc />
	public IConnection CreateConnection(ISocket socket)
	{
		return new Connection(_loggerFactory.CreateLogger<Connection>(), socket, _options.Value);
	}
}