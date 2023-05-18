using System;
using System.Linq;

using Borealis.Domain.Ledstrips;



namespace Borealis.Drivers.Rpi.Exceptions;


public class InvalidLedstripSettingsException : ApplicationException
{
    public string? Json { get; set; }


    public Ledstrip? Ledstrip { get; set; }


    /// <inheritdoc />
    public InvalidLedstripSettingsException() { }


    /// <inheritdoc />
    public InvalidLedstripSettingsException(String? message) : base(message) { }


    /// <inheritdoc />
    public InvalidLedstripSettingsException(String? message, Exception? innerException) : base(message, innerException) { }


    /// <inheritdoc />
    public InvalidLedstripSettingsException(Ledstrip? ledstrip)
    {
        Ledstrip = ledstrip;
    }


    /// <inheritdoc />
    public InvalidLedstripSettingsException(String? message, Ledstrip? ledstrip) : base(message)
    {
        Ledstrip = ledstrip;
    }


    /// <inheritdoc />
    public InvalidLedstripSettingsException(String? message, Exception? innerException, Ledstrip? ledstrip) : base(message, innerException)
    {
        Ledstrip = ledstrip;
    }


    /// <inheritdoc />
    public InvalidLedstripSettingsException(String? message, String? json) : base(message)
    {
        Json = json;
    }


    /// <inheritdoc />
    public InvalidLedstripSettingsException(String? message, Exception? innerException, String? json) : base(message, innerException)
    {
        Json = json;
    }


    /// <inheritdoc />
    public InvalidLedstripSettingsException(String? message, String? json, Ledstrip? ledstrip) : base(message)
    {
        Json = json;
        Ledstrip = ledstrip;
    }


    /// <inheritdoc />
    public InvalidLedstripSettingsException(String? message, Exception? innerException, String? json, Ledstrip? ledstrip) : base(message, innerException)
    {
        Json = json;
        Ledstrip = ledstrip;
    }
}