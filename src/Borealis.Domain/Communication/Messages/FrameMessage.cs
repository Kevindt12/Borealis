using Borealis.Domain.Devices;
using Borealis.Domain.Effects;



namespace Borealis.Domain.Communication.Messages;


public sealed class FrameMessage : MessageBase
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
    public ReadOnlyMemory<PixelColor> Colors { get; }


    /// <summary>
    /// The frame used to send to drivers.
    /// </summary>
    public FrameMessage(ReadOnlyMemory<byte> data)
    {
        // Gets the first part of the frame which is the index and the color spectrum used.
        LedstripIndex = data.Span[0];
        ColorSpectrum spectrum = (ColorSpectrum)data.Span[1];

        Colors = spectrum switch
        {
            ColorSpectrum.Rgb   => Deserialize3ByteColors(data[2..]),
            ColorSpectrum.Rgbw  => Deserialize4ByteColors(data[2..]),
            ColorSpectrum.Rgbww => Deserialize5ByteColors(data[2..]),
            _                   => throw new ArgumentOutOfRangeException(nameof(spectrum), "The spectrum is not set so cant serialize data.")
        };
    }


    /// <summary>
    /// The frame used to send to drivers.
    /// </summary>
    /// <param name="ledstripIndex"> The index of the ledstrip. </param>
    /// <param name="colorSpectrum"> </param>
    /// <param name="colors"> The colors that we want to send. </param>
    public FrameMessage(byte ledstripIndex, ColorSpectrum colorSpectrum, ReadOnlyMemory<PixelColor> colors)
    {
        LedstripIndex = ledstripIndex;
        ColorSpectrum = colorSpectrum;
        Colors = colors.ToArray();
    }


    private static ReadOnlyMemory<PixelColor> Deserialize3ByteColors(ReadOnlyMemory<byte> data)
    {
        PixelColor[] result = new PixelColor[data.Length / 3];

        for (int i = 0; i < data.Length;)
        {
            result[i / 3] = new PixelColor(data.Span[i++], data.Span[i++], data.Span[i++]);
        }

        return result;
    }


    private static ReadOnlyMemory<PixelColor> Deserialize4ByteColors(ReadOnlyMemory<byte> data)
    {
        PixelColor[] result = new PixelColor[data.Length / 4];

        for (int i = 0; i < data.Length;)
        {
            result[i / 3] = new PixelColor(data.Span[i++], data.Span[i++], data.Span[i++], data.Span[i++]);
        }

        return result;
    }


    private static ReadOnlyMemory<PixelColor> Deserialize5ByteColors(ReadOnlyMemory<byte> data)
    {
        PixelColor[] result = new PixelColor[data.Length / 5];

        for (int i = 0; i < data.Length;)
        {
            result[i / 3] = new PixelColor(data.Span[i++], data.Span[i++], data.Span[i++], data.Span[i++], data.Span[i++]);
        }

        return result;
    }


    /// <inheritdoc />
    public override ReadOnlyMemory<Byte> Serialize()
    {
        // Creating the result based on the spectrum to get the right mount of bytes.
        // Note that we add 1 for the ledstrip index.
        byte[] result = new byte[ColorSpectrum switch
        {
            ColorSpectrum.Rgb   => 3 * Colors.Length + 2,
            ColorSpectrum.Rgbw  => 4 * Colors.Length + 2,
            ColorSpectrum.Rgbww => 5 * Colors.Length + 2,
            _                   => throw new ArgumentOutOfRangeException(nameof(ColorSpectrum), "Color spectrum unknown.")
        }];

        // Setting the ledstrip index.
        result[0] = Convert.ToByte(LedstripIndex);
        result[1] = Convert.ToByte((byte)ColorSpectrum);

        // Setting all the colors.
        for (int i = 2,
                 ci = 0; i < result.Length;)
        {
            // The default RGB
            result[i++] = Colors.Span[ci].R;
            result[i++] = Colors.Span[ci].G;
            result[i++] = Colors.Span[ci].B;

            // If W is added then we add it. same with WW
            if (ColorSpectrum == ColorSpectrum.Rgbw) result[i++] = Colors.Span[ci].W;
            if (ColorSpectrum == ColorSpectrum.Rgbww) result[i++] = Colors.Span[ci].WW;

            // Up the color index.
            ci++;
        }

        return result;
    }
}