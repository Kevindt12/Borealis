using Borealis.Domain.Communication.Exceptions;



namespace Borealis.Domain.Communication.Messages;


public class StopAnimationMessage : MessageBase
{
    /// <summary>
    /// The ledstrip that we want to stop the animation and clear the strip.
    /// </summary>
    /// <remarks>
    /// If the ledstrip has no animation but a solid color this will clear the color.
    /// </remarks>
    public byte LedstripIndex { get; init; }


    /// <summary>
    /// Creates a error message from a received buffer.
    /// </summary>
    /// <param name="buffer"> The buffer we received. </param>
    /// <returns> A <see cref="ErrorMessage" /> that has been populated. </returns>
    /// <exception cref="CommunicationException"> When the format of the payload is not correct. </exception>
    public static StopAnimationMessage FromBuffer(ReadOnlyMemory<byte> buffer)
    {
        // Finally, use the ledstripIndex value to construct and return a new StopAnimationMessage object
        return new StopAnimationMessage(buffer.Span[0]);
    }


    /// <summary>
    /// A error message based on a <see cref="string" /> message.
    /// </summary>
    /// <param name="frameDelay"> The string message error. </param>
    public StopAnimationMessage(byte ledstripIndex)
    {
        LedstripIndex = ledstripIndex;
    }


    /// <inheritdoc />
    public override ReadOnlyMemory<Byte> Serialize()
    {
        byte[] serializedBytes = new byte[1] { LedstripIndex };

        // Finally, return the serialized bytes as a ReadOnlyMemory<byte>
        return serializedBytes;
    }
}