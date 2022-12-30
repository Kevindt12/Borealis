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
    Frame = 10,

    Start = 6,

    Stop = 7,


    /// <summary>
    /// Indicating that we have a packet with frames for the device to buffer ready to display.
    /// </summary>
    Frames = 11,

    /// <summary>
    /// A packet that wants to indicate that we want to connect.
    /// </summary>
    Connect = 3,

    /// <summary>
    /// Sends the size of the drivers buffer. This so we can or speed up or slow down the packages that we are sending.
    /// </summary>
    BufferSize = 8,


    /// <summary>
    /// A packet indicating that we want to disconnect.
    /// </summary>
    Disconnect = 4,

    /// <summary>
    /// A configuration message that is used to set the new configuration
    /// </summary>
    Configuration = 5,

    /// <summary>
    /// A error message we want to be relayed.
    /// </summary>
    Error = 99
}