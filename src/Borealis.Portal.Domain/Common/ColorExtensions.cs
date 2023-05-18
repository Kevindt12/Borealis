using System.Drawing;



namespace Borealis.Portal.Domain.Common;


public static class ColorExtensions
{
    public static PixelColor ToPixelColor(this Color color)
    {
        return new PixelColor
            { R = color.R, G = color.G, B = color.B };
    }
}