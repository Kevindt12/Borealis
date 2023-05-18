namespace Borealis.Drivers.RaspberryPi.Sharp.Connection.Controllers;


public class ConnectResult
{
    /// <summary>
    /// Indicating that we have an successful connection.
    /// </summary>
    public bool Successful { get; init; }


    /// <summary>
    /// Indicating that the device configuration has changed. Because the token received is not the same as the one stored.
    /// </summary>
    public bool DeviceConfigurationValid { get; init; }


    public static ConnectResult Success => new ConnectResult
    {
        Successful = true
    };


    public static ConnectResult DeviceConfigurationChanged()
    {
        return new ConnectResult
        {
            Successful = false,
            DeviceConfigurationValid = true
        };
    }
}