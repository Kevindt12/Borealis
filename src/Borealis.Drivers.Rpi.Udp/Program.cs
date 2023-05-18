using System;
using System.Linq;

using Borealis.Drivers.Rpi.Animations;
using Borealis.Drivers.Rpi.Commands;
using Borealis.Drivers.Rpi.Commands.Actions;
using Borealis.Drivers.Rpi.Commands.Handlers;
using Borealis.Drivers.Rpi.Connections;
using Borealis.Drivers.Rpi.Contexts;
using Borealis.Drivers.Rpi.Ledstrips;
using Borealis.Drivers.Rpi.Options;
using Borealis.Drivers.Rpi.Services;

using NLog;
using NLog.Extensions.Logging;



namespace Borealis.Drivers.Rpi;


public static class Program
{
    private static IConfiguration _configuration;


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
        services.AddOptions();

        // Configuration
        services.Configure<ServerOptions>(_configuration.GetSection(ServerOptions.Name));
        services.Configure<AnimationOptions>(_configuration.GetSection(AnimationOptions.Name));

        // Factories.
        services.AddTransient<LedstripProxyFactory>();
        services.AddTransient<PortalConnectionFactory>();
        services.AddTransient<AnimationPlayerFactory>();

        // Services
        services.AddTransient<SettingsService>();

        // State
        services.AddSingleton<ConnectionContext>();
        services.AddSingleton<LedstripContext>();
        services.AddSingleton<DisplayContext>();

        // Handlers
        services.AddTransient<IQueryHandler<RequestFrameBufferCommand, FrameBufferQuery>, RequestFrameBufferQueryHandler>();
        services.AddTransient<IQueryHandler<ConnectCommand, ConnectedQuery>, ConnectQueryHandler>();
        services.AddTransient<ICommandHandler<SetFrameCommand>, SetFrameCommandHandler>();
        services.AddTransient<ICommandHandler<StartAnimationCommand>, StartAnimationCommandHandler>();
        services.AddTransient<ICommandHandler<StopAnimationCommand>, StopAnimationCommandHandler>();
        services.AddTransient<ICommandHandler<ConfigurationCommand>, SetConfigurationCommandHandler>();

        // Hosting
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

        _configuration = config;
    }
}