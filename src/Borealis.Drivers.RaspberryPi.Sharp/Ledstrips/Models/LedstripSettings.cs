using System.Device.Spi;



namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;


public class LedstripSettings
{
    public SpiMode SpiMode { get; set; }

    public int DataBitLength { get; set; }

    public int ClockFrequency { get; set; }

    public DataFlow DataFlow { get; set; }

    public PinState ChipSelectLineActiveState { get; set; }
}



public enum PinState
{
    High,
    Low
}