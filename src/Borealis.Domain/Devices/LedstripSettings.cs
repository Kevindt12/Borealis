namespace Borealis.Domain.Devices;


public class LedstripSettings
{
    /// <summary>
    /// The ledstrips that we have configured.
    /// </summary>
    public List<Ledstrip> Ledstrips { get; init; } = new List<Ledstrip>();


    /// <summary>
    /// The token that changes when we change the settings. Indicating everywhere that we have changed the settings.
    /// </summary>
    public string Token { get; set; }


    /// <summary>
    /// The configuration that holds all the ledstrip data.
    /// </summary>
    public LedstripSettings() { }
}