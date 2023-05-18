using System;
using System.Device.Spi;
using System.Linq;

using Borealis.Domain.Ledstrips;
using Borealis.Drivers.Rpi.Exceptions;

using Iot.Device.Ws28xx;



namespace Borealis.Drivers.Rpi.Ledstrips;


public sealed class NeoPixelStandardLedstripProxy : LedstripProxyBase, IDisposable
{
    private readonly ILogger<NeoPixelStandardLedstripProxy> _logger;

    private readonly SpiDevice _device;
    private readonly Ws28xx _driver;


    /// <summary>
    /// Creates a proxy handler for a ledstrip that is connected to this device.
    /// </summary>
    /// <param name="ledstrip"> The <see cref="Ledstrip" /> that is connect and want to control. </param>
    /// <exception cref="InvalidLedstripSettingsException"> Thrown when the ledstrip settings or the connection settings are not valid. </exception>
    /// <exception cref="LedstripConnectionException"> Thrown when the ledstrip connection is unable to be created. </exception>
    public NeoPixelStandardLedstripProxy(ILogger<NeoPixelStandardLedstripProxy> logger, Ledstrip ledstrip) : base(ledstrip)
    {
        _logger = logger;

        // Setting up the device connection,
        _device = CreateSpiDevice(ledstrip);

        // Setting up the driver that we will use.
        _driver = new Ws2815b(_device, ledstrip.Length);

        PlayStartup();
    }


    /// <summary>
    /// Creates a ledstrip spi device that we can use for the ledstrip.
    /// </summary>
    /// <param name="ledstrip"> The <see cref="Ledstrip" /> that is connect and want to control. </param>
    /// <returns> A <see cref="SpiDevice" /> to be used for the ledstrip connection. </returns>
    /// <exception cref="InvalidLedstripSettingsException"> Thrown when the ledstrip settings or the connection settings are not valid. </exception>
    /// <exception cref="LedstripConnectionException"> Thrown when the ledstrip connection is unable to be created. </exception>
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
        catch (PlatformNotSupportedException platformNotSupportedException)
        {
            // HACK: This is done so we can start the application also on windows.
            _logger.LogError(platformNotSupportedException, "WINDOWS DEBUGGING");

            throw new LedstripConnectionException("Unable to create proxy.", platformNotSupportedException);
        }
        catch (IOException ioException)
        {
            _logger.LogError(ioException, "Unable to create proxy.");
            Dispose(false);

            throw new LedstripConnectionException("Unable to create proxy.", ioException);
        }
    }


    /// <summary>
    /// Sets the colors of the ledstrip to the colors given in the array.
    /// </summary>
    /// <param name="colors">
    /// The <see cref="ReadOnlyMemory{T}" /> of
    /// <see cref="PixelColor" /> the colors that we want to set.
    /// </param>
    public override void SetColors(ReadOnlyMemory<PixelColor> colors)
    {
        for (int i = 0; i < Ledstrip.Length && i < colors.Length; i++)
        {
            _driver.Image.SetPixel(i, 0, colors.Span[i]);
        }

        _driver.Update();
    }


    /// <summary>
    /// Plays the startup sequence used to verifiy that the ledstrip is working.
    /// </summary>
    /// <exception cref="LedstripConnectionException"> Thrown when there was a problem playing the test of the ledstrip. </exception>
    private void PlayStartup() { }


    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clearing the frame on the ledstrip.
            Clear();

            // Dispose of the device.
            _device.Dispose();
        }
    }
}