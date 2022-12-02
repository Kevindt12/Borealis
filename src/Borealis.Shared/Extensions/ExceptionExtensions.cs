using System.Text.Json;
using System.Text.Json.Serialization;



namespace Borealis.Shared.Extensions;


// https://stackoverflow.com/questions/35358190/how-to-serialise-exception-to-json



public static class ExceptionExtensions
{
    private static readonly JsonSerializerOptions _defaultJsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };


    /// <summary>
    /// Serialize the <see cref="Exception" /> to a JSON string.
    /// </summary>
    /// <param name="ex"> The exception </param>
    /// <param name="includeInnerException"> Control if to include inner exception </param>
    /// <param name="includeStackTrace"> Control if to include stack trace </param>
    /// <param name="options"> JSON options. By default nulls are not serialized and the string is indented </param>
    /// <returns> </returns>
    public static string ToJson(this Exception ex,
                                bool includeInnerException = true,
                                bool includeStackTrace = false,
                                JsonSerializerOptions options = null
    )
    {
        ArgumentNullException.ThrowIfNull(ex);
        ExceptionInfo info = new ExceptionInfo(ex, includeInnerException, includeStackTrace);

        return JsonSerializer.Serialize(info, options ?? _defaultJsonSerializerOptions);
    }
}