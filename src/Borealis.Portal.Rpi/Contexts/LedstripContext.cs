using Borealis.Portal.Rpi.Configurations;
using Borealis.Portal.Rpi.Ledstrips;



namespace Borealis.Portal.Rpi.Contexts;


public class LedstripContext : IDisposable
{
    private readonly List<LedstripProxyBase> _ledstrips;

    public LedstripProxyBase this[int index] => _ledstrips[index];


    public LedstripContext()
    {
        _ledstrips = new List<LedstripProxyBase>();
    }


    public void SetConfiguration(DeviceSettings configuration)
    {
        Dispose();

        foreach (LedstripSettings ledstrip in configuration.Ledstrips)
        {
            _ledstrips.Add(new NeoPixelLedstripProxy(ledstrip));
        }
    }


    /// <inheritdoc />
    public void Dispose()
    {
        foreach (LedstripProxyBase ledstrip in _ledstrips)
        {
            ledstrip.Dispose();
        }

        _ledstrips.Clear();
    }
}