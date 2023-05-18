﻿using System.Device.Spi;

using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;

using Iot.Device.Ws28xx;

using Microsoft.Extensions.Logging;



namespace Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Proxies;


public class Ws2815LedstripProxy : NeoPixelStandardLedstripProxy
{
    /// <inheritdoc />
    public Ws2815LedstripProxy(ILogger logger, Ledstrip ledstrip, Bus bus, LedstripSettings settings) : base(logger, ledstrip, bus, settings) { }


    /// <inheritdoc />
    protected override Ws28xx CreateLedstripDevice(SpiDevice spiDevice, Int32 pixelCount)
    {
        return new Ws2815b(spiDevice, pixelCount);
    }
}