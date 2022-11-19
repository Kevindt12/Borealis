using Borealis.Domain.Communication.Messages;



namespace Borealis.Domain.Communication;


/// <summary>
/// A communicable packet to send to devices and back.
/// </summary>
public readonly struct CommunicationPacket
{
    private readonly PacketIdentifier? _acknowledgementIdentifier;


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
    public bool IsEmpty => Payload != null;

    // TODO: Make it look at the ID number not if the field is filled

    /// <summary>
    /// A flag indicating that this packet is a acknowledgement packet.
    /// </summary>
    public bool IsAcknowledgement => Identifier == PacketIdentifier.Acknowledge;


    /// <summary>
    /// A communicable packet to send to devices and back.
    /// </summary>
    /// <param name="acknowledgementIdentifier"> </param>
    private CommunicationPacket(PacketIdentifier acknowledgementIdentifier)
    {
        _acknowledgementIdentifier = acknowledgementIdentifier;
    }


    /// <summary>
    /// The packet we send to the drivers.
    /// </summary>
    private CommunicationPacket(ReadOnlyMemory<byte> payload)
    {
        Identifier = (PacketIdentifier)payload.Span[0];

        Payload = payload.Slice(1);
    }


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
    /// <param name="message"> </param>
    /// <returns> </returns>
    /// <exception cref="InvalidOperationException"> </exception>
    public static CommunicationPacket CreatePacketFromMessage(MessageBase message)
    {
        return new CommunicationPacket
        {
            Identifier = message switch
            {
                FrameMessage frameMessage => PacketIdentifier.Frame,
                ErrorMessage errorMessage => PacketIdentifier.Error,
                _                         => throw new InvalidOperationException("The message has not been implemented.")
            },
            Payload = message.SerializeMessage()
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
        // Making sure we can only make acknowledgements from non acknowledgement packets.
        if (_acknowledgementIdentifier != null) throw new InvalidOperationException("This packet is already a Acknowledgement packet.");

        return new CommunicationPacket(Identifier)
        {
            Identifier = PacketIdentifier.Acknowledge
        };
    }


    /// <summary>
    /// Returns the castes message payload that was received.
    /// </summary>
    /// <typeparam name="TMessageType"> </typeparam>
    /// <returns> </returns>
    public TMessageType ReadPayload<TMessageType>() where TMessageType : MessageBase?
    {
        return Identifier switch
        {
            PacketIdentifier.KeepAlive   => null,
            PacketIdentifier.Disconnect  => null,
            PacketIdentifier.Connect     => null,
            PacketIdentifier.Error       => new ErrorMessage(Payload!.Value) as TMessageType,
            PacketIdentifier.Frame       => new FrameMessage(Payload!.Value) as TMessageType,
            PacketIdentifier.Acknowledge => null
        };
    }


    /// <summary>
    /// Creates the final buffer to be send.
    /// </summary>
    /// <returns> </returns>
    public ReadOnlyMemory<byte> CreateBuffer()
    {
        int length = Payload?.Length ?? 0;

        // Creating the buffer wrapper around the frame.
        byte[] buffer = new byte[(Payload?.Length ?? 0) + 1];

        buffer[0] = (byte)Identifier;

        Payload?.ToArray().CopyTo(buffer, 1);

        return buffer;
    }
}