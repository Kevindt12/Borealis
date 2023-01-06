using Borealis.Domain.Communication.Exceptions;



namespace Borealis.Domain.Communication.Messages;


public class FrameBufferRequestMessage : MessageBase
{
    /// <summary>
    /// The number of frames we are requesting.
    /// </summary>
    public int NumberOfFrames { get; init; }

    /// <summary>
    /// The index of the ledstrip that we want to play the animation at.
    /// </summary>
    public byte LedstripIndex { get; init; }


    /// <summary>
    /// Creates a error message from a received buffer.
    /// </summary>
    /// <param name="buffer"> The buffer we received. </param>
    /// <returns> A <see cref="ErrorMessage" /> that has been populated. </returns>
    /// <exception cref="CommunicationException"> When the format of the payload is not correct. </exception>
    public static FrameBufferRequestMessage FromBuffer(ReadOnlyMemory<byte> buffer)
    {
        // First, create a new byte array with the same length as the buffer
        byte[] serializedBytes = buffer.ToArray();

        // And convert the ledstripIndexBytes array back into an int
        byte ledstripIndex = serializedBytes[0];

        // Next, split the serialized bytes into two arrays, one for the NumberOfFrames and one for the LedstripIndex
        ReadOnlyMemory<byte> numberOfFramesBytes = buffer[1..5];

        // Then, convert the numberOfFramesBytes array back into an int
        int numberOfFrames = BitConverter.ToInt32(numberOfFramesBytes.ToArray(), 0);

        // Finally, use the numberOfFrames and ledstripIndex values to construct and return a new FrameBufferRequestMessage object
        return new FrameBufferRequestMessage(numberOfFrames, ledstripIndex);
    }


    /// <inheritdoc />
    public FrameBufferRequestMessage(Int32 numberOfFrames, byte ledstripIndex)
    {
        NumberOfFrames = numberOfFrames;
        LedstripIndex = ledstripIndex;
    }


    /// <inheritdoc />
    public override ReadOnlyMemory<Byte> Serialize()
    {
        // First, convert the NumberOfFrames property to a byte array
        byte[] numberOfFramesBytes = BitConverter.GetBytes(NumberOfFrames);

        // Next, create a new byte array with a length equal to the size of both properties
        // in bytes, and copy the numberOfFramesBytes and ledstripIndexBytes arrays into it
        byte[] serializedBytes = new byte[1 + numberOfFramesBytes.Length];
        serializedBytes[0] = LedstripIndex;
        numberOfFramesBytes.CopyTo(serializedBytes, 1);

        // Finally, return the serialized bytes as a ReadOnlyMemory<byte>
        return serializedBytes;
    }
}