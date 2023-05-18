using System;
using System.Linq;

using Google.FlatBuffers;



namespace Borealis.Portal.Infrastructure.Communication;


/// <summary>
/// A communicable packet to send to devices and back.
/// </summary>
internal readonly struct CommunicationPacket
{

    public static CommunicationPacket NullPacket = new CommunicationPacket();

    /// <summary>
    /// The identifier of the packet indicating what it is.
    /// </summary>
    public PacketIdentifier Identifier { get; init; }


    /// <summary>
    /// The packet payload.
    /// </summary>
    public ReadOnlyMemory<byte> Payload { get; init; }


    /// <summary>
    /// The packet we send to the drivers.
    /// </summary>
    private CommunicationPacket(ReadOnlyMemory<byte> payload)
    {
        Identifier = (PacketIdentifier)payload.Span[0];

        Payload = payload[1..];
    }


    public CommunicationPacket(PacketIdentifier identifier, ReadOnlyMemory<byte> payload)
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


    public ReadOnlyMemory<byte> CreateBuffer()
    {
        byte[] buffer = new byte[Payload.Length + 1];
        buffer[0] = (byte)Identifier;
        Payload.ToArray().CopyTo(buffer, 1);

        return buffer;
    }


    public ByteBuffer GetBuffer()
    {
        return new ByteBuffer(Payload.ToArray());
    }
}