using Borealis.Drivers.RaspberryPi.Sharp.Connection.Context;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Transmission;
using Borealis.Networking.Connections;
using Borealis.Networking.Protocol;
using Borealis.Networking.Transmission;

using Microsoft.Extensions.Logging;



namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Managers;


public class ConnectionManager : IConnectionManager
{
	private readonly ILogger<ConnectionManager> _logger;
	private readonly ConnectionContext _connectionContext;
	private readonly IChannelFactory _channelFactory;
	private readonly IMessageTransmitterAbstractFactory _messageTransmitterAbstractFactory;


	public ConnectionManager(ILogger<ConnectionManager> logger,
							 ConnectionContext connectionContext,
							 IChannelFactory channelFactory,
							 IMessageTransmitterAbstractFactory messageTransmitterAbstractFactory)
	{
		_logger = logger;
		_connectionContext = connectionContext;
		_channelFactory = channelFactory;
		_messageTransmitterAbstractFactory = messageTransmitterAbstractFactory;
	}


	/// <inheritdoc />
	public async Task SetCurrentConnectionAsync(IConnection connection, CancellationToken token = default)
	{
		_logger.LogDebug($"Setting current connection from {connection.Socket.RemoteEndPoint}.");

		connection.ConnectionDisconnected += OnConnectionDisconnectAsync;

		// Creating the channel
		IChannel channel = _channelFactory.CreateChannel(connection);

		// Creating the transmitter
		DriverMessageTransmitter messageTransmitter = _messageTransmitterAbstractFactory.CreateMessageTransmitter<DriverMessageTransmitter>(channel);

		_logger.LogTrace("Setting the connection context and tracking the connection.");
		_connectionContext.SetCurrentTransmitter(messageTransmitter);
	}


	private async Task OnConnectionDisconnectAsync(Object? sender, ConnectionDisconnectedEventArgs e)
	{
		await _connectionContext.ClearCurrentConnectionAsync();
	}


	/// <summary>
	/// Checks if we have a connection.
	/// </summary>
	/// <returns> A bool indicating that we have an connection. </returns>
	public virtual bool HasConnection()
	{
		return _connectionContext.CurrentMessageTransmitter != null;
	}


	/// <summary>
	/// Disconnects from the portal.
	/// </summary>
	/// <returns> </returns>
	/// <exception cref="InvalidOperationException"> </exception>
	public virtual async Task DisconnectAsync(CancellationToken token = default)
	{
		if (!HasConnection()) throw new InvalidOperationException("Cannot disconnect because there is no connection.");

		_logger.LogTrace("Disconnecting from the portal.");
		await _connectionContext.ClearCurrentConnectionAsync(token).ConfigureAwait(false);

		_logger.LogTrace("Disconnected from the portal.");
	}
}