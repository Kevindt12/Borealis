using Borealis.Domain.Ledstrips;
using Borealis.Portal.Domain.Devices.Models;



namespace Borealis.Portal.Domain.Connectivity.Models;


public class DeviceStatus
{
    public IDictionary<Ledstrip, LedstripStatus> Statuses { get; set; }


    public DeviceStatus(IDictionary<Ledstrip, LedstripStatus> statuses)
    {
        Statuses = statuses;
    }
}