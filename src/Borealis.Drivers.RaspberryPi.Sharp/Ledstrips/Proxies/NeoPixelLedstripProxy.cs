using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Linq;

using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;

using Iot.Device.Ws28xx;

using Microsoft.Extensions.Logging;



namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Proxies;


public abstract class NeoPixelStandardLedstripProxy : LedstripProxyBase, IDisposable
{
    private readonly ILogger _logger;

    private readonly SpiDevice _device;
    private readonly Ws28xx _driver;


    /// <summary>
    /// Creates a proxy handler for a ledstrip that is connected to this device.
    /// </summary>
    /// <param name="ledstrip"> The <see cref="Ledstrip" /> that is connect and want to control. </param>
    /// <exception cref="InvalidLedstripSettingsException"> Thrown when the ledstrip settings or the connection settings are not valid. </exception>
    /// <exception cref="LedstripConnectionException"> Thrown when the ledstrip connection is unable to be created. </exception>
    protected NeoPixelStandardLedstripProxy(ILogger logger, Ledstrip ledstrip, Bus bus, LedstripSettings settings) : base(ledstrip)
    {
        _logger = logger;

        // Setting up the device connection,
        _device = CreateSpiDevice(bus, settings);

        // Setting up the driver that we will use.
        _driver = CreateLedstripDevice(_device, ledstrip.PixelCount);
    }


    /// <summary>
    /// Creates a ledstrip spi device that we can use for the ledstrip.
    /// </summary>
    /// <param name="ledstrip"> The <see cref="Ledstrip" /> that is connect and want to control. </param>
    /// <returns> A <see cref="SpiDevice" /> to be used for the ledstrip connection. </returns>
    /// <exception cref="InvalidLedstripSettingsException"> Thrown when the ledstrip settings or the connection settings are not valid. </exception>
    /// <exception cref="LedstripConnectionException"> Thrown when the ledstrip connection is unable to be created. </exception>
    protected virtual SpiDevice CreateSpiDevice(Bus bus, LedstripSettings ledstripSettings)
    {
        try
        {
            return SpiDevice.Create(new SpiConnectionSettings(bus.SpiBusId, bus.SpiChipSelectId)
            {
                Mode = ledstripSettings.SpiMode,
                DataBitLength = ledstripSettings.DataBitLength,
                ClockFrequency = ledstripSettings.ClockFrequency,
                DataFlow = ledstripSettings.DataFlow,
                ChipSelectLineActiveState = ledstripSettings.ChipSelectLineActiveState switch
                {
                    PinState.Low  => PinValue.Low,
                    PinState.High => PinValue.High
                }
            });
        }
        catch (PlatformNotSupportedException platformNotSupportedException)
        {
            _logger.LogError(platformNotSupportedException, "WINDOWS DEBUGGING");

            throw new ApplicationException("Unable to create proxy.", platformNotSupportedException);
        }
        catch (IOException ioException)
        {
            _logger.LogError(ioException, "Unable to create proxy.");
            Dispose(false);

            throw new ApplicationException("Unable to create proxy.", ioException);
        }
    }


    protected abstract Ws28xx CreateLedstripDevice(SpiDevice spiDevice, int pixelCount);


    /// <summary>
    /// Sets the colors of the ledstrip to the colors given in the array.
    /// </summary>
    /// <param name="colors">
    /// The <see cref="ReadOnlyMemory{T}" /> of
    /// <see cref="PixelColor" /> the colors that we want to set.
    /// </param>
    public override void SetColors(ReadOnlyMemory<PixelColor> colors)
    {
        for (int i = 0; i < Ledstrip.PixelCount && i < colors.Length; i++)
        {
            _driver.Image.SetPixel(i, 0, colors.Span[i]);
        }

        _driver.Update();
    }


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