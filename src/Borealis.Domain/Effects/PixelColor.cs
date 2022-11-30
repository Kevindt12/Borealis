using System.Drawing;



namespace Borealis.Domain.Effects;


public struct PixelColor : IEquatable<PixelColor>
{
    public byte R { get; init; }

    public byte G { get; init; }

    public byte B { get; init; }

    public byte W { get; init; }

    public byte WW { get; init; }


    public PixelColor(Byte r, Byte g, Byte b)
    {
        R = r;
        G = g;
        B = b;
    }


    public PixelColor(Byte r, Byte g, Byte b, Byte w)
    {
        R = r;
        G = g;
        B = b;
        W = w;
    }


    public PixelColor(Byte r,
                      Byte g,
                      Byte b,
                      Byte w,
                      Byte ww
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
        return Color.FromArgb(color.R, color.G, color.B);
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
    public override String ToString()
    {
        return $"R:{R}G:{G}B:{B}";
    }
}