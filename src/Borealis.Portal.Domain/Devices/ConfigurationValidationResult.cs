namespace Borealis.Portal.Domain.Devices;


public class ConfigurationValidationResult
{
    /// <summary>
    /// If the validation succeeded.
    /// </summary>
    public bool Success { get; private init; }

    /// <summary>
    /// The exception that was cached while validating the configuration.
    /// </summary>
    public Exception? Exception { get; private init; }


    public static ConfigurationValidationResult Successful =>
        new ConfigurationValidationResult
            { Success = true };


    public static ConfigurationValidationResult Failed(Exception exception) =>
        new ConfigurationValidationResult
            { Success = false, Exception = exception };
}