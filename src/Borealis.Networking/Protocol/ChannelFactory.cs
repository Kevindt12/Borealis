using Borealis.Networking.Connections;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace Borealis.Networking.Protocol;


internal class ChannelFactory : IChannelFactory
{
	private readonly ILoggerFactory _loggerFactory;
	private readonly IOptions<ChannelOptions> _channelOptions;


	public ChannelFactory(ILoggerFactory loggerFactory, IOptions<ChannelOptions> channelOptions)
	{
		_loggerFactory = loggerFactory;
		_channelOptions = channelOptions;
	}


	/// <inheritdoc />
	public IChannel CreateChannel(IConnection connection)
	{
		return new Channel(_loggerFactory.CreateLogger<Channel>(), connection, _channelOptions.Value);
	}
}