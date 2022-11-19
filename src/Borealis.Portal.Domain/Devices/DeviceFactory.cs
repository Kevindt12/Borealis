using System.Net;
using System.Text.Json;

using Borealis.Domain.Devices;
using Borealis.Portal.Domain.Exceptions;



namespace Borealis.Portal.Domain.Devices;


internal class DeviceFactory : IDeviceFactory
{
    public DeviceFactory() { }


    /// <inheritdoc />
    public Device CreateDevice(String name, IPEndPoint endPoint)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

        return new Device(name, endPoint)
        {
            ConfigurationJson = CreateBoilerplateDeviceConfiguration()
        };
    }


    /// <inheritdoc />
    public void ChangeDeviceConfiguration(Device device, String configurationJson)
    {
        // Validate the json configuration.
        ConfigurationValidationResult validationResult = ValidateConfigurationJson(configurationJson);

        // Throw if validation did not work.
        if (!validationResult.Success) throw new InvalidDeviceConfigurationException("The configuration was not valid.", validationResult.Exception);

        // Updating the json.
        device.ConfigurationJson = configurationJson;
    }


    /// <inheritdoc />
    public ConfigurationValidationResult ValidateConfigurationJson(String configurationJson)
    {
        try
        {
            // Testing if we can deserialize the json.
            _ = JsonSerializer.Deserialize<LedstripSettings>(configurationJson);

            return ConfigurationValidationResult.Successful;
        }
        catch (JsonException e)
        {
            // If we failed then throw a exception.
            return ConfigurationValidationResult.Failed(e);
        }
    }


    //// System.NullReferenceException
    //HResult= 0x80004003

    //Message=Object reference not set to an instance of an object.

    //Source=Borealis.Portal.Domain
    //    StackTrace:

    //at Borealis.Portal.Domain.Devices.DeviceFactory.CreateBoilerplateDeviceConfiguration() in D:\Documents\Projects\Small Projects\Borealis.Portal.Domain\Devices\DeviceFactory.cs:line 69


    /// <summary>
    /// Creates a new json model with a single ledstrip as example.
    /// </summary>
    /// <returns> A boiler plate device configuration. </returns>
    protected virtual string CreateBoilerplateDeviceConfiguration()
    {
        return JsonSerializer.Serialize(new LedstripSettings
                                        {
                                            Ledstrips =
                                            {
                                                new Ledstrip
                                                {
                                                    Name = "First Ledstrip", Length = 20, Colors = ColorSpectrum.Rgb
                                                }
                                            }
                                        },
                                        new JsonSerializerOptions
                                            { WriteIndented = true });
    }
}