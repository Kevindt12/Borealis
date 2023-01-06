using Borealis.Domain.Devices;
using Borealis.Domain.Effects;



namespace Borealis.Domain.Communication.Messages;


public class LedstripFrame
{
    public ColorSpectrum ColorSpectrum { get; init; }


    public ReadOnlyMemory<PixelColor> Payload { get; init; }


    public int ByteLength =>
        ColorSpectrum switch
        {
            ColorSpectrum.Rgb   => Payload.Length * 3,
            ColorSpectrum.Rgbw  => Payload.Length * 4,
            ColorSpectrum.Rgbww => Payload.Length * 5,
            _                   => throw new ArgumentOutOfRangeException(nameof(ColorSpectrum))
        };


    public LedstripFrame(ReadOnlyMemory<PixelColor> payload, ColorSpectrum spectrum)
    {
        Payload = payload;
        ColorSpectrum = spectrum;
    }


    private LedstripFrame(ReadOnlyMemory<byte> buffer, ColorSpectrum spectrum)
    {
        ColorSpectrum = spectrum;

        Payload = spectrum switch
        {
            ColorSpectrum.Rgb   => Deserialize3Byte(buffer.Span),
            ColorSpectrum.Rgbw  => Deserialize4Byte(buffer.Span),
            ColorSpectrum.Rgbww => Deserialize5Byte(buffer.Span),
            _                   => throw new ArgumentOutOfRangeException(nameof(ColorSpectrum), "The spectrum is not set so cant serialize data.")
        };
    }


    public static LedstripFrame FromBuffer(ColorSpectrum colorSpectrum, ReadOnlyMemory<byte> buffer)
    {
        return new LedstripFrame(buffer, colorSpectrum);
    }


    private static ReadOnlyMemory<PixelColor> Deserialize3Byte(ReadOnlySpan<byte> data)
    {
        PixelColor[] result = new PixelColor[data.Length / 3];

        for (int i = 0; i < data.Length;)
        {
            result[i / 3] = new PixelColor(data[i++], data[i++], data[i++]);
        }

        return result;
    }


    private static ReadOnlyMemory<PixelColor> Deserialize4Byte(ReadOnlySpan<byte> data)
    {
        PixelColor[] result = new PixelColor[data.Length / 4];

        for (int i = 0; i < data.Length;)
        {
            result[i / 3] = new PixelColor(data[i++], data[i++], data[i++], data[i++]);
        }

        return result;
    }


    private static ReadOnlyMemory<PixelColor> Deserialize5Byte(ReadOnlySpan<byte> data)
    {
        PixelColor[] result = new PixelColor[data.Length / 5];

        for (int i = 0; i < data.Length;)
        {
            result[i / 3] = new PixelColor(data[i++], data[i++], data[i++], data[i++], data[i++]);
        }

        return result;
    }


    public void CopyTo(Memory<byte> buffer, int startIndex)
    {
        // Setting all the colors.
        for (int i = startIndex,
                 ci = 0; i < startIndex + ByteLength;)
        {
            // The default RGB
            buffer.Span[i++] = Payload.Span[ci].R;
            buffer.Span[i++] = Payload.Span[ci].G;
            buffer.Span[i++] = Payload.Span[ci].B;

            // If W is added then we add it. same with WW
            if (ColorSpectrum == ColorSpectrum.Rgbw) buffer.Span[i++] = Payload.Span[ci].W;
            if (ColorSpectrum == ColorSpectrum.Rgbww) buffer.Span[i++] = Payload.Span[ci].WW;

            // Up the color index.
            ci++;
        }
    }
}