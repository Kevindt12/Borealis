using System;

using FluentValidation;

using System.Linq;
using System.Reflection;

using Borealis.Drivers.Rpi.Udp.Contexts;
using Borealis.Drivers.Rpi.Udp.Ledstrips;
using Borealis.Drivers.Rpi.Udp.Services;

using NLog;
using NLog.Extensions.Logging;



namespace Borealis.Drivers.Rpi.Udp;


public static class Program
{
    public static async Task Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
                         .ConfigureAppConfiguration(ConfigureConfiguration)
                         .ConfigureServices(ConfigureServices)
                         .ConfigureLogging(ConfigureLogging)
                         .Build();

        // Start the application.
        await host.RunAsync();
    }


    private static void ConfigureServices(IServiceCollection services)
    {
        // Middleware
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient);

        // Factories.
        services.AddTransient<LedstripProxyFactory>();

        // State
        services.AddSingleton<LedstripContext>();

        // Hosting
        services.AddHostedService<DriverHostedService>();
        services.AddHostedService<ServerHostedService>();
    }


    private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
    {
        // Setting the NLog configuration.
        LogManager.Configuration = new NLogLoggingConfiguration(context.Configuration.GetSection("NLog"));

        // Using NLog for logging.
        builder.ClearProviders();
        builder.AddNLog();
    }


    private static void ConfigureConfiguration(IConfigurationBuilder builder)
    {
        IConfiguration config = builder.SetBasePath(Directory.GetCurrentDirectory())
                                       .AddJsonFile("appsettings.json", false, true)
                                       .Build();
    }
}