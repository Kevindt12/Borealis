using System.Net.Sockets;

using Borealis.Domain.Ledstrips;
using Borealis.Portal.Domain.Connectivity.Connections;
using Borealis.Portal.Domain.Connectivity.Models;
using Borealis.Portal.Domain.Devices.Models;
using Borealis.Portal.Domain.Exceptions;
using Borealis.Portal.Infrastructure.Connectivity.Factories;
using Borealis.Portal.Infrastructure.Connectivity.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace Borealis.Portal.Infrastructure.Connectivity.Connections;


internal class DeviceConnection : IDeviceConnection
{
	private readonly ILogger<DeviceConnection> _logger;


	private readonly LedstripConnectionFactory _ledstripConnectionFactory;
	private readonly CommunicationHandlerFactory _communicationHandlerFactory;

	private readonly ConnectivityOptions _connectivityOptions;

	private readonly List<IDeviceLedstripConnection> _ledstripConnections;
	private ICommunicationHandler _communicationHandler = default!;


	/// <summary>
	/// Thrown when we are disconnecting because of an error or just because we are disconnecting by request.
	/// </summary>
	public virtual event EventHandler? Disposing;

	/// <inheritdoc />
	public virtual Device Device { get; }

	/// <inheritdoc />
	public virtual IReadOnlyList<ILedstripConnection> LedstripConnections => _ledstripConnections.AsReadOnly();

	/// <inheritdoc />
	public virtual Boolean IsConfigurationValid { get; protected set; }


	/// <summary>
	/// The device connection that is used when communicating with the devices.
	/// </summary>
	public DeviceConnection(ILogger<DeviceConnection> logger,
							IOptions<ConnectivityOptions> connectivityOptions,
							MessageSerializer messageSerializer,
							LedstripConnectionFactory ledstripConnectionFactory,
							CommunicationHandlerFactory communicationHandlerFactory,
							Device device)
	{
		Device = device;

		_logger = logger;
		_messageSerializer = messageSerializer;
		_ledstripConnectionFactory = ledstripConnectionFactory;
		_communicationHandlerFactory = communicationHandlerFactory;

		_connectivityOptions = connectivityOptions.Value;

		_ledstripConnections = new List<IDeviceLedstripConnection>();
	}


	/// <inheritdoc />
	public async Task<DeviceConnectionResult> ConnectAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();

		if (_communicationHandler != null) throw new InvalidOperationException("The connection has already been initialized and cannot be initialized again.");

		// Starts the internal connection with the device.
		await StartInternalConnectionAsync(token).ConfigureAwait(false);

		// Start the application layer connection with the device.
		DeviceConnectionResult result = await StartProtocolConnectionAsync(token).ConfigureAwait(false);
		IsConfigurationValid = result.ConfigurationValid;

		// If the configuration is the same then setup the ledstrip connections.
		if (IsConfigurationValid)
		{
			SetupLedstripConnections();
		}

		return result;
	}


	/// <summary>
	/// Starts the internal tcp connection to the device.
	/// </summary>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <exception cref="DeviceConnectionException"> When we where unable to start the tcp connection with the device. </exception>
	protected virtual async Task StartInternalConnectionAsync(CancellationToken token = default)
	{
		TcpClient client = new TcpClient();
		ICommunicationHandler? communicationHandler = null;

		try
		{
			// Starting the tcp connection.
			_logger.LogTrace($"Starting the tcp connection to {Device.EndPoint}.");
			await client.ConnectAsync(Device.EndPoint, token).ConfigureAwait(false);

			// Creating the connection handler.
			_logger.LogTrace("Tcp connection is up initializing the connection.");
			communicationHandler = _communicationHandlerFactory.Create(client, HandleIncomingPacketAsync);

			// Setting the connection handler.
			_logger.LogTrace("Finalizing the connection with the device.");
			_communicationHandler = communicationHandler;
		}
		catch (SocketException socketException)
		{
			if (communicationHandler == null)
			{
				client?.Dispose();
			}
			else
			{
				await communicationHandler.DisposeAsync();
			}

			throw new DeviceConnectionException("Unable to create connection with the device.", socketException, Device);
		}
	}


	/// <summary>
	/// Starts the application layer connection process.
	/// </summary>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> A <see cref="DeviceConnectionResult" /> indicating the result of the connection. </returns>
	protected virtual async Task<DeviceConnectionResult> StartProtocolConnectionAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();

		// Creating the request packet.
		CommunicationPacket requestPacket = _messageSerializer.SerializeConnectRequest(Device.ConfigurationConcurrencyToken);

		try
		{
			// Sending the packet and receiving the reply.
			_logger.LogDebug($"Sending application connection request to the device with concurrency token :{Device.ConfigurationConcurrencyToken}.");
			CommunicationPacket replyPacket = await _communicationHandler.SendWithReplyAsync(requestPacket, token).ConfigureAwait(false);

			_logger.LogDebug("Reply received from the device.");
			if (replyPacket.Identifier != PacketIdentifier.ConnectReply)
			{
				if (replyPacket.Identifier == PacketIdentifier.ErrorReply)
				{
					throw CreateErrorReplyException(replyPacket);
				}

				throw CreateUnknownPacketException(replyPacket);
			}

			// Read the packet
			_messageSerializer.DeserializeConnectReply(replyPacket, out bool configurationConcurrencyTokenChanged);
			_logger.LogDebug($"The device configuration has changed : {configurationConcurrencyTokenChanged}.");

			return new DeviceConnectionResult
			{
				ConfigurationValid = configurationConcurrencyTokenChanged
			};
		}
		catch (SocketException socketException)
		{
			await DisposeAsyncCore().ConfigureAwait(false);

			throw new DeviceConnectionException("There was an problem with the connection of the device.", socketException, Device);
		}
		catch (Exception e)
		{
			await DisposeAsyncCore().ConfigureAwait(false);

			throw;
		}
	}


	/// <summary>
	/// Sets up the ledstrip connections.
	/// </summary>
	private void SetupLedstripConnections()
	{
		// Clearing if there are any connections.
		if (_ledstripConnections.Any())
		{
			_logger.LogTrace($"Cleaning up the current ledstrip connections of device {Device}.");
			_ledstripConnections.Clear();
		}

		_logger.LogTrace("Setting up the ledstrip connections.");
		_ledstripConnections.AddRange(CreateLedstripConnections(_communicationHandler));
	}


	/// <summary>
	/// Creates the ledstrip connections.
	/// </summary>
	/// <param name="communicationHandler"> The handler that handles the communication between the portal and the driver. </param>
	/// <returns> Returns a collection of ledstrip connections that can be used to interact with the ledstrip. </returns>
	protected virtual IEnumerable<IDeviceLedstripConnection> CreateLedstripConnections(ICommunicationHandler communicationHandler)
	{
		IReadOnlyList<Ledstrip> ledstrips = Device.Ports.Where(x => x.Ledstrip != null).Select(x => x.Ledstrip!).ToList();

		for (byte i = 0; i < ledstrips.Count; i++)
		{
			yield return _ledstripConnectionFactory.Create(ledstrips[i], communicationHandler);
		}
	}


	/// <returns> A <see cref="DeviceException" /> Thrown when there is a problem at the device. </returns>
	/// <returns> A <see cref="DeviceCommunicationException" /> Thrown when there is a problem with the communication between the portal and the device. </returns>
	/// <exception cref="DeviceConnectionException"> Thrown when there is a problem with the connection between the driver and the portal. </exception>
	public virtual async Task UploadConfigurationAsync(CancellationToken token = default)
	{
		if (_communicationHandler == null) throw new InvalidOperationException("The connection has not been initialized.");

		_logger.LogTrace("Sending set configuration packet to the driver.");
		CommunicationPacket setConfigurationRequestPacket = _messageSerializer.SerializeSetConfigurationRequest(Device.ConfigurationConcurrencyToken, Device.Ports);

		CommunicationPacket replyPacket = await _communicationHandler.SendWithReplyAsync(setConfigurationRequestPacket, token).ConfigureAwait(false);
		_logger.LogTrace("Received the set configuration reply from the device.");

		// Checking if we have the correct packet.
		if (replyPacket.Identifier != PacketIdentifier.SetConfigurationReply)
		{
			if (replyPacket.Identifier == PacketIdentifier.ErrorReply)
			{
				throw CreateErrorReplyException(replyPacket);
			}

			throw CreateUnknownPacketException(replyPacket);
		}

		// Deserialize the packet.
		_messageSerializer.DeserializeSetConfigurationReplyPacket(replyPacket, out bool success, out string? errorMessage);

		// If we could not change the configuration throw an exception.
		if (!success)
		{
			IsConfigurationValid = false;

			CleanUpLedstripConnections();

			throw new DeviceConfigurationException($"Unable to set the configuration for the device, Device error: {errorMessage}", Device);
		}

		// Setup the ledstrip connection once the configuration has changed.
		_logger.LogTrace("Configuration ahs been updated.");
		IsConfigurationValid = true;
		SetupLedstripConnections();
	}


	/// <inheritdoc />
	public virtual async Task<DeviceStatus> RequestStatusAsync(CancellationToken token = default)
	{
		if (_communicationHandler == null) throw new InvalidOperationException("The connection has not been initialized.");

		// Creating message.
		_logger.LogTrace("Requesting the status from the driver.");
		CommunicationPacket requestPacket = _messageSerializer.SerializeGetDriverStatusRequestPacket();

		// Send and receive the packet.
		CommunicationPacket replyPacket = await _communicationHandler.SendWithReplyAsync(requestPacket, token).ConfigureAwait(false);

		// Reading the communication packet.
		_logger.LogTrace("Reading the status from the driver.");
		_messageSerializer.DeserializeGetDriverStatusRequest(replyPacket, out IReadOnlyDictionary<Guid, LedstripStatus> ledstripStatuses);

		// Getting the response of the packet
		return new DeviceStatus(ledstripStatuses.ToDictionary(key => _ledstripConnections.Single(connection => connection.Ledstrip.Id == key.Key).Ledstrip, s => s.Value));
	}


	/// <summary>
	/// Handles a incoming packet.
	/// </summary>
	/// <param name="packet"> The <see cref="CommunicationPacket" /> that we received. </param>
	protected virtual Task<CommunicationPacket> HandleIncomingPacketAsync(CommunicationPacket packet, CancellationToken token = default) => packet.Identifier switch
	{
		PacketIdentifier.AnimationBufferRequest => HandleAnimationBufferRequestAsync(packet, token),
		PacketIdentifier.ErrorReply             => HandleDeviceErrorAsync(packet, token),
		_                                       => HandleUnknownPacketAsync(packet, token)
	};


	protected virtual Task<CommunicationPacket> HandleDeviceErrorAsync(CommunicationPacket packet, CancellationToken token = default)
	{
		return Task.FromException<CommunicationPacket>(CreateErrorReplyException(packet));
	}


	protected virtual Task<CommunicationPacket> HandleUnknownPacketAsync(CommunicationPacket packet, CancellationToken token)
	{
		return Task.FromException<CommunicationPacket>(CreateUnknownPacketException(packet));
	}


	protected virtual async Task<CommunicationPacket> HandleAnimationBufferRequestAsync(CommunicationPacket packet, CancellationToken token)
	{
		// Reading the message from the driver
		_logger.LogTrace("Frame buffer request received.");
		_messageSerializer.DeserializeAnimationBufferRequestPacket(packet, out Guid ledstripId, out int amount);

		// Reading the next frame buffer.
		_logger.LogTrace($"Getting the next frame buffer for the ledstrip {ledstripId} with {amount} frames.");
		CommunicationPacket replyPacket = await _ledstripConnections.Single(l => l.Ledstrip.Id == ledstripId).HandleAnimationBufferRequest(amount, token).ConfigureAwait(false);

		// Returning the received frame buffer.
		_logger.LogTrace("Send buffer reply to the device.");

		return replyPacket;
	}


	/// <summary>
	/// Cleans up the ledstrip connections.
	/// </summary>
	private void CleanUpLedstripConnections()
	{
		_logger.LogTrace("Cleaning up the ledstrip connections");
		_ledstripConnections.Clear();
	}


	private DeviceException CreateErrorReplyException(CommunicationPacket packet)
	{
		_messageSerializer.DeserializeErrorReplyPacket(packet, out string errorMessage);

		return new DeviceException($"There was an error received from the device: {errorMessage}", Device);
	}


	private static DeviceCommunicationException CreateUnknownPacketException(CommunicationPacket packet)
	{
		return new DeviceCommunicationException($"There was an unknown packet received from the device, With packet id: {packet.Identifier}.")
		{
			Data = { { "Packet", packet } }
		};
	}


	#region IDisposable

	private bool _disposed;


	/// <inheritdoc />
	public void Dispose()
	{
		if (!_disposed) return;

		// Telling the application that we have started disposing of the connection to this device.
		Disposing?.Invoke(this, EventArgs.Empty);

		Dispose(true);
		GC.SuppressFinalize(this);

		_disposed = true;
	}


	protected virtual void Dispose(bool disposing) { }


	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		if (!_disposed) return;

		// Telling the application that we have started disposing of the connection to this device.
		Disposing?.Invoke(this, EventArgs.Empty);

		await DisposeAsyncCore().ConfigureAwait(false);

		Dispose(false);
		GC.SuppressFinalize(this);

		_disposed = true;
	}


	protected virtual async ValueTask DisposeAsyncCore()
	{
		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (_communicationHandler != null)
		{
			await _communicationHandler.DisposeAsync().ConfigureAwait(false);
		}
	}

	#endregion
}