using System.Text;
using System.Text.Json;

using Borealis.Domain.Communication.Exceptions;
using Borealis.Shared.Extensions;



namespace Borealis.Domain.Communication.Messages;


public class StackSizeMessage : MessageBase
{
    public int StackSize { get; init; }


    protected StackSizeMessage(ExceptionInfo? info, string? message) { }


    /// <summary>
    /// Creates a error message from a received buffer.
    /// </summary>
    /// <param name="buffer"> The buffer we received. </param>
    /// <returns> A <see cref="ErrorMessage" /> that has been populated. </returns>
    /// <exception cref="CommunicationException"> When the format of the payload is not correct. </exception>
    public static StackSizeMessage FromBuffer(ReadOnlyMemory<byte> buffer)
    {
        try
        {
            return JsonSerializer.Deserialize<StackSizeMessage>(buffer.Span)!;
        }
        catch (JsonException e)
        {
            throw new CommunicationException("Wrong payload format.", e);
        }
    }


    /// <summary>
    /// A error message based on a <see cref="string" /> message.
    /// </summary>
    /// <param name="stackSize"> The string message error. </param>
    public StackSizeMessage(int stackSize)
    {
        StackSize = stackSize;
    }


    /// <inheritdoc />
    public override ReadOnlyMemory<Byte> Serialize()
    {
        // Serialize to json.
        string json = JsonSerializer.Serialize(this);

        // Creating and filling buffer.
        byte[] buffer = new Byte[json.Length];
        Encoding.UTF8.GetBytes(json).CopyTo(buffer, 0);

        return buffer;
    }
}