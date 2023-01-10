using Borealis.Domain.Devices;
using Borealis.Domain.Effects;



namespace Borealis.Domain.Communication;


/// <summary>
/// The serializer used for serializing frames.
/// </summary>
public static class FrameSerializer
{
    /// <summary>
    /// Serializes a single frame into the memory buffer given.
    /// </summary>
    /// <param name="buffer"> The buffer we want to write to. </param>
    /// <param name="colorSpectrum"> The color spectrum that we are using on the device. </param>
    /// <param name="startIndex"> The start index on the buffer that we will start writing to. </param>
    /// <param name="frame"> The frame that we want to write. </param>
    /// <exception cref="ArgumentOutOfRangeException"> Thrown when the color spectrum is invalid. </exception>
    public static void SerializeFrame(Memory<byte> buffer, ColorSpectrum colorSpectrum, int startIndex, ReadOnlyMemory<PixelColor> frame)
    {
        int byteLength = CalculateByteLength(colorSpectrum, frame.Length);

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


    /// <summary>
    /// Deserializes a frame in the given buffer.
    /// </summary>
    /// <param name="buffer"> The buffer we want to read from. </param>
    /// <param name="colorSpectrum"> The color spectrum that we need to use to deserialize the frame. </param>
    /// <param name="startIndex"> The start index of the frame in the given buffer. </param>
    /// <param name="pixelCount"> The length of the frame. (The amount of pixels.) </param>
    /// <returns> </returns>
    /// <exception cref="ArgumentOutOfRangeException"> </exception>
    public static ReadOnlyMemory<PixelColor> DeserializeFrame(ReadOnlyMemory<byte> buffer, ColorSpectrum colorSpectrum, int startIndex, int pixelCount)
    {
        int byteLength = CalculateByteLength(colorSpectrum, pixelCount);

        ReadOnlyMemory<PixelColor> frameBuffer = colorSpectrum switch
        {
            ColorSpectrum.Rgb   => Deserialize3Byte(buffer.Slice(startIndex, byteLength).Span),
            ColorSpectrum.Rgbw  => Deserialize4Byte(buffer.Slice(startIndex, byteLength).Span),
            ColorSpectrum.Rgbww => Deserialize5Byte(buffer.Slice(startIndex, byteLength).Span),
            _                   => throw new ArgumentOutOfRangeException(nameof(ColorSpectrum))
        };

        return frameBuffer;
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


    /// <summary>
    /// This calculates the byte length of a frame by its number of pixels.
    /// </summary>
    /// <param name="spectrum"> The spectrum that we are using. </param>
    /// <param name="pixelCount"> The number of pixels in the frame. </param>
    /// <returns> The number of bytes the frame will be. </returns>
    /// <exception cref="ArgumentOutOfRangeException"> If the spectrum is not valid. </exception>
    public static int CalculateByteLength(ColorSpectrum spectrum, int pixelCount)
    {
        return spectrum switch
        {
            ColorSpectrum.Rgb   => pixelCount * 3,
            ColorSpectrum.Rgbw  => pixelCount * 4,
            ColorSpectrum.Rgbww => pixelCount * 5,
            _                   => throw new ArgumentOutOfRangeException(nameof(ColorSpectrum))
        };
    }
}