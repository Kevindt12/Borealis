using System.Text;



namespace Borealis.Domain.Communication.Messages;


public class ConnectMessage : MessageBase
{
    public string ConfigurationConcurrencyToken { get; init; }


    public ConnectMessage(string configurationConcurrencyToken)
    {
        ConfigurationConcurrencyToken = configurationConcurrencyToken;
    }


    public static ConnectMessage FromBuffer(ReadOnlyMemory<byte> buffer)
    {
        string configurationConcurrencyToken = Encoding.ASCII.GetString(buffer.Span);

        return new ConnectMessage(configurationConcurrencyToken);
    }


    /// <inheritdoc />
    public override ReadOnlyMemory<Byte> Serialize()
    {
        byte[] buffer = new Byte[128];

        // Wiring the concurrency token that we generate when changing the congfiguration.
        Encoding.ASCII.GetBytes(ConfigurationConcurrencyToken).CopyTo(buffer, 0);

        return buffer;
    }
}