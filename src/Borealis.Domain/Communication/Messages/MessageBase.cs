namespace Borealis.Domain.Communication.Messages;


/// <summary>
/// The message that we want to send to a device or a device to us.
/// </summary>
public abstract class MessageBase
{
    /// <summary>
    /// The message that we want to send to a device or a device to us.
    /// </summary>
    protected MessageBase() { }


    /// <summary>
    /// Serializes the message into a buffer to be send.
    /// </summary>
    /// <returns> A <see cref="ReadOnlyMemory{Byte}" /> of bytes as the buffer to be send. </returns>
    public abstract ReadOnlyMemory<byte> SerializeMessage();
}