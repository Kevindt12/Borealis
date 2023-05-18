using Borealis.Domain.Ledstrips;
using Borealis.Portal.Core.Ledstrips.State;
using Borealis.Portal.Domain.Connectivity.Connections;



namespace Borealis.Portal.Core.Ledstrips.Contexts;


internal class LedstripDisplayContext
{
    private readonly List<LedstripDisplayState> _ledstripDisplayStates;


    /// <summary>
    /// Holds the context of the ledstrips.
    /// </summary>
    public LedstripDisplayContext()
    {
        _ledstripDisplayStates = new List<LedstripDisplayState>();
    }


    /// <summary>
    /// Get the state of a ledstrip.
    /// </summary>
    /// <param name="ledstrip"> </param>
    /// <returns> </returns>
    public LedstripDisplayState? GetLedstripDisplayState(Ledstrip ledstrip)
    {
        return _ledstripDisplayStates.FirstOrDefault(x => x.Connection.Ledstrip == ledstrip);
    }


    /// <summary>
    /// Start tracking the device connection with all its ledstrips.
    /// </summary>
    /// <param name="deviceConnection"> </param>
    public void TrackDeviceConnection(IDeviceConnection deviceConnection)
    {
        foreach (ILedstripConnection ledstripConnection in deviceConnection.LedstripConnections)
        {
            _ledstripDisplayStates.Add(new LedstripDisplayState(ledstripConnection));
        }
    }


    /// <summary>
    /// Remove device connection from the context and stop tracking it.
    /// </summary>
    /// <param name="deviceConnection"> </param>
    public void RemoveDeviceConnection(IDeviceConnection deviceConnection)
    {
        foreach (ILedstripConnection ledstripConnection in deviceConnection.LedstripConnections)
        {
            _ledstripDisplayStates.RemoveAll(x => x.Connection == ledstripConnection);
        }
    }
}