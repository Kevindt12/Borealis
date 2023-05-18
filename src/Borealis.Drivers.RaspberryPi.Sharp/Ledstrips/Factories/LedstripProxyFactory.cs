using System;
using System.Drawing;
using System.Linq;

using Borealis.Drivers.RaspberryPi.Sharp.Common;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Providers;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Providers;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Proxies;

using Microsoft.Extensions.Logging;



namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Factories;


public class LedstripProxyFactory : ILedstripProxyFactory
{
    private readonly ILogger<LedstripProxyFactory> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILedstripSettingsProvider _ledstripSettingsProvider;
    private readonly IBusSettingsProvider _busSettingsProvider;


    /// <summary>
    /// Handles the creation of ledstrip proxies.
    /// </summary>
    public LedstripProxyFactory(ILogger<LedstripProxyFactory> logger, ILoggerFactory loggerFactory, ILedstripSettingsProvider ledstripSettingsProvider, IBusSettingsProvider busSettingsProvider)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _ledstripSettingsProvider = ledstripSettingsProvider;
        _busSettingsProvider = busSettingsProvider;
    }


    /// <summary>
    /// Creates a ledstrip proxy that can be used to communicate with the ledstrip.
    /// </summary>
    /// <param name="ledstrip"> The <see cref="Ledstrip" /> that we want to make an active proxy of. </param>
    /// <returns> Returns an <see cref="LedstripProxyBase" /> that is configured and ready to be used. </returns>
    /// <exception cref="NotImplementedException"> Thrown because we have not implemented any other ledstrips that still has to be done. </exception>
    public virtual LedstripProxyBase CreateLedstripProxy(Ledstrip ledstrip)
    {
        LedstripSettings settings = _ledstripSettingsProvider.GetLedstripSettings(ledstrip.Chip);
        Bus bus = _busSettingsProvider.GetBusSettingsById(ledstrip.Bus);

        LedstripProxyBase proxy = ledstrip.Chip switch
        {
            LedstripChip.WS2812B => new Ws2812LedstripProxy(_loggerFactory.CreateLogger<Ws2812LedstripProxy>(), ledstrip, bus, settings),
            LedstripChip.WS2813  => new Ws2813LedstripProxy(_loggerFactory.CreateLogger<Ws2813LedstripProxy>(), ledstrip, bus, settings),
            LedstripChip.WS2815  => new Ws2815LedstripProxy(_loggerFactory.CreateLogger<Ws2815LedstripProxy>(), ledstrip, bus, settings),
            _                    => throw new ArgumentOutOfRangeException("The selected chip is not supported.")
        };

        _logger.LogDebug("Ledstrip proxy made. Starting test animation.");
        PlayStartup(proxy);

        return proxy;
    }


    /// <summary>
    /// Plays the startup animation on a given <see cref="LedstripProxyBase" />.
    /// </summary>
    /// <param name="ledstripProxy"> The ledstrip proxy that we want to show the animation on. </param>
    protected virtual void PlayStartup(LedstripProxyBase ledstripProxy)
    {
        try
        {
            _logger.LogInformation($"Running test on ledstrip {ledstripProxy.Id}.");

            _logger.LogDebug($"Setting color to red. Colors Setting {(PixelColor)Color.Red}");
            ledstripProxy.SetColors(Enumerable.Repeat((PixelColor)Color.Red, ledstripProxy.Ledstrip.PixelCount).ToArray());
            Thread.Sleep(500);

            _logger.LogDebug($"Setting color to green. Colors Setting {(PixelColor)Color.Green}");
            ledstripProxy.SetColors(Enumerable.Repeat((PixelColor)Color.Green, ledstripProxy.Ledstrip.PixelCount).ToArray());
            Thread.Sleep(500);

            _logger.LogDebug($"Setting color to blue. Colors Setting {(PixelColor)Color.Blue}");
            ledstripProxy.SetColors(Enumerable.Repeat((PixelColor)Color.Blue, ledstripProxy.Ledstrip.PixelCount).ToArray());
            Thread.Sleep(500);

            _logger.LogDebug("Clearing ledstrip...");
            ledstripProxy.Clear();
        }
        catch (IOException ioException)
        {
            // Cleaning up and rethrowing it as a connection exception.
            _logger.LogError(ioException, "There was a problem with the startup test.");
            ledstripProxy.Dispose();

            throw new ApplicationException();

            //throw new ApplicationException("There was a problem with the startup test.", ioException, ledstripProxy.Ledstrip);
        }
    }
}