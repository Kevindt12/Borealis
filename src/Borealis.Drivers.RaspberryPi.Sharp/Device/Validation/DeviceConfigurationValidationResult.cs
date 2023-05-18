using System.Text;



namespace Borealis.Drivers.RaspberryPi.Sharp.Device.Validation;


/// <summary>
/// The result of the device validation.
/// </summary>
public class DeviceConfigurationValidationResult
{
    /// <summary>
    /// Indicating that the validation was successful.
    /// </summary>
    public bool Successful { get; init; }


    /// <summary>
    /// The errors in the validation.
    /// </summary>
    public string[] Errors { get; init; } = Array.Empty<string>();


    /// <summary>
    /// Successful validation.
    /// </summary>
    public static DeviceConfigurationValidationResult Success => new DeviceConfigurationValidationResult
    {
        Successful = true
    };


    /// <summary>
    /// Failed validation.
    /// </summary>
    /// <param name="errors"> The validation errors. </param>
    public static DeviceConfigurationValidationResult Failed(params string[] errors) => new DeviceConfigurationValidationResult
    {
        Successful = false,
        Errors = errors
    };


    /// <summary>
    /// Generates a single error messages from all the error messages.
    /// </summary>
    /// <returns> A single string message of all the error messages. </returns>
    public string GenerateSingleErrorMessage()
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < Errors.Length; i++)
        {
            builder.AppendLine($"Error {i} : {Errors[i]},");
        }

        return builder.ToString();
    }
}