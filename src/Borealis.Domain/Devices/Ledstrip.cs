namespace Borealis.Domain.Devices;


public class Ledstrip
{
    /// <summary>
    /// The number of pixels on the ledstrip.
    /// </summary>
    public int Length { get; init; }

    /// <summary>
    /// The colors that we can display on the ledstrip.
    /// </summary>
    public ColorSpectrum Colors { get; init; }

    /// <summary>
    /// The protocol that we need to use to communicate with the ledstrip.
    /// </summary>
    public LedstripProtocol Protocol { get; init; }

    /// <summary>
    /// The name of the ledstrip.
    /// </summary>
    public string? Name { get; init; }


    /// <summary>
    /// The connection settings used for the ledstrip.
    /// </summary>
    public ConnectionSettings? Connection { get; set; }


    /// <summary>
    /// The ledstrip that we can use to display something.
    /// </summary>
    public Ledstrip() { }
}