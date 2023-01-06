using System;
using System.Linq;

using Borealis.Domain.Devices;
using Borealis.Drivers.Rpi.Udp.Exceptions;
using Borealis.Drivers.Rpi.Udp.Ledstrips;



namespace Borealis.Drivers.Rpi.Udp.Contexts;


public class LedstripContext : IDisposable
{
    private readonly ILogger<LedstripContext> _logger;
    private readonly LedstripProxyFactory _ledstripProxyFactory;

    private readonly List<LedstripProxyBase> _ledstrips;


    /// <summary>
    /// Getting a Ledstrip proxy by the index in the configuration.
    /// </summary>
    /// <param name="index"> </param>
    /// <returns> </returns>
    public LedstripProxyBase this[int index] => _ledstrips[index];


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
    /// Reset the configuration.
    /// </summary>
    /// <param name="configuration"> </param>
    public void SetConfiguration(LedstripSettings configuration)
    {
        // If there are any ledstrips running then clean them up.
        if (_ledstrips.Any())
        {
            _logger.LogDebug("Cleaning up ledstrips.");
            CleanupLedstrips();
        }

        _logger.LogDebug("Loading ledstrips.");

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
            }
            catch (InvalidLedstripSettingsException invalidLedstripSettingsException)
            {
                // Handle invalid configuration,
                _logger.LogError(invalidLedstripSettingsException, "The ledstrip configuration was not valid.");
            }
            catch (NotImplementedException notImplementedException)
            {
                // Handle not implemented.
                _logger.LogError(notImplementedException, "The selected ledstrip with the current settings have not been implemented.");
            }
        }
    }


    public byte IndexOf(LedstripProxyBase ledstripProxy)
    {
        return Convert.ToByte(_ledstrips.IndexOf(ledstripProxy));
    }


    public virtual void ClearAllLedstrips()
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