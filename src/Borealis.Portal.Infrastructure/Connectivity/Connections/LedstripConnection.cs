using Borealis.Domain.Ledstrips;
using Borealis.Portal.Domain.Common;
using Borealis.Portal.Domain.Connectivity.Connections;
using Borealis.Portal.Domain.Devices.Models;
using Borealis.Portal.Domain.Exceptions;
using Borealis.Portal.Infrastructure.Connectivity.Handlers;

using Microsoft.Extensions.Logging;

using UnitsNet;



namespace Borealis.Portal.Infrastructure.Connectivity.Connections;


internal class LedstripConnection : IDeviceLedstripConnection
{
	private readonly ILogger<LedstripConnection> _logger;
	private readonly MessageSerializer _messageSerializer;
	private readonly ICommunicationHandler _communicationHandler;

	/// <inheritdoc />
	public FrameBufferRequestHandler? FrameBufferRequestHandler { get; set; }

	/// <inheritdoc />
	public Ledstrip Ledstrip { get; }


	/// <summary>
	/// The ledstrip connection that is a mask for each ledstrip that is connected to a device.
	/// </summary>
	/// <param name="communication"> The <see cref="CommunicationHandler" /> that handles the communication between the driver and the portal. </param>
	/// <param name="ledstrip">
	/// The <see cref="Ledstrip" /> that we are connected with via the
	/// <see cref="DeviceConnection" />.
	/// </param>
	/// <param name="ledstripIndex"> The index of the ledstrip. </param>
	public LedstripConnection(ILogger<LedstripConnection> logger, MessageSerializer messageSerializer, ICommunicationHandler communication, Ledstrip ledstrip)
	{
		Ledstrip = ledstrip;
		_logger = logger;
		_messageSerializer = messageSerializer;
		_communicationHandler = communication;
	}


	/// <inheritdoc />
	public async Task<LedstripStatus> GetLedstripStatus()
	{
		throw new NotImplementedException();
	}


	/// <inheritdoc />
	/// <exception cref="InvalidOperationException">
	/// Thrown when the handler
	/// <see cref="FrameBufferRequestHandler" /> to get more frames when the driver requests it has not been set.
	/// </exception>
	/// <exception cref="DeviceException"> Thrown when there is a that the device experienced. </exception>
	/// <exception cref="DeviceCommunicationException"> Thrown when the device has problems with the communication. </exception>
	/// <exception cref="DeviceConnectionException"> Thrown when there is a problem with the connection with the device. </exception>
	public async Task StartAnimationAsync(Frequency frequency, ReadOnlyMemory<ReadOnlyMemory<PixelColor>> initialFrameBuffer, CancellationToken token = default)
	{
		if (FrameBufferRequestHandler == null) throw new InvalidOperationException("Cannot start an animation on a ledstrip where the request for frame buffer handler has not been set.");

		// Create the request packet.
		CommunicationPacket requestPacket = _messageSerializer.SerializeStartAnimationRequest(Ledstrip.Id, frequency, initialFrameBuffer.ToArray());

		// Sending the request
		CommunicationPacket replyPacket = await _communicationHandler.SendWithReplyAsync(requestPacket, token).ConfigureAwait(false);

		if (replyPacket.Identifier == PacketIdentifier.ErrorReply)
		{
			throw ErrorReplyException(replyPacket);
		}

		if (replyPacket.Identifier != PacketIdentifier.SuccessReply)
		{
			throw InvalidReplyException(replyPacket);
		}
	}


	/// <inheritdoc />
	public virtual async Task PauseAnimationAsync(CancellationToken token = default)
	{
		// Create the message.
		CommunicationPacket requestPacket = _messageSerializer.SerializePauseAnimationRequest(Ledstrip.Id);

		// Send the packet to the device.
		CommunicationPacket replyPacket = await _communicationHandler.SendWithReplyAsync(requestPacket, token).ConfigureAwait(false);

		if (replyPacket.Identifier == PacketIdentifier.ErrorReply)
		{
			throw ErrorReplyException(replyPacket);
		}

		if (replyPacket.Identifier != PacketIdentifier.SuccessReply)
		{
			throw InvalidReplyException(replyPacket);
		}
	}


	/// <inheritdoc />
	/// <exception cref="DeviceException"> Thrown when there is a that the device experienced. </exception>
	/// <exception cref="DeviceCommunicationException"> Thrown when the device has problems with the communication. </exception>
	/// <exception cref="DeviceConnectionException"> Thrown when there is a problem with the connection with the device. </exception>
	public async Task StopAnimationAsync(CancellationToken token = default)
	{
		CommunicationPacket requestPacket = _messageSerializer.SerializeStopAnimationRequest(Ledstrip.Id);

		// Send the packet to the device.
		CommunicationPacket replyPacket = await _communicationHandler.SendWithReplyAsync(requestPacket, token);

		if (replyPacket.Identifier == PacketIdentifier.ErrorReply)
		{
			throw ErrorReplyException(replyPacket);
		}

		if (replyPacket.Identifier != PacketIdentifier.SuccessReply)
		{
			throw InvalidReplyException(replyPacket);
		}
	}


	/// <inheritdoc />
	/// <exception cref="DeviceException"> Thrown when there is a that the device experienced. </exception>
	/// <exception cref="DeviceCommunicationException"> Thrown when the device has problems with the communication. </exception>
	/// <exception cref="DeviceConnectionException"> Thrown when there is a problem with the connection with the device. </exception>
	public async Task DisplayFrameAsync(ReadOnlyMemory<PixelColor> frame, CancellationToken token = default)
	{
		CommunicationPacket requestPacket = _messageSerializer.SerializeDisplayFrameRequest(Ledstrip.Id, frame);

		CommunicationPacket replyPacket = await _communicationHandler.SendWithReplyAsync(requestPacket, token);

		if (replyPacket.Identifier == PacketIdentifier.ErrorReply)
		{
			throw ErrorReplyException(replyPacket);
		}

		if (replyPacket.Identifier != PacketIdentifier.SuccessReply)
		{
			throw InvalidReplyException(replyPacket);
		}
	}


	/// <inheritdoc />
	public virtual async Task ClearAsync(CancellationToken token = default)
	{
		CommunicationPacket requestPacket = _messageSerializer.SerializeClearLedstripRequest(Ledstrip.Id);

		CommunicationPacket replyPacket = await _communicationHandler.SendWithReplyAsync(requestPacket, token);

		if (replyPacket.Identifier == PacketIdentifier.ErrorReply)
		{
			throw ErrorReplyException(replyPacket);
		}

		if (replyPacket.Identifier != PacketIdentifier.SuccessReply)
		{
			throw InvalidReplyException(replyPacket);
		}
	}


	public async Task<CommunicationPacket> HandleAnimationBufferRequest(int count, CancellationToken token = default)
	{
		// Getting the frame
		ReadOnlyMemory<ReadOnlyMemory<PixelColor>> frame = await FrameBufferRequestHandler!.Invoke(count).ConfigureAwait(false);

		CommunicationPacket replyPacket = _messageSerializer.SerializeAnimationBufferReply(frame.ToArray());

		return replyPacket;
	}


	private DeviceException ErrorReplyException(CommunicationPacket packet)
	{
		ErrorReply reply = ErrorReply.GetRootAsErrorReply(packet.GetBuffer());

		return new DeviceException($"There was a problem starting the ledstrip at the device, ErrorId: {reply.ErrorId} , Message: {reply.Message}.");
	}


	private DeviceCommunicationException InvalidReplyException(CommunicationPacket packet)
	{
		throw new DeviceCommunicationException("The packet that was received was not the packet we where expecting.");
	}
}