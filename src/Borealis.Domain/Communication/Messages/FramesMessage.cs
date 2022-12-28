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


    public int FrameCount => Frames.Count;

    public int FrameSize { get; init; }

    /// <summary>
    /// The colors of the ledstrip.
    /// </summary>
    public IReadOnlyList<ReadOnlyMemory<PixelColor>> Frames { get; }


    /// <summary>
    /// The frame used to send to drivers.
    /// </summary>
    /// <param name="ledstripIndex"> The index of the ledstrip. </param>
    /// <param name="colorSpectrum"> </param>
    /// <param name="colors"> The colors that we want to send. </param>
    public FramesMessage(byte ledstripIndex, ColorSpectrum colorSpectrum, int frameSize, IEnumerable<ReadOnlyMemory<PixelColor>> frames)
    {
        LedstripIndex = ledstripIndex;
        ColorSpectrum = colorSpectrum;
        Frames = new List<ReadOnlyMemory<PixelColor>>(frames);

        if (Frames.Any())
        {
            FrameSize = Frames[0].Length;
        }
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
        ReadOnlyMemory<PixelColor>[] colors = new ReadOnlyMemory<PixelColor>[framesCount];

        for (int i = 0; i < framesCount; i++)
        {
            int startIndex = i * framesSize * GetBytesPerLed(spectrum);
            int endIndex = (i + 1) * framesSize * GetBytesPerLed(spectrum) - 1;

            colors[i] = spectrum switch
            {
                ColorSpectrum.Rgb   => Deserialize3ByteColors(framesData[startIndex..endIndex]),
                ColorSpectrum.Rgbw  => Deserialize4ByteColors(framesData[startIndex..endIndex]),
                ColorSpectrum.Rgbww => Deserialize5ByteColors(framesData[startIndex..endIndex]),
                _                   => throw new ArgumentOutOfRangeException(nameof(spectrum), "The spectrum is not set so cant serialize data.")
            };
        }

        return new FramesMessage(ledstripIndex, spectrum, framesSize, colors);
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
        byte[] result = new byte[10 + FrameCount * FrameSize * GetBytesPerLed(ColorSpectrum)];

        // Setting the ledstrip index.
        result[0] = Convert.ToByte(LedstripIndex);
        result[1] = Convert.ToByte((byte)ColorSpectrum);
        BitConverter.GetBytes(FrameSize).CopyTo(result, 3);
        BitConverter.GetBytes(FrameCount).CopyTo(result, 7);

        for (int frameIndex = 0; frameIndex < Frames.Count; frameIndex++)
        {
            ReadOnlyMemory<PixelColor> frame = Frames[frameIndex];

            // Setting all the colors.
            for (int i = 11 + frameIndex * FrameSize * GetBytesPerLed(ColorSpectrum),
                     ci = 0; i < result.Length;)
            {
                // The default RGB
                result[i++] = frame.Span[ci].R;
                result[i++] = frame.Span[ci].G;
                result[i++] = frame.Span[ci].B;

                // If W is added then we add it. same with WW
                if (ColorSpectrum == ColorSpectrum.Rgbw) result[i++] = frame.Span[ci].W;
                if (ColorSpectrum == ColorSpectrum.Rgbww) result[i++] = frame.Span[ci].WW;

                // Up the color index.
                ci++;
            }
        }

        return result;
    }


    private static int GetBytesPerLed(ColorSpectrum colorSpectrum) =>
        colorSpectrum switch

        {
            ColorSpectrum.Rgb   => 3,
            ColorSpectrum.Rgbw  => 4,
            ColorSpectrum.Rgbww => 5,
            _                   => throw new ArgumentOutOfRangeException(nameof(ColorSpectrum), "Color spectrum unknown.")
        };
}