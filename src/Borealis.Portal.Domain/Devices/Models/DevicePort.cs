using System;
using System.Linq;

using Borealis.Domain.Ledstrips;



namespace Borealis.Portal.Domain.Devices.Models;


public class DevicePort
{
    /// <summary>
    /// The id of the port.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The Bus of the device.
    /// </summary>
    public byte Bus { get; internal set; }

    /// <summary>
    /// The ledstrip that is connected to that bus.
    /// </summary>
    public Ledstrip? Ledstrip { get; set; }

    /// <summary>
    /// The device that we are connected to.
    /// </summary>
    public Device Device { get; init; }


    internal DevicePort() { }


    /// <summary>
    /// A port on the device where a ledstrip can be connected with.
    /// </summary>
    /// <param name="device"> The parent device that this ports is owned to. </param>
    /// <param name="bus"> The bus id that we are using for this port. </param>
    /// <param name="ledstrip"> The ledstrip that we want to connect. </param>
    internal DevicePort(Device device, byte bus, Ledstrip? ledstrip = null)
    {
        Device = device;
        Bus = bus;
        Ledstrip = ledstrip;
    }
}