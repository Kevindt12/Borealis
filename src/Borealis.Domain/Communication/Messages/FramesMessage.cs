using Borealis.Domain.Devices;
using Borealis.Domain.Effects;



namespace Borealis.Domain.Communication.Messages;


public sealed class FramesMessage : MessageBase
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
    public FrameDataCollection Frames { get; }


    /// <summary>
    /// The frame used to send to drivers.
    /// </summary>
    /// <param name="ledstripIndex"> The index of the ledstrip. </param>
    /// <param name="colorSpectrum"> </param>
    /// <param name="colors"> The colors that we want to send. </param>
    public FramesMessage(byte ledstripIndex, ColorSpectrum colorSpectrum, IEnumerable<FrameData> frames)
    {
        LedstripIndex = ledstripIndex;
        ColorSpectrum = colorSpectrum;
        Frames = new FrameDataCollection(frames);
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
    public static FramesMessage FromBuffer(ReadOnlyMemory<byte> buffer)
    {
        ReadOnlySpan<byte> data = buffer.Span;

        // Gets the first part of the frame which is the index and the color spectrum used.
        byte ledstripIndex = data[0];
        ColorSpectrum spectrum = (ColorSpectrum)data[1];
        int framesSize = BitConverter.ToInt32(data[2..6]);
        int framesCount = BitConverter.ToInt32(data[7..10]);

        ReadOnlySpan<byte> framesData = data.Slice(11);
        FrameDataCollection frames = new FrameDataCollection();

        for (int i = 0; i < framesCount; i++)
        {
            ReadOnlySpan<byte> frameData = data.Slice(i * framesSize, (i + 1) * framesSize);

            FrameData frame = FrameData.FromBuffer(spectrum, frameData.ToArray());

            frames.Add(frame);
        }

        return new FramesMessage(ledstripIndex, spectrum, frames);
    }


    private static ReadOnlyMemory<PixelColor> Deserialize3ByteColors(ReadOnlySpan<byte> data)
    {
        PixelColor[] result = new PixelColor[data.Length / 3];

        for (int i = 0; i < data.Length;)
        {
            result[i / 3] = new PixelColor(data[i++], data[i++], data[i++]);
        }

        return result;
    }


    private static ReadOnlyMemory<PixelColor> Deserialize4ByteColors(ReadOnlySpan<byte> data)
    {
        PixelColor[] result = new PixelColor[data.Length / 4];

        for (int i = 0; i < data.Length;)
        {
            result[i / 3] = new PixelColor(data[i++], data[i++], data[i++], data[i++]);
        }

        return result;
    }


    private static ReadOnlyMemory<PixelColor> Deserialize5ByteColors(ReadOnlySpan<byte> data)
    {
        PixelColor[] result = new PixelColor[data.Length / 5];

        for (int i = 0; i < data.Length;)
        {
            result[i / 3] = new PixelColor(data[i++], data[i++], data[i++], data[i++], data[i++]);
        }

        return result;
    }


    /// <inheritdoc />
    public override ReadOnlyMemory<Byte> Serialize()
    {
        // Need 10 bytes for the header.
        byte[] result = new byte[10 + Frames.Count * Frames.FrameSize];

        // Setting the ledstrip index.
        result[0] = Convert.ToByte(LedstripIndex);
        result[1] = Convert.ToByte((byte)ColorSpectrum);
        BitConverter.GetBytes(Frames.Count).CopyTo(result, 3);
        BitConverter.GetBytes(Frames.FrameSize).CopyTo(result, 7);

        for (int frameIndex = 0; frameIndex < Frames.Count; frameIndex++)
        {
            FrameData frame = Frames[frameIndex];
            frame.CopyTo(result, 10 + Frames.FrameSize * frameIndex);
        }

        return result;
    }
}