namespace Borealis.Domain.Communication.Messages;


public class ConnectedMessage : MessageBase
{
    public bool IsConfigurationValid { get; init; }


    public ConnectedMessage(Boolean isConfigurationValid)
    {
        IsConfigurationValid = isConfigurationValid;
    }


    public static ConnectedMessage FromBuffer(ReadOnlyMemory<byte> buffer)
    {
        bool isConfigurationValid = BitConverter.ToBoolean(buffer.ToArray(), 0);

        return new ConnectedMessage(isConfigurationValid);
    }


    /// <inheritdoc />
    public override ReadOnlyMemory<Byte> Serialize()
    {
        byte[] buffer = { BitConverter.GetBytes(IsConfigurationValid)[0] };

        return buffer;
    }
}