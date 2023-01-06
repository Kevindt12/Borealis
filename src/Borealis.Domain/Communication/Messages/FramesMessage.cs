using Borealis.Domain.Devices;
using Borealis.Domain.Effects;



namespace Borealis.Domain.Communication.Messages;


// Packet 
// | Ledstrip Index | Color Spectrum | Frame Length | Frame Data |
// |     1 Byte     |     1 Byte     |     4 Bytes  |  N Bytes   |



public sealed class FramesBufferMessage : MessageBase
{
    /// <summary>
    /// The ledstrip index of the device.
    /// </summary>
    public byte LedstripIndex { get; }

    /// <summary>
    /// What color spectrum we are using.
    /// </summary>
    public ColorSpectrum ColorSpectrum { get; }

    /// <summary>
    /// The colors of the ledstrip.
    /// </summary>
    public ReadOnlyMemory<PixelColor>[] Frames { get; }


    /// <summary>
    /// The frame used to send to drivers.
    /// </summary>
    /// <param name="ledstripIndex"> The index of the ledstrip. </param>
    /// <param name="colorSpectrum"> </param>
    /// <param name="colors"> The colors that we want to send. </param>
    public FramesBufferMessage(byte ledstripIndex, ColorSpectrum colorSpectrum, IEnumerable<ReadOnlyMemory<PixelColor>> frames)
    {
        LedstripIndex = ledstripIndex;
        ColorSpectrum = colorSpectrum;
        Frames = frames.ToArray();
    }


    /// <summary>
    /// The frame used to send to drivers.
    /// </summary>
    /// <param name="buffer"> The buffer we want to read the data from. </param>
    /// <returns> A <see cref="FrameMessage" /> that has been deserialized. </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A exception thrown when the
    /// <see cref="ColorSpectrum" /> is not in range.
    /// </exception>
    public static FramesBufferMessage FromBuffer(ReadOnlyMemory<byte> buffer)
    {
        // Next, split the serialized bytes into two arrays, one for the LedstripIndex and one for the LedstripFrames
        byte ledstripIndex = buffer.Span[0];
        ColorSpectrum colorSpectrum = (ColorSpectrum)buffer.Span[1];
        int frameLength = BitConverter.ToInt32(buffer[2..6].Span);
        ReadOnlyMemory<byte> framesBufferBytes = buffer.Slice(6);

        int frameCount = framesBufferBytes.Length / FrameSerializer.CalculateByteLength(colorSpectrum, frameLength);

        // And create a list of LedstripFrame objects from the framesBytes
        ReadOnlyMemory<PixelColor>[] frames = new ReadOnlyMemory<PixelColor>[frameCount];

        for (int i = 0; i < frameCount; i++)
        {
            ReadOnlyMemory<PixelColor> frame = FrameSerializer.DeserializeFrame(framesBufferBytes, colorSpectrum, frameLength * i, frameLength);

            frames[i] = frame;
        }

        // Finally, use the ledstripIndex and frame values to construct and return a new FramesBufferMessage object
        return new FramesBufferMessage(ledstripIndex, colorSpectrum, frames);
    }


    /// <inheritdoc />
    public override ReadOnlyMemory<Byte> Serialize()
    {
        int frameBufferLength = FrameSerializer.CalculateByteLength(ColorSpectrum, Frames.FirstOrDefault().Length);

        byte[] buffer = new Byte[6 + frameBufferLength * Frames.Length];

        buffer[0] = LedstripIndex;
        buffer[1] = (byte)ColorSpectrum;
        BitConverter.GetBytes(Frames.FirstOrDefault().Length).CopyTo(buffer, 2);

        for (int i = 0; i < Frames.Length; i++)
        {
            FrameSerializer.SerializeFrame(buffer, ColorSpectrum, i * frameBufferLength + 6, Frames[i]);
        }

        // Finally, return the serialized bytes as a ReadOnlyMemory<byte>
        return buffer;
    }
}