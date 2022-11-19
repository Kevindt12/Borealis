using Borealis.Domain.Devices;



namespace Borealis.Portal.Rpi.Configurations;


public class LedstripSettings
{
    public int Length { get; set; }

    public ColorSpectrum Colors { get; set; }


    public ConnectionSettings Connection { get; set; } = new ConnectionSettings();
}