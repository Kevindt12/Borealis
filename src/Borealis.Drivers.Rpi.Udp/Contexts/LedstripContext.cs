using System;
using System.Linq;

using Borealis.Domain.Devices;
using Borealis.Drivers.Rpi.Udp.Exceptions;
using Borealis.Drivers.Rpi.Udp.Ledstrips;



namespace Borealis.Drivers.Rpi.Udp.Contexts;


public sealed class LedstripContext : IDisposable
{
    private readonly ILogger<LedstripContext> _logger;
    private readonly LedstripProxyFactory _ledstripProxyFactory;

    private readonly List<LedstripProxyBase> _ledstrips;


    /// <summary>
    /// Getting a Ledstrip proxy by the index in the configuration.
    /// </summary>
    /// <param name="index"> The of the ledstrip proxy we want to get. </param>
    /// <returns> </returns>
    public LedstripProxyBase this[byte index] => _ledstrips[index];


    public int Count => _ledstrips.Count;


    /// <summary>
    /// The ledstrip context that holds all the ledstrip proxies.
    /// </summary>
    public LedstripContext(ILogger<LedstripContext> logger, LedstripProxyFactory ledstripProxyFactory)
    {
        _logger = logger;
        _ledstripProxyFactory = ledstripProxyFactory;
        _ledstrips = new List<LedstripProxyBase>();
    }


    /// <summary>
    /// Sets the ledstrip configuration that was given.
    /// </summary>
    /// <param name="configuration"> The configuration that was given. </param>
    public void SetConfiguration(LedstripSettings configuration)
    {
        // If there are any ledstrips running then clean them up.
        if (_ledstrips.Any())
        {
            _logger.LogDebug("Cleaning up ledstrips.");
            CleanupLedstrips();
        }

        _logger.LogDebug("Loading ledstrips.");

        List<Exception> exceptions = new List<Exception>();

        foreach (Ledstrip ledstrip in configuration.Ledstrips)
        {
            try
            {
                _ledstrips.Add(_ledstripProxyFactory.CreateLedstripProxy(ledstrip));
                _logger.LogDebug($"Ledstrip added {ledstrip.Name ?? string.Empty}");
            }
            catch (LedstripConnectionException ledstripConnectionException)
            {
                // Handle unable to connect
                _logger.LogError(ledstripConnectionException, "Unable to create ledstrip proxy.");

                exceptions.Add(ledstripConnectionException);
            }
            catch (InvalidLedstripSettingsException invalidLedstripSettingsException)
            {
                // Handle invalid configuration,
                _logger.LogError(invalidLedstripSettingsException, "The ledstrip configuration was not valid.");

                exceptions.Add(invalidLedstripSettingsException);
            }
            catch (NotImplementedException notImplementedException)
            {
                // Handle not implemented.
                _logger.LogError(notImplementedException, "The selected ledstrip with the current settings have not been implemented.");
            }
        }

        if (exceptions.Any())
        {
            throw new AggregateException(exceptions);
        }
    }


    /// <summary>
    /// Gets the index of the ledstrip by the given <see cref="LedstripProxyBase" />.
    /// </summary>
    /// <param name="ledstripProxy"> The <see cref="LedstripProxyBase" /> that we want to index of. </param>
    /// <returns> The <see cref="byte" /> index of the ledstrip. </returns>
    public byte IndexOf(LedstripProxyBase ledstripProxy)
    {
        return Convert.ToByte(_ledstrips.IndexOf(ledstripProxy));
    }


    /// <summary>
    /// Clears all the ledstrips that we are managing.
    /// </summary>
    public void ClearAllLedstrips()
    {
        foreach (LedstripProxyBase proxy in _ledstrips)
        {
            proxy.Clear();
        }
    }


    /// <summary>
    /// Cleaning up the ledstrips.
    /// </summary>
    private void CleanupLedstrips()
    {
        _logger.LogDebug("Cleaning up the ledstrip context.");

        foreach (LedstripProxyBase ledstrip in _ledstrips)
        {
            ledstrip.Dispose();
        }

        _ledstrips.Clear();
        _logger.LogDebug("Ledstrip context cleared.");
    }


    /// <inheritdoc />
    public void Dispose()
    {
        CleanupLedstrips();
    }
}