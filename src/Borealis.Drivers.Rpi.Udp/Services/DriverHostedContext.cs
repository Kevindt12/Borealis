using System;

using FluentValidation;

using System.Linq;
using System.Text.Json;

using Borealis.Domain.Devices;
using Borealis.Drivers.Rpi.Udp.Contexts;
using Borealis.Drivers.Rpi.Udp.Validation;



namespace Borealis.Drivers.Rpi.Udp.Services;


public class DriverHostedService : IHostedService
{
    private readonly ILogger<DriverHostedService> _logger;
    private readonly IConfiguration _configuration;

    private readonly LedstripContext _ledstripContext;
    private readonly LedstripSettingsValidator _ledstripSettingsValidator;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;


    public DriverHostedService(ILogger<DriverHostedService> logger,
                               LedstripContext ledstripContext,
                               LedstripSettingsValidator ledstripSettingsValidator,
                               IHostApplicationLifetime hostApplicationLifetime,
                               IConfiguration configuration
    )
    {
        _logger = logger;
        _ledstripContext = ledstripContext;
        _ledstripSettingsValidator = ledstripSettingsValidator;
        _hostApplicationLifetime = hostApplicationLifetime;
        _configuration = configuration;
    }


    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Getting the configuration path.
            string settingsRelativePath = _configuration.GetValue<string>("SettingsPath") ?? throw new InvalidOperationException("There was no path to configuration file.");

            // Getting the path and checking that there is a file there.
            string settingsAbsolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, settingsRelativePath);

            if (!File.Exists(settingsAbsolutePath)) throw new InvalidOperationException("There was no settings file found.");

            // Reading the json.
            _logger.LogInformation($"Settings path {settingsAbsolutePath}. Start reading Json.");
            string json = await File.ReadAllTextAsync(settingsAbsolutePath, cancellationToken);

            // Deserializing the Json
            LedstripSettings settings = JsonSerializer.Deserialize<LedstripSettings>(json)!;
            _logger.LogInformation("Json has been successfully parsed.");

            _logger.LogInformation("Validating settings");
            await _ledstripSettingsValidator.ValidateAndThrowAsync(settings, cancellationToken);

            _logger.LogInformation("Loading all ledstrips.");
            _ledstripContext.SetConfiguration(settings);
        }
        catch (InvalidOperationException e)
        {
            // Domain rules there needs to be least a empty array defined.

            // When there is no configuration then this is a fatal error. Because we cant do anything without it.
            _logger.LogError(e, "Unable to load the configuration.");
            _hostApplicationLifetime.StopApplication();
        }
        catch (ValidationException validationException)
        {
            // Handle invalid configuration.
            _logger.LogError(validationException.InnerException, "The validation of the settings failed.");
            _hostApplicationLifetime.StopApplication();
        }
        catch (JsonException e)
        {
            // Invalid Json.
            _logger.LogError(e, "Invalid Json.");
            _hostApplicationLifetime.StopApplication();
        }
    }


    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken) { }
}