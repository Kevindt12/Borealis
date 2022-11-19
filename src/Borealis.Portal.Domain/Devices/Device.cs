using System;
using System.Linq;
using System.Net;
using System.Text.Json;

using Borealis.Domain.Devices;



namespace Borealis.Portal.Domain.Devices;


public class Device : IEquatable<Device>
{
    private LedstripSettings? _configuration;
    private string _configurationJson;

    /// <summary>
    /// The id of the device.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the device.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The ledstrips of the device. This is gotten by its json.
    /// </summary>
    public virtual LedstripSettings? Configuration
    {
        get
        {
            try
            {
                return _configuration ??= JsonSerializer.Deserialize<LedstripSettings>(ConfigurationJson);
            }
            catch (JsonException e)
            {
                // Don't care if we cant deserialize it
                return null;
            }
        }
    }

    /// <summary>
    /// The endpoint of the device.
    /// </summary>
    public IPEndPoint EndPoint { get; set; }

    /// <summary>
    /// If we should auto connect with the device.
    /// </summary>
    public bool AutoConnect { get; set; }

    /// <summary>
    /// The json configuration.
    /// </summary>
    public string ConfigurationJson
    {
        get => _configurationJson;
        internal set
        {
            _configurationJson = value;

            try
            {
                _configuration = JsonSerializer.Deserialize<LedstripSettings>(value);
            }
            catch (JsonException e)
            {
                _configuration = null;
            }
        }
    }

    /// <summary>
    /// Checks if we have a valid configuration to work with.
    /// </summary>
    public bool HasValidConfiguration
    {
        get
        {
            try
            {
                JsonSerializer.Deserialize<LedstripSettings>(ConfigurationJson);

                return true;
            }
            catch (JsonException e)
            {
                return false;
            }
        }
    }


    /// <summary>
    /// The type of connection we will be using to connect with this driver.
    /// </summary>
    public ConnectionType ConnectionType { get; set; }


    public Device() { }


    /// <summary>
    /// The device where the ledstrip are connect to.
    /// </summary>
    internal Device(String name, IPEndPoint endPoint)
    {
        Name = name;
        EndPoint = endPoint;
        _configurationJson = string.Empty;
    }


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
}