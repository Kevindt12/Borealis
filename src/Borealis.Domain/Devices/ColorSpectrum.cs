namespace Borealis.Domain.Devices;


/// <summary>
/// The type of pixel that is used.
/// </summary>
public enum ColorSpectrum
{
    /// <summary>
    /// The pixel type has 3 components RGB
    /// </summary>
    Rgb = 0,

    /// <summary>
    /// The pixel type has 4 components RGB White
    /// </summary>
    Rgbw = 1,

    /// <summary>
    /// The pixel type has 5 components RGB White White
    /// </summary>
    Rgbww = 2
}