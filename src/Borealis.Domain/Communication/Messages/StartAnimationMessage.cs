using Borealis.Domain.Communication.Exceptions;
using Borealis.Domain.Devices;
using Borealis.Domain.Effects;

using UnitsNet;



namespace Borealis.Domain.Communication.Messages;


// Packet 
// | Ledstrip Index |  Frequency  |  Frame Data |
// |     1 Byte     |   8 Byte    |  N Bytes   |



public class StartAnimationMessage : MessageBase
{
    /// <summary>
    /// The frequency at which the device should play the animation at.
    /// </summary>
    public Frequency Frequency { get; init; }

    /// <summary>
    /// The index of the ledstrip that we want to play the animation at.
    /// </summary>
    public byte LedstripIndex { get; init; }

    private ColorSpectrum ColorSpectrum { get; }

    /// <summary>
    /// The initial frame buffer that is send when starting the animation.
    /// </summary>
    public IEnumerable<ReadOnlyMemory<PixelColor>> InitialFrameBuffer { get; set; }


    /// <summary>
    /// Creates a error message from a received buffer.
    /// </summary>
    /// <param name="buffer"> The buffer we received. </param>
    /// <returns> A <see cref="ErrorMessage" /> that has been populated. </returns>
    /// <exception cref="CommunicationException"> When the format of the payload is not correct. </exception>
    public static StartAnimationMessage FromBuffer(ReadOnlyMemory<byte> buffer)
    {
        // First, create a new byte array with the same length as the buffer
        byte[] serializedBytes = buffer.ToArray();
        buffer.CopyTo(serializedBytes);

        // Next, split the serialized bytes into two arrays, one for the Frequency and one for the LedstripIndex
        byte ledstripIndex = serializedBytes[0];

        // Then, convert the frequencyBytes array back into a double
        double frequency = BitConverter.ToDouble(serializedBytes, 1);

        // Reading the under laying frames buffer message
        FramesBufferMessage framesBufferMessage = FramesBufferMessage.FromBuffer(buffer[9..]);

        // Finally, use the frequency and ledstripIndex values to construct and return a new StartAnimationMessage object
        return new StartAnimationMessage(Frequency.FromHertz(frequency), ledstripIndex, framesBufferMessage.ColorSpectrum, framesBufferMessage.Frames);
    }


    /// <inheritdoc />
    public StartAnimationMessage(Frequency frequency, byte ledstripIndex, ColorSpectrum colorSpectrum, IEnumerable<ReadOnlyMemory<PixelColor>> initialFrameBuffer)
    {
        Frequency = frequency;
        LedstripIndex = ledstripIndex;
        ColorSpectrum = colorSpectrum;
        InitialFrameBuffer = initialFrameBuffer;
    }


    /// <inheritdoc />
    public override ReadOnlyMemory<Byte> Serialize()
    {
        FramesBufferMessage initialFrameBufferWrapper = new FramesBufferMessage(LedstripIndex, ColorSpectrum, InitialFrameBuffer);

        // Serialize the initial frame buffer.
        ReadOnlyMemory<byte> initialFrameBufferWrapperBuffer = initialFrameBufferWrapper.Serialize();

        // First, convert the Frequency.Hertz property to a byte array
        byte[] frequencyBytes = BitConverter.GetBytes(Frequency.Hertz);

        // Next, create a new byte array with a length equal to the size of both properties
        // in bytes, and copy the frequencyBytes and ledstripIndexBytes arrays into it
        byte[] serializedBytes = new byte[1 + frequencyBytes.Length + initialFrameBufferWrapperBuffer.Length];
        serializedBytes[0] = LedstripIndex;
        frequencyBytes.CopyTo(serializedBytes, 1);
        initialFrameBufferWrapperBuffer.ToArray().CopyTo(serializedBytes, 9);

        // Finally, return the serialized bytes as a ReadOnlyMemory<byte>
        return serializedBytes;
    }
}