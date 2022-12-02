using System.Text.Json;



namespace Borealis.Shared.Extensions;


public static class StringExtensions
{
    // TODO: Replace with structured logging.
    /// <summary>
    /// Logs a object to a json format for logging.
    /// </summary>
    /// <returns> A string for readable logging. </returns>
    public static string LogToJson(this object? value)
    {
        if (value == null) return "Object was null";

        string resultString = string.Empty;

        resultString += $"Object Type : {value.GetType().Name} Has Value | ";

        try
        {
            resultString += JsonSerializer.Serialize(value,
                                                     new JsonSerializerOptions
                                                         { WriteIndented = true });
        }
        catch (JsonException e)
        {
            resultString = $"Failed to write Json {e.Message}";
        }

        return resultString;
    }
}