using System.Net;

using Borealis.Portal.Domain.Exceptions;



namespace Borealis.Portal.Domain.Devices;


/// <summary>
/// The factory used for create new devices and update existing ones.
/// </summary>
public interface IDeviceFactory
{
    /// <summary>
    /// Creates a new device with some boilerplate attached.
    /// </summary>
    /// <exception cref="ArgumentNullException"> When the name is null or whitespace. </exception>
    /// <param name="name"> The name of the device. </param>
    /// <param name="endPoint"> The ip endpoint of the device. </param>
    /// <returns> A configured device. </returns>
    Device CreateDevice(string name, IPEndPoint endPoint);


    /// <summary>
    /// Changes the device configuration.
    /// </summary>
    /// <exception cref="InvalidDeviceConfigurationException"> Thrown when the configuration is not valid. </exception>
    /// <param name="device"> The device we want to change the configuration of. </param>
    /// <param name="configurationJson"> The configuration we want to save. </param>
    void ChangeDeviceConfiguration(Device device, string configurationJson);


    /// <summary>
    /// Validates a configuration.
    /// </summary>
    /// <param name="configurationJson"> </param>
    /// <returns> </returns>
    ConfigurationValidationResult ValidateConfigurationJson(string configurationJson);
}