using System.Text;
using System.Text.Json;

using Borealis.Domain.Communication.Exceptions;
using Borealis.Shared.Extensions;



namespace Borealis.Domain.Communication.Messages;


public sealed class ErrorMessage : MessageBase
{
    /// <summary>
    /// The error message that we want to display.
    /// </summary>
    /// <remarks>
    /// If we don't supply a message we will just use the one from the exception.
    /// </remarks>
    public string? Message { get; init; }

    //  _message ?? (Exception?.InnerException ?? Exception)?.Message ?? "Unknown Error.";

    /// <summary>
    /// A optional exception that we can fill to send to the other side.
    /// </summary>
    /// <remarks>
    /// Note that if the <see cref="Type" /> does not exist in a other assembly then the calling one it will default to
    /// <see cref="null" />
    /// </remarks>
    public ExceptionInfo? Exception { get; init; }


    protected ErrorMessage(ExceptionInfo? info, string? message) { }


    /// <summary>
    /// Creates a error message from a received buffer.
    /// </summary>
    /// <param name="buffer"> The buffer we received. </param>
    /// <returns> A <see cref="ErrorMessage" /> that has been populated. </returns>
    /// <exception cref="CommunicationException"> When the format of the payload is not correct. </exception>
    public static ErrorMessage FromBuffer(ReadOnlyMemory<byte> buffer)
    {
        try
        {
            return JsonSerializer.Deserialize<ErrorMessage>(buffer.Span)!;
        }
        catch (JsonException e)
        {
            throw new CommunicationException("Wrong payload format.", e);
        }
    }


    /// <summary>
    /// A error message based on a <see cref="Exception" />
    /// </summary>
    /// <param name="exception"> The <see cref="Exception" /> that we want to encapsulate in this send-able error message. </param>
    public ErrorMessage(Exception exception)
    {
        Exception = new ExceptionInfo(exception);
    }


    /// <summary>
    /// A error message based on a <see cref="string" /> message.
    /// </summary>
    /// <param name="message"> The string message error. </param>
    public ErrorMessage(string message)
    {
        Message = message;
    }


    /// <summary>
    /// A error message based on a <see cref="string" /> message.
    /// </summary>
    /// <param name="message"> The string message error. </param>
    /// <param name="exception"> The <see cref="Exception" /> that we want to encapsulate in this send-able error message. </param>
    public ErrorMessage(string message, Exception exception)
    {
        Message = message;
        Exception = new ExceptionInfo(exception);
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