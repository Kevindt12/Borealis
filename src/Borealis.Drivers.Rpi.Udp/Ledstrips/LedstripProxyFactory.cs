using System;
using System.Drawing;

using Borealis.Shared.Extensions;

using System.Linq;

using Borealis.Domain.Devices;
using Borealis.Domain.Ledstrips;
using Borealis.Drivers.Rpi.Exceptions;



namespace Borealis.Drivers.Rpi.Ledstrips;


/**
 * TODO: Create a inheritance for neo pixel ledstrip and use the converter that is open source.
 */
public class LedstripProxyFactory
{
    private readonly ILogger<LedstripProxyFactory> _logger;
    private readonly ILoggerFactory _loggerFactory;


    /// <summary>
    /// Handles the creation of ledstrip proxies.
    /// </summary>
    public LedstripProxyFactory(ILogger<LedstripProxyFactory> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
    }


    /// <summary>
    /// Creates a ledstrip proxy that can be used to communicate with the ledstrip.
    /// </summary>
    /// <param name="ledstripSettings"> The <see cref="Ledstrip" /> that we want to make an active proxy of. </param>
    /// <returns> Returns an <see cref="LedstripProxyBase" /> that is configured and ready to be used. </returns>
    /// <exception cref="LedstripConnectionException"> Thrown when there is a problem with the connection. </exception>
    /// <exception cref="NotImplementedException"> Thrown because we have not implemented any other ledstrips that still has to be done. </exception>
    public LedstripProxyBase CreateLedstripProxy(Ledstrip ledstripSettings)
    {
        LedstripProxyBase ledstrip = ledstripSettings.Protocol switch
        {
            LedstripProtocol.NeoPixel => CreateNeoPixelLedstripProxy(ledstripSettings),
            _                         => throw new NotImplementedException("The protocol was not supported.")
        };

        _logger.LogDebug("Ledstrip proxy made. Starting test animation.");
        PlayStartup(ledstrip);

        return ledstrip;
    }


    /// <summary>
    /// Creates a proxy of a <see cref="Ledstrip" /> that we can interact with.
    /// </summary>
    /// <param name="settings"> The ledstrip that we want to create an active proxt of. </param>
    /// <returns> Creating a <see cref="LedstripProxyBase" /> ledstrip that we can interact with. </returns>
    /// <exception cref="LedstripConnectionException"> Thrown when there is a problem with the connection. </exception>
    /// <exception cref="NotImplementedException"> Thrown because we have not implemented any other ledstrips that still has to be done. </exception>
    protected virtual LedstripProxyBase CreateNeoPixelLedstripProxy(Ledstrip settings)
    {
        _logger.LogDebug($"Creating neo pixel ledstrip with {settings.LogToJson()}");

        LedstripProxyBase ledstrip = settings.Colors switch
        {
            ColorSpectrum.Rgb => new NeoPixelStandardLedstripProxy(_loggerFactory.CreateLogger<NeoPixelStandardLedstripProxy>(), settings),
            _                 => throw new NotImplementedException("The selected color spectrum is not implemented")
        };

        return ledstrip;
    }


    /// <summary>
    /// Plays the startup animation on a given <see cref="LedstripProxyBase" />.
    /// </summary>
    /// <param name="ledstripProxy"> The ledstrip proxy that we want to show the animation on. </param>
    /// <exception cref="LedstripConnectionException"> Thrown when there is a problem with the connection. </exception>
    private void PlayStartup(LedstripProxyBase ledstripProxy)
    {
        try
        {
            _logger.LogInformation($"Running test on ledstrip {ledstripProxy.Id}.");

            _logger.LogDebug($"Setting color to red. Colors Setting {(PixelColor)Color.Red}");
            ledstripProxy.SetColors(Enumerable.Repeat((PixelColor)Color.Red, ledstripProxy.Ledstrip.Length).ToArray());
            Thread.Sleep(500);

            _logger.LogDebug($"Setting color to green. Colors Setting {(PixelColor)Color.Green}");
            ledstripProxy.SetColors(Enumerable.Repeat((PixelColor)Color.Green, ledstripProxy.Ledstrip.Length).ToArray());
            Thread.Sleep(500);

            _logger.LogDebug($"Setting color to blue. Colors Setting {(PixelColor)Color.Blue}");
            ledstripProxy.SetColors(Enumerable.Repeat((PixelColor)Color.Blue, ledstripProxy.Ledstrip.Length).ToArray());
            Thread.Sleep(500);

            _logger.LogDebug("Clearing ledstrip...");
            ledstripProxy.Clear();
        }
        catch (IOException ioException)
        {
            // Cleaning up and rethrowing it as a connection exception.
            _logger.LogError(ioException, "There was a problem with the startup test.");
            ledstripProxy.Dispose();

            throw new LedstripConnectionException("There was a problem with the startup test.", ioException, ledstripProxy.Ledstrip);
        }
    }
}