using System.Text;



namespace Borealis.Domain.Communication.Messages;


public sealed class ErrorMessage : MessageBase
{
    private readonly string? _message;

    public string Message => _message ?? (Exception.InnerException ?? Exception)?.Message ?? "Unknown Error.";

    public Exception? Exception { get; }


    /// <summary>
    /// The Error message used for deserialization.
    /// </summary>
    /// <param name="buffer"> </param>
    public ErrorMessage(ReadOnlyMemory<byte> buffer)
    {
        // Deserialize message.
        int length = BitConverter.ToInt32(buffer[..3].ToArray(), 0);
        PayloadType type = (PayloadType)buffer.Span[4];
        string message = Encoding.ASCII.GetString(buffer[5..].Span);

        if (type == PayloadType.Exception)
        {
            Type? exceptionType = Type.GetType(message);

            if (exceptionType == null)
            {
                _message = message;

                return;
            }

            Exception = (Exception)Activator.CreateInstance(exceptionType)!;
        }
        else if (type == PayloadType.String)
        {
            _message = message;
        }
    }


    /// <summary>
    /// A error message based on a <see cref="Exception" />
    /// </summary>
    /// <param name="exception"> The <see cref="Exception" /> that we want to encapsulate in this send-able error message. </param>
    public ErrorMessage(Exception? exception)
    {
        Exception = exception;
    }


    /// <summary>
    /// A error message based on a <see cref="string" /> message.
    /// </summary>
    /// <param name="message"> The string message error. </param>
    public ErrorMessage(string message)
    {
        _message = message;
    }


    /// Type | PayloadType = byte = The enum that selects the error type we are sending in this case | String | Exception.
    /// 
    /// |  length |  Type  |  Type name || String  |   
    /// | 4 bytes |  byte  |      VAR Bytes        |
    /// |   int   |  byte  |  ReadOnlyMemory Bytes |
    /// <inheritdoc />
    public override ReadOnlyMemory<Byte> SerializeMessage()
    {
        // Getting the Variables.
        byte type = (byte)(Exception == null ? PayloadType.String : PayloadType.Exception);
        string message = Exception?.GetType().FullName ?? _message ?? "Unknown";
        int length = message.Length + 1;

        byte[] buffer = new Byte[length];

        BitConverter.GetBytes(length).CopyTo(buffer, 0);
        buffer[4] = type;
        Encoding.ASCII.GetBytes(message).CopyTo(buffer, 5);

        return buffer;
    }



    private enum PayloadType : byte
    {
        Exception,
        String
    }
}