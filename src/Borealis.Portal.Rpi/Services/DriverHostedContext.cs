using System.Text.Json;

using Borealis.Portal.Rpi.Configurations;
using Borealis.Portal.Rpi.Contexts;



namespace Borealis.Portal.Rpi.Services;


public class DriverHostedService : IHostedService
{
    private readonly ILogger<DriverHostedService> _logger;
    private readonly LedstripContext _ledstripContext;
    private readonly IConfiguration _configuration;


    public DriverHostedService(ILogger<DriverHostedService> logger, LedstripContext ledstripContext, IConfiguration configuration)
    {
        _logger = logger;
        _ledstripContext = ledstripContext;
        _configuration = configuration;
    }


    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string configurationLocation = _configuration.GetValue<string>("ConfigurationPath")!;

        DeviceSettings settings = JsonSerializer.Deserialize<DeviceSettings>(await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configurationLocation), cancellationToken))!;

        _ledstripContext.SetConfiguration(settings);
    }


    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _ledstripContext.Dispose();
    }
}