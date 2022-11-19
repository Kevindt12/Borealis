namespace Borealis.Domain.Communication;


/// <summary>
/// The type of packet this is.
/// </summary>
public enum PacketIdentifier : byte
{
    /// <summary>
    /// A keep alive packet.
    /// </summary>
    KeepAlive = 0,

    /// <summary>
    /// The Acknowledgement message to indicate that we received a message.
    /// </summary>
    /// <remarks>
    /// Note that the Keep alive and frame message don't send back acknowledgments.
    /// </remarks>
    Acknowledge = 1,

    /// <summary>
    /// The packet we send when we want to send a frame.
    /// </summary>
    Frame = 2,

    /// <summary>
    /// A packet that wants to indicate that we want to connect.
    /// </summary>
    Connect = 3,


    /// <summary>
    /// A packet indicating that we want to disconnect.
    /// </summary>
    Disconnect = 4,

    /// <summary>
    /// A error message we want to be relayed.
    /// </summary>
    Error = 99
}