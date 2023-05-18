using System;
using System.Linq;
using System.Net;

using Borealis.Domain.Ledstrips;
using Borealis.Portal.Domain.Exceptions;



namespace Borealis.Portal.Domain.Devices.Models;


public class Device : IEquatable<Device>
{
    private readonly List<DevicePort> _ports;

    public const int ConcurrencyTokenLength = 128;

    /// <summary>
    /// The id of the device.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the device.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The ports that are connected on the device.
    /// </summary>
    public IReadOnlyList<DevicePort> Ports => _ports;

    /// <summary>
    /// The concurrency token of the ports.
    /// </summary>
    public string ConfigurationConcurrencyToken { get; set; }


    /// <summary>
    /// The endpoint of the device.
    /// </summary>
    public IPEndPoint EndPoint { get; set; }

    /// <summary>
    /// If we should auto connect with the device.
    /// </summary>
    public bool AutoConnect { get; set; }


    public Device()
    {
        _ports = new List<DevicePort>();
        EndPoint = new IPEndPoint(IPAddress.None, 0);
        ConfigurationConcurrencyToken = GenerateToken();
    }


    /// <summary>
    /// The device where the ledstrip are connect to.
    /// </summary>
    internal Device(String name, IPEndPoint endPoint)
    {
        _ports = new List<DevicePort>();
        Name = name;
        EndPoint = endPoint;
    }


    /// <summary>
    /// Attaches a new ledstrip to the port that was specified or creates the new port if there is not one.
    /// </summary>
    /// <param name="bus"> The bus that we want to use to connect to. </param>
    /// <param name="ledstrip"> The ledstrip that we want to attach. </param>
    public void AttachLedstrip(byte bus, Ledstrip ledstrip)
    {
        DevicePort? port;

        if ((port = _ports.SingleOrDefault(p => p.Bus == bus)) == null)
        {
            port = new DevicePort(this, bus, ledstrip);
            _ports.Add(port);
        }
        else
        {
            port.Ledstrip = ledstrip;
        }

        ConfigurationConcurrencyToken = GenerateToken();
    }


    /// <summary>
    /// Adds a new bus to the device.
    /// </summary>
    /// <param name="bus"> </param>
    public void AddBus(byte bus)
    {
        if (_ports.Any(x => x.Bus == bus)) throw new InvalidDeviceConfigurationException("The port that was selected is already in use.");
        DevicePort port = new DevicePort(this, bus);

        _ports.Add(port);
        ConfigurationConcurrencyToken = GenerateToken();
    }


    /// <summary>
    /// Changes the bus id of the connection.
    /// </summary>
    /// <param name="oldBus"> The old bus id. </param>
    /// <param name="newBus"> The new bus id. </param>
    public void ChangeBusId(byte oldBus, byte newBus)
    {
        if (_ports.Any(x => x.Bus == newBus)) throw new InvalidDeviceConfigurationException("The port that was selected is already in use.");

        DevicePort port = _ports.Single(x => x.Bus == oldBus);

        port.Bus = newBus;
        ConfigurationConcurrencyToken = GenerateToken();
    }


    /// <summary>
    /// Detaches the ledstrip from the bus.
    /// </summary>
    /// <param name="ledstrip"> The ledstrip that we want to remove from the connections </param>
    /// <exception cref="InvalidOperationException"> If the ledstrip was not found. </exception>
    public void DetachLedstrip(Ledstrip ledstrip)
    {
        DevicePort port = _ports.SingleOrDefault(x => x.Ledstrip == ledstrip) ?? throw new InvalidOperationException("The ledstrip was not found.");

        port.Ledstrip = null;
        ConfigurationConcurrencyToken = GenerateToken();
    }


    /// <summary>
    /// Removes the bus from the device.
    /// </summary>
    /// <param name="bus"> The bus that was attached on the ledstrip. </param>
    public void RemoveBus(byte bus)
    {
        // Removes the bus from the ledstrip.
        _ports.RemoveAll(x => x.Bus == bus);
        ConfigurationConcurrencyToken = GenerateToken();
    }


    /// <summary>
    /// Generates a new concurrency token.
    /// </summary>
    /// <returns> </returns>
    private string GenerateToken()
    {
        Random random = new Random();
        byte[] buffer = new byte[ConcurrencyTokenLength];
        random.NextBytes(buffer);

        return Convert.ToBase64String(buffer);
    }


    /// <inheritdoc />
    public override String ToString()
    {
        return $"{Name} ({EndPoint})";
    }


    #region Equals

    /// <inheritdoc />
    public bool Equals(Device? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Id.Equals(other.Id);
    }


    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((Device)obj);
    }


    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }


    public static bool operator ==(Device? left, Device? right)
    {
        return Equals(left, right);
    }


    public static bool operator !=(Device? left, Device? right)
    {
        return !Equals(left, right);
    }

    #endregion
}