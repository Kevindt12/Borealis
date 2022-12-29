using Borealis.Domain.Devices;
using Borealis.Domain.Effects;



namespace Borealis.Domain.Communication.Messages;


public class FrameData
{
    public ColorSpectrum ColorSpectrum { get; init; }


    public ReadOnlyMemory<PixelColor> Frames { get; init; }


    public int ByteLength =>
        ColorSpectrum switch
        {
            ColorSpectrum.Rgb   => Frames.Length * 3,
            ColorSpectrum.Rgbw  => Frames.Length * 4,
            ColorSpectrum.Rgbww => Frames.Length * 5,
            _                   => throw new ArgumentOutOfRangeException(nameof(ColorSpectrum))
        };


    public FrameData(ReadOnlyMemory<PixelColor> frames, ColorSpectrum spectrum)
    {
        Frames = frames;
        ColorSpectrum = spectrum;
    }


    private FrameData(ReadOnlyMemory<byte> buffer, ColorSpectrum spectrum)
    {
        ColorSpectrum = spectrum;

        Frames = spectrum switch
        {
            ColorSpectrum.Rgb   => Deserialize3Byte(buffer.Span),
            ColorSpectrum.Rgbw  => Deserialize4Byte(buffer.Span),
            ColorSpectrum.Rgbww => Deserialize5Byte(buffer.Span),
            _                   => throw new ArgumentOutOfRangeException(nameof(ColorSpectrum), "The spectrum is not set so cant serialize data.")
        };
    }


    public static FrameData FromBuffer(ColorSpectrum colorSpectrum, ReadOnlyMemory<byte> buffer)
    {
        return new FrameData(buffer, colorSpectrum);
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
}