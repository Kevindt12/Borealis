using System;
using System.Device.Spi;
using System.Drawing;
using System.Linq;

using Borealis.Domain.Devices;
using Borealis.Domain.Effects;
using Borealis.Drivers.Rpi.Udp.Exceptions;

using Iot.Device.Ws28xx;



namespace Borealis.Drivers.Rpi.Udp.Ledstrips;


public sealed class NeoPixelStandardLedstripProxy : LedstripProxyBase, IDisposable
{
    private readonly ILogger<NeoPixelStandardLedstripProxy> _logger;

    private readonly SpiDevice _device;
    private readonly Ws28xx _driver;


    public Guid Id { get; set; } = Guid.NewGuid();


    public NeoPixelStandardLedstripProxy(ILogger<NeoPixelStandardLedstripProxy> logger, Ledstrip ledstrip) : base(ledstrip)
    {
        _logger = logger;

        // Setting up the device connection,
        _device = CreateSpiDevice(ledstrip);

        // Setting up the driver that we will use.
        _driver = new Ws2812b(_device, ledstrip.Length);

        PlayStartup();
    }


    private SpiDevice CreateSpiDevice(Ledstrip ledstrip)
    {
        // Guards.
        if (ledstrip.Connection!.Spi == null) throw new InvalidLedstripSettingsException("There is no connection  set.");

        try
        {
            return SpiDevice.Create(new SpiConnectionSettings(ledstrip.Connection!.Spi!.BusId, ledstrip.Connection.Spi.ChipSelectLine)
            {
                Mode = ledstrip.Connection.Spi.Mode,
                DataBitLength = ledstrip.Connection.Spi.DataBitLength,
                ClockFrequency = ledstrip.Connection.Spi.ClockFrequency,
                DataFlow = ledstrip.Connection.Spi.DataFlow,
                ChipSelectLineActiveState = ledstrip.Connection.Spi.ChipSelectLineActiveState
            });
        }
        catch (PlatformNotSupportedException)
        {
            _logger.LogError("WINDOWS DEBUGGING");

            throw new LedstripConnectionException("Unable to create proxy.");
        }
        catch (IOException ioException)
        {
            _logger.LogError(ioException, "Unable to create proxy.");
            Dispose(false);

            throw new LedstripConnectionException("Unable to create proxy.", ioException);
        }
    }


    public override void SetColors(ReadOnlyMemory<PixelColor> colors)
    {
        for (int i = 0; i < Ledstrip.Length; i++)
        {
            _driver.Image.SetPixel(i, 0, colors.Span[i]);
        }

        _driver.Update();
    }


    private void PlayStartup()
    {
        try
        {
            _logger.LogInformation($"Running test on ledstrip {Id}.");

            _logger.LogDebug($"Setting color to red. Colors Setting {(PixelColor)Color.Red}");
            SetColors(Enumerable.Repeat((PixelColor)Color.Red, Ledstrip.Length).ToArray());
            Thread.Sleep(1500);

            _logger.LogDebug($"Setting color to green. Colors Setting {(PixelColor)Color.Green}");
            SetColors(Enumerable.Repeat((PixelColor)Color.Green, Ledstrip.Length).ToArray());
            Thread.Sleep(1500);

            _logger.LogDebug($"Setting color to blue. Colors Setting {(PixelColor)Color.Blue}");
            SetColors(Enumerable.Repeat((PixelColor)Color.Blue, Ledstrip.Length).ToArray());
            Thread.Sleep(1500);

            _logger.LogDebug("Clearing ledstrip...");
            Clear();
        }
        catch (IOException ioException)
        {
            // Cleaning up and rethrowing it as a connection exception.
            _logger.LogError(ioException, "There was a problem with the startup test.");
            Dispose(true);

            throw new LedstripConnectionException("There was a problem with the startup test.", ioException, Ledstrip);
        }
    }


    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _device.Dispose();
        }
    }
}