using System;
using System.Linq;

using Borealis.Portal.Domain.Connectivity.Factories;
using Borealis.Portal.Infrastructure.Connectivity.Factories;
using Borealis.Portal.Infrastructure.Connectivity.Options;
using Borealis.Portal.Infrastructure.Connectivity.Serialization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;



namespace Borealis.Portal.Infrastructure.DependencyInjection;


public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds the infrastructure options to the application.
	/// </summary>
	/// <param name="services"> </param>
	/// <param name="configuration"> </param>
	public static void AddInfrastructureOptions(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddOptions<ConnectivityOptions>().Bind(configuration.GetSection(ConnectivityOptions.Name));
		services.AddOptions<KeepAliveOptions>().Bind(configuration.GetSection(KeepAliveOptions.Name));
	}


	/// <summary>
	/// Adds the infrastructure servers that are needed.
	/// </summary>
	/// <param name="services"> </param>
	public static void AddInfrastructureServices(this IServiceCollection services)
	{
		// Serialization 
		services.AddTransient<MessageSerializer>();

		// Factories
		services.AddTransient<CommunicationHandlerFactory>();
		services.AddTransient<LedstripConnectionFactory>();
		services.AddTransient<DeviceConnectionFactory>();
		services.AddTransient<IDeviceConnectionFactory, DeviceConnectionFactory>();
	}
}