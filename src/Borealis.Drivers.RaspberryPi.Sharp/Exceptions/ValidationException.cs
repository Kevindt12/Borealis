namespace Borealis.Drivers.RaspberryPi.Sharp.Exceptions;


/// <summary>
/// Exception when an validation has errors.
/// </summary>
public class ValidationException : ApplicationException
{
    public string[] Errors { get; set; } = Array.Empty<string>();


    /// <inheritdoc />
    public ValidationException() { }


    /// <inheritdoc />
    public ValidationException(String? message) : base(message) { }


    /// <inheritdoc />
    public ValidationException(String? message, Exception? innerException) : base(message, innerException) { }


    /// <inheritdoc />
    public ValidationException(String[] errors)
    {
        Errors = errors;
    }


    /// <inheritdoc />
    public ValidationException(String? message, String[] errors) : base(message)
    {
        Errors = errors;
    }


    /// <inheritdoc />
    public ValidationException(String? message, Exception? innerException, String[] errors) : base(message, innerException)
    {
        Errors = errors;
    }
}