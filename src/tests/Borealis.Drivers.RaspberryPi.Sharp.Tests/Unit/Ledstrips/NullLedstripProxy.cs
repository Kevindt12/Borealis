using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Proxies;



namespace Borealis.Drivers.RaspberryPi.Sharp.Tests.Unit.Ledstrips;


public class NullLedstripProxy : LedstripProxyBase
{
    public static NullLedstripProxy Instance = new NullLedstripProxy();


    public NullLedstripProxy() : base(null) { }


    /// <inheritdoc />
    public override void SetColors(ReadOnlyMemory<PixelColor> colors) { }
}