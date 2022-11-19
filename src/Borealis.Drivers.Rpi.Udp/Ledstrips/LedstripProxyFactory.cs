using Borealis.Domain.Devices;
using Borealis.Shared.Extensions;



namespace Borealis.Drivers.Rpi.Udp.Ledstrips;


/**
 * TODO: Create a inheritance for neo pixel ledstrip and use the converter that is open source.
 */
public class LedstripProxyFactory
{
    private readonly ILogger<LedstripProxyFactory> _logger;
    private readonly ILoggerFactory _loggerFactory;


    public LedstripProxyFactory(ILogger<LedstripProxyFactory> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
    }


    /// <summary>
    /// Creates a ledstrip proxy that can be used to communicate with the ledstrip.
    /// </summary>
    /// <param name="ledstripSettings"> </param>
    /// <returns> </returns>
    /// <exception cref="NotSupportedException"> </exception>
    public LedstripProxyBase CreateLedstripProxy(Ledstrip ledstripSettings)
    {
        return ledstripSettings.Protocol switch
        {
            LedstripProtocol.NeoPixel => CreateNeoPixelLedstripProxy(ledstripSettings),
            _                         => throw new NotImplementedException("The protocol was not supported.")
        };
    }


    /// <summary>
    /// Creates a variant of a NeoPixel Ledstrip Proxy.
    /// </summary>
    /// <param name="settings"> </param>
    /// <returns> </returns>
    /// <exception cref="NotSupportedException"> </exception>
    protected virtual LedstripProxyBase CreateNeoPixelLedstripProxy(Ledstrip settings)
    {
        _logger.LogDebug($"Creating neo pixel ledstrip with {settings.LogToJson()}");

        return settings.Colors switch
        {
            ColorSpectrum.Rgb => new NeoPixelStandardLedstripProxy(_loggerFactory.CreateLogger<NeoPixelStandardLedstripProxy>(), settings),
            _                 => throw new NotImplementedException("The selected color spectrum is not implemented")
        };
    }
}