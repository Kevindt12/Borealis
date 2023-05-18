using System;
using System.Linq;

using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Contexts;



namespace Borealis.Drivers.RaspberryPi.Sharp.Device.Models;


public class DriverLedstripStatus
{
    public Guid Ledstrip { get; set; }

    public LedstripStatus Status { get; set; }
}