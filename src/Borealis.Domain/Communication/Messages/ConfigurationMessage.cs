using System.Text;
using System.Text.Json;

using Borealis.Domain.Communication.Exceptions;
using Borealis.Domain.Devices;



namespace Borealis.Domain.Communication.Messages;


public class ConfigurationMessage : MessageBase
{
    /// <summary>
    /// The ledstrips configuration.
    /// </summary>
    public LedstripSettings Settings { get; init; }


    /// <summary>
    /// A configuration message that can be send to driver devices.
    /// </summary>
    /// <param name="settings"> The configuration settings that we want to send to the device. </param>
    public ConfigurationMessage(LedstripSettings settings)
    {
        Settings = settings;
    }


    /// <summary>
    /// Creates a new <see cref="ConfigurationMessage" /> that can be deserialized.
    /// </summary>
    /// <param name="buffer"> The buffer payload that we got. </param>
    /// <returns> A populated <see cref="ConfigurationMessage" /> message. </returns>
    /// <exception cref="CommunicationException"> When the payload Json was unable to be deserialized. </exception>
    public static ConfigurationMessage FromBuffer(ReadOnlyMemory<byte> buffer)
    {
        try
        {
            // Reading the json that we got.
            string json = Encoding.ASCII.GetString(buffer.Span);

            // Deserialize the ledstrip configuration.
            LedstripSettings ledstrip = JsonSerializer.Deserialize<LedstripSettings>(json)!;

            // Returning the configuration.
            return new ConfigurationMessage(ledstrip);
        }
        catch (JsonException jsonException)
        {
            throw new CommunicationException("The Json of a configuration package could not be deserialized.", jsonException);
        }
    }


    /// <inheritdoc />
    public override ReadOnlyMemory<Byte> Serialize()
    {
        // Serializing the settings.
        string json = JsonSerializer.Serialize(Settings);

        // Encoding it and sending it out.
        return Encoding.ASCII.GetBytes(json);
    }
}