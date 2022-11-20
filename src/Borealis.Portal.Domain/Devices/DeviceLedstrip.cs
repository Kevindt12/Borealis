using Borealis.Domain.Devices;



namespace Borealis.Portal.Domain.Devices;


public class DeviceLedstrip : Ledstrip
{
    /// <summary>
    /// The device that the ledstrip is connected to.
    /// </summary>
    public Device Device { get; set; } = default!;
}