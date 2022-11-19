using System.Device.Spi;
using System.Drawing;

using Borealis.Portal.Rpi.Configurations;

using Iot.Device.Ws28xx;



namespace Borealis.Portal.Rpi.Ledstrips;


public sealed class NeoPixelLedstripProxy : LedstripProxyBase, IDisposable
{
    private readonly SpiDevice _device;
    private readonly Ws28xx _driver;


    public NeoPixelLedstripProxy(LedstripSettings ledstripInfo)
    {
        LedstripSettings = ledstripInfo;

        #if !DEBUG
        _device = SpiDevice.Create(new SpiConnectionSettings(ledstripInfo.Connection.Spi.BusId, ledstripInfo.Connection.Spi.ChipSelectLine)
        {
            Mode = ledstripInfo.Connection.Spi.Mode,
            DataBitLength = ledstripInfo.Connection.Spi.DataBitLength,
            ClockFrequency = ledstripInfo.Connection.Spi.ClockFrequency,
            DataFlow = ledstripInfo.Connection.Spi.DataFlow,
            ChipSelectLineActiveState = ledstripInfo.Connection.Spi.ChipSelectLineActiveState
        });

        _driver = new Ws2815b(_device, ledstripInfo.Length);
        #endif
    }


    public override void SetColors(ReadOnlySpan<Color> colors)
    {
        for (int i = 0; i < LedstripSettings.Length; i++)
        {
            _driver.Image.SetPixel(i, 0, colors[i]);
        }

        _driver.Update();
    }


    public override void Clear()
    {
        SetColors(Enumerable.Repeat(Color.Black, LedstripSettings.Length).ToArray());
    }


    /// <inheritdoc />
    public override void Dispose()
    {
        _device.Dispose();
    }
}