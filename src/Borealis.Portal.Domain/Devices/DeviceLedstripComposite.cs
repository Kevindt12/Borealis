using Borealis.Domain.Devices;



namespace Borealis.Portal.Domain.Devices;


public class DeviceLedstripComposite : IEquatable<DeviceLedstripComposite>
{
    /// <inheritdoc />
    public bool Equals(DeviceLedstripComposite? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return _ledstripIndex == other._ledstripIndex && Device.Equals(other.Device);
    }


    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((DeviceLedstripComposite)obj);
    }


    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(_ledstripIndex, Device);
    }


    public static bool operator ==(DeviceLedstripComposite? left, DeviceLedstripComposite? right)
    {
        return Equals(left, right);
    }


    public static bool operator !=(DeviceLedstripComposite? left, DeviceLedstripComposite? right)
    {
        return !Equals(left, right);
    }


    private readonly int _ledstripIndex;

    public Device Device { get; set; }


    public Ledstrip Ledstrip => Device.Configuration!.Ledstrips[_ledstripIndex];


    public DeviceLedstripComposite(Device device, Ledstrip ledstrip)
    {
        _ledstripIndex = device.Configuration!.Ledstrips.IndexOf(ledstrip);
        Device = device;
    }
}