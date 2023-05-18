using System.IO.Abstractions;

using Borealis.Drivers.RaspberryPi.Sharp.Animations.Factories;
using Borealis.Drivers.RaspberryPi.Sharp.Animations.Options;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Context;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Factories;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Managers;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Options;
using Borealis.Drivers.RaspberryPi.Sharp.Connection.Services;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Manager;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Providers;
using Borealis.Drivers.RaspberryPi.Sharp.Device.Validation;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Contexts;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Factories;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Providers;
using Borealis.Drivers.RaspberryPi.Sharp.Ledstrips.Service;
using Borealis.Drivers.RaspberryPi.Sharp.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NLog;
using NLog.Extensions.Logging;



namespace Borealis.Drivers.RaspberryPi.Sharp;


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
		services.AddSingleton<IFileSystem, FileSystem>();

		//// Configuration
		services.Configure<ServerOptions>(_configuration.GetSection("Server"));
		services.Configure<PathOptions>(_configuration.GetSection("Paths"));
		services.Configure<AnimationOptions>(_configuration.GetSection("Animation"));

		// Validation
		services.AddTransient<IDeviceConfigurationValidator, DeviceConfigurationValidator>();

		// State
		services.AddSingleton<ConnectionContext>();
		services.AddSingleton<LedstripContext>();

		// Providers
		services.AddSingleton<ILedstripSettingsProvider, LedstripSettingsProvider>();
		services.AddSingleton<IBusSettingsProvider, BusSettingsProvider>();

		//// Factories.
		services.AddTransient<ILedstripProxyFactory, LedstripProxyFactory>();
		services.AddTransient<AnimationPlayerFactory>();
		services.AddTransient<PortalConnectionFactory>();
		services.AddTransient<ConnectionControllerFactory>();
		services.AddTransient<LedstripStateFactory>();
		services.AddTransient<ServerFactory>();

		// Managers
		services.AddTransient<IDeviceConfigurationManager, DeviceConfigurationManager>();
		services.AddTransient<IConnectionManager, ConnectionManager>();

		//// Services
		services.AddTransient<IConnectionService, ConnectionService>();
		services.AddTransient<ILedstripControlService, LedstripService>();
		services.AddTransient<ILedstripConfigurationService, LedstripService>();
		services.AddTransient<ILedstripService, LedstripService>();

		// Hosting
		services.AddHostedService<LedstripHostedService>();
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
									   .AddJsonFile("ledstripsettings.json", true, true)
									   .Build();

		_configuration = config;
	}
}