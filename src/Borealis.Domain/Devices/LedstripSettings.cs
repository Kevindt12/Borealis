namespace Borealis.Domain.Devices;


public class LedstripSettings
{
    /// <summary>
    /// The ledstrips that we have configured.
    /// </summary>
    public List<Ledstrip> Ledstrips { get; init; } = new List<Ledstrip>();


    /// <summary>
    /// The configuration that holds all the ledstrip data.
    /// </summary>
    public LedstripSettings() { }
}