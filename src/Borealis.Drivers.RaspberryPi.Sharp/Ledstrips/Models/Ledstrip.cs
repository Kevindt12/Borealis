namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;


public class Ledstrip
{
    /// <summary>
    /// The id of the ledstrip.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The connected bus of the ledstrip.
    /// </summary>
    public byte Bus { get; set; }

    /// <summary>
    /// The pixel count of the ledstrip.
    /// </summary>
    public UInt16 PixelCount { get; set; }

    /// <summary>
    /// The chip the ledstrip is using.
    /// </summary>
    public LedstripChip Chip { get; set; }
}