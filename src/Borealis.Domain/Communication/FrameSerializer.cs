using Borealis.Domain.Devices;
using Borealis.Domain.Effects;



namespace Borealis.Domain.Communication;


public static class FrameSerializer
{
    public static void SerializeFrame(Memory<byte> buffer, ColorSpectrum colorSpectrum, int startIndex, ReadOnlyMemory<PixelColor> frame)
    {
        int byteLength = colorSpectrum switch
        {
            ColorSpectrum.Rgb   => frame.Length * 3,
            ColorSpectrum.Rgbw  => frame.Length * 4,
            ColorSpectrum.Rgbww => frame.Length * 5,
            _                   => throw new ArgumentOutOfRangeException(nameof(ColorSpectrum))
        };

        // Setting all the colors.
        for (int i = startIndex,
                 ci = 0; i < startIndex + byteLength;)
        {
            // The default RGB
            buffer.Span[i++] = frame.Span[ci].R;
            buffer.Span[i++] = frame.Span[ci].G;
            buffer.Span[i++] = frame.Span[ci].B;

            // If W is added then we add it. same with WW
            if (colorSpectrum == ColorSpectrum.Rgbw) buffer.Span[i++] = frame.Span[ci].W;
            if (colorSpectrum == ColorSpectrum.Rgbww) buffer.Span[i++] = frame.Span[ci].WW;

            // Up the color index.
            ci++;
        }
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
            result[i / 4] = new PixelColor(data[i++], data[i++], data[i++], data[i++]);
        }

        return result;
    }


    private static ReadOnlyMemory<PixelColor> Deserialize5Byte(ReadOnlySpan<byte> data)
    {
        PixelColor[] result = new PixelColor[data.Length / 5];

        for (int i = 0; i < data.Length;)
        {
            result[i / 5] = new PixelColor(data[i++], data[i++], data[i++], data[i++], data[i++]);
        }

        return result;
    }


    public static ReadOnlyMemory<PixelColor> DeserializeFrame(ReadOnlyMemory<byte> buffer, ColorSpectrum colorSpectrum, int startIndex, int frameLength)
    {
        int byteLength = colorSpectrum switch
        {
            ColorSpectrum.Rgb   => frameLength * 3,
            ColorSpectrum.Rgbw  => frameLength * 4,
            ColorSpectrum.Rgbww => frameLength * 5,
            _                   => throw new ArgumentOutOfRangeException(nameof(ColorSpectrum))
        };

        ReadOnlyMemory<PixelColor> frameBuffer = colorSpectrum switch
        {
            ColorSpectrum.Rgb   => Deserialize3Byte(buffer.Slice(startIndex, byteLength).Span),
            ColorSpectrum.Rgbw  => Deserialize4Byte(buffer.Slice(startIndex, byteLength).Span),
            ColorSpectrum.Rgbww => Deserialize5Byte(buffer.Slice(startIndex, byteLength).Span),
            _                   => throw new ArgumentOutOfRangeException(nameof(ColorSpectrum))
        };

        return frameBuffer;
    }


    public static int CalculateByteLength(ColorSpectrum spectrum, int frameLength)
    {
        return spectrum switch
        {
            ColorSpectrum.Rgb   => frameLength * 3,
            ColorSpectrum.Rgbw  => frameLength * 4,
            ColorSpectrum.Rgbww => frameLength * 5,
            _                   => throw new ArgumentOutOfRangeException(nameof(ColorSpectrum))
        };
    }
}