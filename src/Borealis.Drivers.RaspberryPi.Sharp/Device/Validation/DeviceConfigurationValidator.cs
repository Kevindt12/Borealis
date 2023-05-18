using Borealis.Drivers.RaspberryPi.Sharp.Device.Models;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Providers;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Models;



namespace Borealis.Drivers.RaspberryPi.Sharp.Device.Validation;


public class DeviceConfigurationValidator : IDeviceConfigurationValidator
{
    private readonly IBusSettingsProvider _busSettingsProvider;


    public DeviceConfigurationValidator(IBusSettingsProvider busSettingsProvider)
    {
        _busSettingsProvider = busSettingsProvider;
    }


    /// <inheritdoc />
    public Task<DeviceConfigurationValidationResult> ValidateAsync(DeviceConfiguration configuration, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        List<string> errors = new List<string>();

        if (String.IsNullOrEmpty(configuration.ConcurrencyToken))
        {
            errors.Add("The configuration concurrency token is not set.");
        }

        foreach (Ledstrip ledstrip in configuration.Ledstrips)
        {
            if (!_busSettingsProvider.GetBuses().Select(x => x.Key).Contains(ledstrip.Bus))
            {
                errors.Add($"The bus {ledstrip.Bus} is not configured on the device.");
            }
        }

        return Task.FromResult(errors.Count == 0 ? DeviceConfigurationValidationResult.Success : DeviceConfigurationValidationResult.Failed(errors.ToArray()));
    }
}