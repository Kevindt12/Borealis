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
    /// Acknowledgement that we processed the message that we have send.
    /// </summary>
    Acknowledgement = 1,


    /// <summary>
    /// A packet indicating that we want to disconnect.
    /// </summary>
    Disconnect = 3,

    /// <summary>
    /// The connect message that we send to the driver.
    /// </summary>
    Connect = 4,

    /// <summary>
    /// The response message to the connect message before.
    /// </summary>
    Connected = 5,


    /// <summary>
    /// Starts the animation on a ledstrip.
    /// </summary>
    StartAnimation = 6,

    /// <summary>
    /// Stops a animation on a ledstrip.
    /// </summary>
    StopAnimation = 7,


    /// <summary>
    /// Sends a single frame to the ledstrip
    /// </summary>
    Frame = 8,

    /// <summary>
    /// Sends a frame buffer to the device.
    /// </summary>
    FramesBuffer = 9,

    /// <summary>
    /// Sends a request from the device to the portal for a frame buffer.
    /// </summary>
    FramesBufferRequest = 10,


    /// <summary>
    /// A configuration message that is used to set the new configuration
    /// </summary>
    Configuration = 20,

    /// <summary>
    /// A error message we want to be relayed.
    /// </summary>
    Error = 99
}