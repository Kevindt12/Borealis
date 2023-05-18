using System.Drawing;



namespace Borealis.Drivers.RaspberryPi.Sharp.Common;


public readonly struct PixelColor : IEquatable<PixelColor>
{
    public byte R { get; init; }

    public byte G { get; init; }

    public byte B { get; init; }

    public byte W { get; init; }

    public byte WW { get; init; }


    public PixelColor(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }


    public PixelColor(byte r, byte g, byte b, byte w)
    {
        R = r;
        G = g;
        B = b;
        W = w;
    }


    public PixelColor(byte r,
                      byte g,
                      byte b,
                      byte w,
                      byte ww
    )
    {
        R = r;
        G = g;
        B = b;
        W = w;
        WW = ww;
    }


    public static implicit operator Color(PixelColor color)
    {
        return Color.FromArgb(255, color.R, color.G, color.B);
    }


    public static implicit operator PixelColor(Color color)
    {
        return new PixelColor(color.R, color.G, color.B);
    }


    /// <inheritdoc />
    public bool Equals(PixelColor other)
    {
        return R == other.R && G == other.G && B == other.B && W == other.W && WW == other.WW;
    }


    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is PixelColor other && Equals(other);
    }


    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B, W, WW);
    }


    public static bool operator ==(PixelColor left, PixelColor right)
    {
        return left.Equals(right);
    }


    public static bool operator !=(PixelColor left, PixelColor right)
    {
        return !left.Equals(right);
    }


    /// <inheritdoc />
    public override string ToString()
    {
        return $"R:{R}G:{G}B:{B}";
    }
}