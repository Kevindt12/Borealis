using Borealis.Domain.Devices;
using Borealis.Domain.Effects;



namespace Borealis.Domain.Communication.Messages;


// Packet 
// | Ledstrip Index | Color Spectrum | Frame Length | Frame Data |
// |     1 Byte     |     1 Byte     |     4 Bytes  |  N Bytes   |



public sealed class FrameMessage : MessageBase
{
    /// <summary>
    /// The ledstrip index of the device.
    /// </summary>
    public byte LedstripIndex { get; }


    public ColorSpectrum ColorSpectrum { get; set; }


    /// <summary>
    /// The frame that we want to display on the ledstrip.
    /// </summary>
    public ReadOnlyMemory<PixelColor> Frame { get; init; }


    /// <summary>
    /// The frame used to send to drivers.
    /// </summary>
    /// <param name="ledstripIndex"> The index of the ledstrip. </param>
    /// <param name="colorSpectrum"> </param>
    /// <param name="frame"> The colors that we want to send. </param>
    public FrameMessage(byte ledstripIndex, ColorSpectrum colorSpectrum, ReadOnlyMemory<PixelColor> frame)
    {
        LedstripIndex = ledstripIndex;
        ColorSpectrum = colorSpectrum;
        Frame = frame;
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
    public static FrameMessage FromBuffer(ReadOnlyMemory<byte> buffer)
    {
        // Next, split the serialized bytes into two arrays, one for the LedstripIndex and one for the LedstripFrame
        byte ledstripIndex = buffer.Span[0];

        // Read the ColorSpectrum value from the serializedBytes array
        ColorSpectrum colorSpectrum = (ColorSpectrum)buffer.Span[1];

        int frameLength = BitConverter.ToInt32(buffer[2..5].Span);

        ReadOnlyMemory<PixelColor> frame = FrameSerializer.DeserializeFrame(buffer, colorSpectrum, 6, frameLength);

        // Finally, use the ledstripIndex and frame values to construct and return a new FrameMessage object
        return new FrameMessage(ledstripIndex, colorSpectrum, frame);
    }


    /// <inheritdoc />
    public override ReadOnlyMemory<Byte> Serialize()
    {
        int frameBufferLength = FrameSerializer.CalculateByteLength(ColorSpectrum, Frame.Length);

        byte[] buffer = new Byte[6 + frameBufferLength];

        buffer[0] = LedstripIndex;
        buffer[1] = (byte)ColorSpectrum;
        BitConverter.GetBytes(Frame.Length).CopyTo(buffer, 2);

        FrameSerializer.SerializeFrame(buffer, ColorSpectrum, 6, Frame);

        // Finally, return the serialized bytes as a ReadOnlyMemory<byte>
        return buffer;
    }
}