using Borealis.Networking.Communication;
using Borealis.Networking.Exceptions;
using Borealis.Networking.Messages;
using Borealis.Networking.Protocol;

using Google.Protobuf;

using Microsoft.Extensions.Logging;



namespace Borealis.Networking.Transmission;


public abstract class MessageTransmitterBase
{
	private readonly ILogger _logger;

	/// <summary>
	/// The channel that we wil be using to communicate between the clients with.
	/// </summary>
	public virtual IChannel Channel { get; }


	/// <summary>
	/// The message transmitter used to communicate between the 2 clients.
	/// </summary>
	/// <param name="logger"> The logger that we will use to log information about the transmitting. </param>
	/// <param name="channel"> The channel that we will be using to communicate between. </param>
	protected MessageTransmitterBase(ILogger logger, IChannel channel)
	{
		_logger = logger;

		Channel = channel;
		Channel.ReceiveAsyncHandler = OrchestrateIncomingReceivingPacketAsync;
	}


	#region Receiving

	/// <summary>
	/// Handles the exception that could be thrown in the handlers.
	/// </summary>
	/// <remarks>
	/// This will handle all <see cref="Exception" /> but specifies a separate handler for
	/// <see cref="NotImplementedException" />.
	/// This can be overriden with more exception to handle.
	/// </remarks>
	/// <param name="exception"> The exception that was thrown in the handlers. </param>
	/// <returns> An <see cref="ErrorReply" /> that we will send to the client. </returns>
	protected virtual ErrorReply HandleProcessException(Exception exception)
	{
		if (exception is NotImplementedException)
		{
			return new ErrorReply
			{
				ErrorId = ErrorId.Unimplemented,
				Message = "This function has not been implemented by the remote client."
			};
		}

		return new ErrorReply
		{
			ErrorId = ErrorId.Internalerror,
			Message = $"There was an unknown exception thrown : {exception.GetType()}, {exception.Message}."
		};
	}


	/// <summary>
	/// Handles the requests that come from the channel.
	/// </summary>
	/// <param name="receivedPacket"> The request packet that we received. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> The <see cref="CommunicationPacket" /> that we are going to send back. </returns>
	private async ValueTask<CommunicationPacket> OrchestrateIncomingReceivingPacketAsync(CommunicationPacket receivedPacket, CancellationToken token)
	{
		CommunicationPacket packet = receivedPacket.Identifier switch
		{
			PacketIdentifier.ConnectRequest          => await HandleIncomingPacketAsync<ConnectRequest, ConnectReply>(HandleConnectRequestAsync, receivedPacket, token),
			PacketIdentifier.SetConfigurationRequest => await HandleIncomingPacketAsync<SetConfigurationRequest, SetConfigurationReply>(HandleSetConfigurationRequestAsync, receivedPacket, token),
			PacketIdentifier.GetDriverStatusRequest  => await HandleIncomingPacketAsync<GetDriverStatusRequest, GetDriverStatusReply>(HandleGetDriverStatusRequestAsync, receivedPacket, token),

			PacketIdentifier.StartAnimationRequest  => await HandleIncomingPacketAsync<StartAnimationRequest, SuccessReply>(HandleStartAnimationRequestAsync, receivedPacket, token),
			PacketIdentifier.StopAnimationRequest   => await HandleIncomingPacketAsync<StopAnimationRequest, SuccessReply>(HandleStopAnimationRequestAsync, receivedPacket, token),
			PacketIdentifier.AnimationBufferRequest => await HandleIncomingPacketAsync<AnimationBufferRequest, AnimationBufferReply>(HandleAnimationBufferRequestAsync, receivedPacket, token),
			PacketIdentifier.DisplayFrameRequest    => await HandleIncomingPacketAsync<DisplayFrameRequest, SuccessReply>(HandleDisplayFrameRequestAsync, receivedPacket, token),

			PacketIdentifier.ClearLedstripRequest => await HandleIncomingPacketAsync<ClearLedstripRequest, SuccessReply>(HandleClearLedstripRequestAsync, receivedPacket, token),

			_ => CommunicationPacket.FromMessage(UnknownPacketReply(receivedPacket))
		};

		return packet;
	}


	/// <summary>
	/// Handles the request handlers..
	/// </summary>
	/// <typeparam name="TRequest"> The <see cref="IMessage" /> request that we received from the client. </typeparam>
	/// <typeparam name="TReply"> The <see cref="IMessage" /> that we are going to send back to the client. </typeparam>
	/// <param name="handler"> The handler that we want to call. </param>
	/// <param name="packet"> The <see cref="CommunicationPacket" /> we want to handle and extract the data from. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> The <see cref="IMessage" /> message that we want to send back to the client. </returns>
	private async ValueTask<CommunicationPacket> HandleIncomingPacketAsync<TRequest, TReply>(Func<TRequest, CancellationToken, Task<TReply>> handler, CommunicationPacket packet, CancellationToken token) where TRequest : IMessage<TRequest>, new()
																																																		   where TReply : IMessage<TReply>, new()
	{
		try
		{
			IMessage message = await handler.Invoke(packet.ToMessage<TRequest>(), token);

			return CommunicationPacket.FromMessage(message);
		}
		catch (Exception e)
		{
			return CommunicationPacket.FromMessage(HandleProcessException(e));
		}
	}


	/// <summary>
	/// Handles the unknown packet that we received.
	/// </summary>
	/// <param name="packet"> The packet that we received. </param>
	/// <returns> A <see cref="ErrorReply" /> indicating that we received something that we could not handle. </returns>
	private ErrorReply UnknownPacketReply(CommunicationPacket packet)
	{
		string errorMessage = $"An unknown packet was received by the remote client with packet id {packet.Identifier}";
		_logger.LogError(errorMessage);

		return new ErrorReply
		{
			ErrorId = ErrorId.Communication,
			Message = errorMessage
		};
	}

	#endregion


	#region Sending Methods

	/// <summary>
	/// Sends the request to connect on the application layer to the remote client.
	/// </summary>
	/// <param name="request"> The <see cref="ConnectRequest" /> message that we want to send. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> The <see cref="ConnectReply" /> that we get from the remote client. </returns>
	public async Task<ConnectReply> SendConnectRequestAsync(ConnectRequest request, CancellationToken token = default)
	{
		return await SendRequestAsyncCore<ConnectRequest, ConnectReply>(request, PacketIdentifier.ConnectReply, token);
	}


	/// <summary>
	/// Sends the request to update the configuration on the remote client.
	/// </summary>
	/// <param name="request"> The <see cref="SetConfigurationRequest" /> that we want to send. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> The <see cref="SetConfigurationReply" /> that we get from the remote client. </returns>
	public async Task<SetConfigurationReply> SendSetConfigurationRequestAsync(SetConfigurationRequest request, CancellationToken token = default)
	{
		return await SendRequestAsyncCore<SetConfigurationRequest, SetConfigurationReply>(request, PacketIdentifier.SetConfigurationReply, token);
	}


	/// <summary>
	/// Sends a request to get the driver status from the remote client.
	/// </summary>
	/// <param name="request"> The <see cref="GetDriverStatusRequest" /> that we want to send to the remote client. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> The <see cref="GetDriverStatusReply" /> that we want to get from the remote client. </returns>
	public async Task<GetDriverStatusReply> GetDriverStatusRequestAsync(GetDriverStatusRequest request, CancellationToken token = default)
	{
		return await SendRequestAsyncCore<GetDriverStatusRequest, GetDriverStatusReply>(request, PacketIdentifier.GetDriverStatusReply, token);
	}


	/// <summary>
	/// Sends the request to start an animation on the remote client.
	/// </summary>
	/// <param name="request"> The <see cref="StartAnimationRequest" /> that we want to send to the remote client. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> Get a <see cref="SuccessReply" /> indicating that the operation was successful. </returns>
	public async Task<SuccessReply> StartAnimationRequestAsync(StartAnimationRequest request, CancellationToken token = default)
	{
		return await SendRequestAsyncCore<StartAnimationRequest, SuccessReply>(request, PacketIdentifier.SuccessReply, token);
	}


	/// <summary>
	/// Sends the request to stop an animation on the remote client.
	/// </summary>
	/// <param name="request"> The <see cref="StopAnimationRequest" /> that we want to send to the remote client. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> Get a <see cref="SuccessReply" /> indicating that the operation was successful </returns>
	public async Task<SuccessReply> StopAnimationRequestAsync(StopAnimationRequest request, CancellationToken token = default)
	{
		return await SendRequestAsyncCore<StopAnimationRequest, SuccessReply>(request, PacketIdentifier.SuccessReply, token);
	}


	/// <summary>
	/// Sends a request to get an frame buffer from the remote client.
	/// </summary>
	/// <param name="request"> The <see cref="AnimationBufferRequest" /> that we want to send to the remote client. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> The <see cref="AnimationBufferReply" /> that contains the frame buffer for the request. </returns>
	public async Task<AnimationBufferReply> AnimationBufferRequestAsync(AnimationBufferRequest request, CancellationToken token = default)
	{
		return await SendRequestAsyncCore<AnimationBufferRequest, AnimationBufferReply>(request, PacketIdentifier.AnimationBufferReply, token);
	}


	/// <summary>
	/// Sends a request to display a single frame on the device.
	/// </summary>
	/// <param name="request"> The <see cref="DisplayFrameRequest" /> that we want to send to the device. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> Get a <see cref="SuccessReply" /> indicating that the operation was successful </returns>
	public async Task<SuccessReply> DisplayFrameRequestAsync(DisplayFrameRequest request, CancellationToken token = default)
	{
		return await SendRequestAsyncCore<DisplayFrameRequest, SuccessReply>(request, PacketIdentifier.SuccessReply, token);
	}


	/// <summary>
	/// Sends a request that we want to clear all operations on the ledstrip.
	/// </summary>
	/// <param name="request"> The <see cref="ClearLedstripRequest" /> that we want to send to the remote client. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> Get a <see cref="SuccessReply" /> indicating that the operation was successful </returns>
	public async Task<SuccessReply> ClearLedstripAsync(ClearLedstripRequest request, CancellationToken token = default)
	{
		return await SendRequestAsyncCore<ClearLedstripRequest, SuccessReply>(request, PacketIdentifier.SuccessReply, token);
	}


	/// <summary>
	/// Sends a message to the remote client and waits for and reply.
	/// </summary>
	/// <typeparam name="TRequest"> The request message we want to send. </typeparam>
	/// <typeparam name="TReply"> The reply we are expecting from the remote client. </typeparam>
	/// <param name="request"> The request message we want to send. </param>
	/// <param name="expectedReply"> The type of message we ware expecting to receive from the remote client. </param>
	/// <param name="token"> A token to cancel the current operation. </param>
	/// <returns> The expected reply from the client. </returns>
	private async Task<TReply> SendRequestAsyncCore<TRequest, TReply>(TRequest request, PacketIdentifier expectedReply, CancellationToken token) where TRequest : IMessage<TRequest>, new()
																																				 where TReply : IMessage<TReply>, new()
	{
		// Checking token.
		token.ThrowIfCancellationRequested();

		// Creating packet.
		CommunicationPacket requestPacket = CommunicationPacket.FromMessage(request);

		// Sending packet.
		_logger.LogTrace("Sending request {request} to the remote client.", request);
		CommunicationPacket replyPacket = await Channel.SendAsync(requestPacket, token);

		// Reading reply
		_logger.LogTrace("Received {reply} from the remote client.", replyPacket.Identifier);
		if (replyPacket.Identifier == expectedReply)
		{
			_logger.LogTrace("Reading reply payload.");

			return replyPacket.ToMessage<TReply>();
		}

		// Handling unexpected packet types.
		throw CreateUnforeseenReplyException(replyPacket);
	}


	/// <summary>
	/// Handles unforeseen replies from the remote client.
	/// </summary>
	/// <param name="packet"> The <see cref="CommunicationPacket" /> that we received from the remote client. </param>
	/// <returns> An <see cref="Exception" /> that we are throwing. </returns>
	private Exception CreateUnforeseenReplyException(CommunicationPacket packet)
	{
		if (packet.Identifier == PacketIdentifier.ErrorReply)
		{
			ErrorReply reply = packet.ToMessage<ErrorReply>();

			if (reply.ErrorId == ErrorId.Unimplemented)
			{
				return new NotImplementedException($"The remote connection has not implemented the feature that we where requesting {packet.Identifier}.");
			}

			return new RemoteException(packet, $"Error packet received from remote endpoint, Type: {reply.ErrorId}, Message: {reply.Message}");
		}

		return new CommunicationException(packet, "The packet that we received was not the packet that we where expecting.");
	}

	#endregion


	#region To Be Implemented by Base Class

	protected virtual Task<ConnectReply> HandleConnectRequestAsync(ConnectRequest request, CancellationToken token)
	{
		return Task.FromException<ConnectReply>(new NotImplementedException());
	}


	protected virtual Task<SetConfigurationReply> HandleSetConfigurationRequestAsync(SetConfigurationRequest request, CancellationToken token)
	{
		return Task.FromException<SetConfigurationReply>(new NotImplementedException());
	}


	protected virtual Task<GetDriverStatusReply> HandleGetDriverStatusRequestAsync(GetDriverStatusRequest request, CancellationToken token)
	{
		return Task.FromException<GetDriverStatusReply>(new NotImplementedException());
	}


	protected virtual Task<SuccessReply> HandleStartAnimationRequestAsync(StartAnimationRequest request, CancellationToken token)
	{
		return Task.FromException<SuccessReply>(new NotImplementedException());
	}


	protected virtual Task<SuccessReply> HandleStopAnimationRequestAsync(StopAnimationRequest request, CancellationToken token)
	{
		return Task.FromException<SuccessReply>(new NotImplementedException());
	}


	protected virtual Task<AnimationBufferReply> HandleAnimationBufferRequestAsync(AnimationBufferRequest request, CancellationToken token)
	{
		return Task.FromException<AnimationBufferReply>(new NotImplementedException());
	}


	protected virtual Task<SuccessReply> HandleDisplayFrameRequestAsync(DisplayFrameRequest request, CancellationToken token)
	{
		return Task.FromException<SuccessReply>(new NotImplementedException());
	}


	protected virtual Task<SuccessReply> HandleClearLedstripRequestAsync(ClearLedstripRequest request, CancellationToken token)
	{
		return Task.FromException<SuccessReply>(new NotImplementedException());
	}

	#endregion
}