using System;

using Google.Protobuf;

using System.Linq;

using Borealis.Networking.Messages;



namespace Borealis.Networking.Communication;


public readonly struct CommunicationPacket
{
	/// <summary>
	/// The <see cref="PacketIdentifier" /> of this message.
	/// </summary>
	public PacketIdentifier Identifier { get; }


	/// <summary>
	/// The packet payload.
	/// </summary>
	public ReadOnlyMemory<byte> Payload { get; }


	/// <summary>
	/// The packet we send to the drivers.
	/// </summary>
	private CommunicationPacket(ReadOnlyMemory<byte> payload)
	{
		Identifier = (PacketIdentifier)payload.Span[0];

		Payload = payload[1..];
	}


	/// <summary>
	/// A communication packet used for transmitting data.
	/// </summary>
	/// <param name="identifier"> The <see cref="PacketIdentifier" /> of the message. </param>
	/// <param name="payload"> The payload of the message. </param>
	private CommunicationPacket(PacketIdentifier identifier, ReadOnlyMemory<byte> payload)
	{
		Identifier = identifier;
		Payload = payload;
	}


	/// <summary>
	/// Reads a communication packet from a buffer.
	/// </summary>
	/// <param name="buffer"> The buffer we want to read from. </param>
	/// <returns> A <see cref="CommunicationPacket" /> representing the packet that was send. </returns>
	public static CommunicationPacket FromBuffer(ReadOnlyMemory<byte> buffer)
	{
		return new CommunicationPacket(buffer);
	}


	/// <summary>
	/// Creates an <see cref="CommunicationPacket" /> from the message that was inserted.
	/// </summary>
	/// <typeparam name="TMessage"> The message type we want to create a buffer from. </typeparam>
	/// <param name="message"> The message we want to create a buffer from. </param>
	/// <returns> A <see cref="CommunicationPacket" /> that can be send to the remote client. </returns>
	/// <exception cref="InvalidOperationException">
	/// When the <see cref="TMessage" />
	/// type has not been given and packet identifier.
	/// </exception>
	public static CommunicationPacket FromMessage<TMessage>(TMessage message) where TMessage : IMessage
	{
		// Guard
		if (!_messageIdentifierTypeLookupTable.ContainsValue(message.Descriptor.ClrType)) throw CreateUnknownPacketException();

		// Get the packet identifier.
		PacketIdentifier identifier = _messageIdentifierTypeLookupTable.Single(kp => kp.Value == message.Descriptor.ClrType).Key;

		return new CommunicationPacket(identifier, message.ToByteArray());
	}


	/// <summary>
	/// Creates a message from this packet.
	/// </summary>
	/// <typeparam name="TMessage"> The message type we want to create. </typeparam>
	/// <returns> The <see cref="TMessage" /> we wanted out of the packet. </returns>
	/// <exception cref="InvalidOperationException">
	/// When the <see cref="TMessage" />
	/// type has not been given and packet identifier.
	/// </exception>
	public TMessage ToMessage<TMessage>() where TMessage : IMessage<TMessage>, new()
	{
		// Guard
		if (_messageIdentifierTypeLookupTable[Identifier] != typeof(TMessage)) throw CreateUnknownPacketException();

		// Deserialize message.
		MessageParser<TMessage> parser = new MessageParser<TMessage>(() => new TMessage());
		TMessage message = parser.ParseFrom(Payload.Span);

		return message;
	}


	/// <summary>
	/// Creates a single buffer of the data that we want to transport.
	/// </summary>
	/// <remarks>
	/// Packet structure { Id(1 Byte) | Payload(N) }
	/// </remarks>
	/// <returns> The buffer of data that we want to send. </returns>
	public ReadOnlyMemory<byte> CreateBuffer()
	{
		byte[] buffer = new byte[Payload.Length + 1];
		buffer[0] = (byte)Identifier;
		Payload.ToArray().CopyTo(buffer, 1);

		return buffer;
	}


	#region Lookup Table

	private static readonly Dictionary<PacketIdentifier, Type> _messageIdentifierTypeLookupTable = new Dictionary<PacketIdentifier, Type>
	{
		{ PacketIdentifier.ConnectRequest, typeof(ConnectRequest) },
		{ PacketIdentifier.ConnectReply, typeof(ConnectReply) },

		{ PacketIdentifier.SetConfigurationRequest, typeof(SetConfigurationRequest) },
		{ PacketIdentifier.SetConfigurationReply, typeof(SetConfigurationReply) },

		{ PacketIdentifier.GetDriverStatusRequest, typeof(GetDriverStatusRequest) },
		{ PacketIdentifier.GetDriverStatusReply, typeof(GetDriverStatusReply) },

		{ PacketIdentifier.StartAnimationRequest, typeof(StartAnimationRequest) },
		{ PacketIdentifier.StopAnimationRequest, typeof(StopAnimationRequest) },
		{ PacketIdentifier.DisplayFrameRequest, typeof(DisplayFrameRequest) },

		{ PacketIdentifier.AnimationBufferRequest, typeof(AnimationBufferRequest) },
		{ PacketIdentifier.AnimationBufferReply, typeof(AnimationBufferReply) },

		{ PacketIdentifier.ClearLedstripRequest, typeof(ClearLedstripRequest) },

		{ PacketIdentifier.SuccessReply, typeof(SuccessReply) },
		{ PacketIdentifier.ErrorReply, typeof(ErrorReply) }
	};

	#endregion


	private static InvalidOperationException CreateUnknownPacketException()
	{
		return new InvalidOperationException("The type given does not match the packet identifier.");
	}
}