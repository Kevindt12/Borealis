using Borealis.Domain.Communication.Messages;



namespace Borealis.Domain.Communication;


/// <summary>
/// A communicable packet to send to devices and back.
/// </summary>
public readonly struct CommunicationPacket
{
    /// <summary>
    /// The identifier of the packet indicating what it is.
    /// </summary>
    public PacketIdentifier Identifier { get; init; }


    /// <summary>
    /// The packet payload.
    /// </summary>
    public ReadOnlyMemory<byte>? Payload { get; init; }

    /// <summary>
    /// Indicating that the packet has a payload attached.
    /// </summary>
    public bool IsEmpty => Payload == null;

    /// <summary>
    /// A flag indicating that this packet is a acknowledgement packet.
    /// </summary>
    public bool IsAcknowledgement => Identifier == PacketIdentifier.Acknowledge;


    /// <summary>
    /// The packet we send to the drivers.
    /// </summary>
    private CommunicationPacket(ReadOnlyMemory<byte> payload)
    {
        Identifier = (PacketIdentifier)payload.Span[0];

        Payload = payload[1..];
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
    /// Creates a keep alive message to be send to a other device.
    /// </summary>
    /// <returns> </returns>
    public static CommunicationPacket CreateKeepAlive()
    {
        return new CommunicationPacket
        {
            Identifier = PacketIdentifier.KeepAlive,
            Payload = null
        };
    }


    /// <summary>
    /// Creates a packet from a message.
    /// </summary>
    /// <param name="message"> The message we want to send. </param>
    /// <returns> A <see cref="CommunicationPacket" /> that wraps around the message to be send. </returns>
    /// <exception cref="InvalidOperationException"> When the message has not been implemented. </exception>
    public static CommunicationPacket CreatePacketFromMessage(MessageBase message)
    {
        return new CommunicationPacket
        {
            Identifier = message switch
            {
                FrameMessage         => PacketIdentifier.Frame,
                ConfigurationMessage => PacketIdentifier.Configuration,
                ErrorMessage         => PacketIdentifier.Error,
                FramesMessage        => PacketIdentifier.Frames,
                StackSizeMessage     => PacketIdentifier.BufferSize,
                StartMessage         => PacketIdentifier.Start,
                StopMessage          => PacketIdentifier.Stop,
                _                    => throw new InvalidOperationException("The message has not been implemented.")
            },
            Payload = message.Serialize()
        };
    }


    /// <summary>
    /// Creates a connection packet.
    /// </summary>
    /// <returns> Returns a instance packet of its self. </returns>
    public static CommunicationPacket CreateConnectionPacket()
    {
        return new CommunicationPacket
        {
            Identifier = PacketIdentifier.Connect
        };
    }


    /// <summary>
    /// Creates a disconnection packet.
    /// </summary>
    /// <returns> Returns a instance packet of its self. </returns>
    public static CommunicationPacket CreateDisconnectionPacket()
    {
        return new CommunicationPacket
        {
            Identifier = PacketIdentifier.Disconnect
        };
    }


    /// <summary>
    /// Creates a acknowledgement packet for the other side.
    /// </summary>
    /// <returns> A <see cref="CommunicationPacket" /> that is ready to send back. </returns>
    /// <exception cref="InvalidOperationException"> When the packet is already a acknowledgement packet </exception>
    public CommunicationPacket GenerateAcknowledgementPacket()
    {
        // Making sure we can only make acknowledgement from non acknowledgement packets.
        if (IsAcknowledgement) throw new InvalidOperationException("This packet is already a Acknowledgement packet.");

        return new CommunicationPacket
        {
            Identifier = PacketIdentifier.Acknowledge
        };
    }


    /// <summary>
    /// Returns the castes message payload that was received.
    /// </summary>
    /// <remarks>
    /// Note that not all packets have a payload. Read the messages documentation for more information.
    /// </remarks>
    /// <typeparam name="TMessageType"> The type of message that we need to read if we need to read it at all. </typeparam>
    /// <returns>
    /// <see cref="null" /> if there is no payload else it will read a message with a base class of
    /// <see cref="MessageBase" />.
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when the
    /// <see cref="PacketIdentifier" /> is not in a valid state.
    /// </exception>
    public TMessageType? ReadPayload<TMessageType>() where TMessageType : MessageBase?
    {
        return (Identifier switch
        {
            PacketIdentifier.KeepAlive     => null,
            PacketIdentifier.Disconnect    => null,
            PacketIdentifier.Connect       => null,
            PacketIdentifier.Error         => ErrorMessage.FromBuffer(Payload!.Value) as TMessageType,
            PacketIdentifier.Frame         => FrameMessage.FromBuffer(Payload!.Value) as TMessageType,
            PacketIdentifier.Configuration => ConfigurationMessage.FromBuffer(Payload!.Value) as TMessageType,
            PacketIdentifier.Frames        => FramesMessage.FromBuffer(Payload!.Value) as TMessageType,
            PacketIdentifier.BufferSize    => StackSizeMessage.FromBuffer(Payload!.Value) as TMessageType,
            PacketIdentifier.Start         => StartMessage.FromBuffer(Payload!.Value) as TMessageType,
            PacketIdentifier.Stop          => StopMessage.FromBuffer(Payload!.Value) as TMessageType,

            PacketIdentifier.Acknowledge => null,
            _                            => throw new IndexOutOfRangeException("The PacketIdentifier was out of range.")
        })!;
    }


    /// <summary>
    /// Creates the final buffer to be send.
    /// </summary>
    /// <remarks>
    /// Packet buffer being creates has the following format 1 bytes for the
    /// <see cref="Identifier" /> then the payload <see cref="Payload" />.
    /// </remarks>
    /// <returns> </returns>
    public ReadOnlyMemory<byte> CreateBuffer()
    {
        // Gets the length of the payload
        int payloadLength = Payload?.Length ?? 0;

        // Creating the buffer wrapper around the frame.
        byte[] buffer = new byte[(Payload?.Length ?? 0) + 1];

        // Writing the id.
        buffer[0] = (byte)Identifier;

        // Writing the payload.
        Payload?.ToArray().CopyTo(buffer, 1);

        // Sending the buffer back.
        return buffer;
    }
}