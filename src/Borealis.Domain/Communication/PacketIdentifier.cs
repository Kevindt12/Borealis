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
    /// Starts the animation on a ledstrip.
    /// </summary>
    StartAnimation = 4,

    /// <summary>
    /// Stops a animation on a ledstrip.
    /// </summary>
    StopAnimation = 5,

    /// <summary>
    /// Sends a single frame to the ledstrip
    /// </summary>
    Frame = 7,

    /// <summary>
    /// Sends a frame buffer to the device.
    /// </summary>
    FramesBuffer = 8,

    /// <summary>
    /// Sends a request from the device to the portal for a frame buffer.
    /// </summary>
    FramesBufferRequest = 9,

    /// <summary>
    /// A packet indicating that we want to disconnect.
    /// </summary>
    Disconnect = 2,

    /// <summary>
    /// A configuration message that is used to set the new configuration
    /// </summary>
    Configuration = 15,

    /// <summary>
    /// A error message we want to be relayed.
    /// </summary>
    Error = 99
}