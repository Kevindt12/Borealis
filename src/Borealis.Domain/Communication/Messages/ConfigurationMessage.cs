using System.Text;
using System.Text.Json;

using Borealis.Domain.Devices;



namespace Borealis.Domain.Communication.Messages;


public class ConfigurationMessage : MessageBase
{
    /// <summary>
    /// The ledstrips configuration.
    /// </summary>
    public LedstripSettings Settings { get; set; } = new LedstripSettings();


    /// <summary>
    /// This is used for incoming messages to decode.
    /// </summary>
    /// <param name="buffer"> </param>
    protected ConfigurationMessage(ReadOnlyMemory<byte> buffer)
    {
        try
        {
            string json = Encoding.ASCII.GetString(buffer.Span);

            LedstripSettings ledstrip = JsonSerializer.Deserialize<LedstripSettings>(json)!;
        }
        catch (Exception e)
        {
            // TODO: Error handling.
        }
    }


    public static ConfigurationMessage FromBuffer(ReadOnlyMemory<byte> buffer)
    {
        return new ConfigurationMessage(buffer);
    }


    /// <inheritdoc />
    public override ReadOnlyMemory<Byte> SerializeMessage()
    {
        string json = JsonSerializer.Serialize(Settings);

        return Encoding.ASCII.GetBytes(json);
    }
}