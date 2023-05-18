using System;
using System.Linq;

using Borealis.Networking.Connections;
using Borealis.Networking.IO;
using Borealis.Networking.Protocol;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;



namespace Borealis.Networking.DependencyInjection;


public static class NetworkingServiceCollectionExtensions
{
	/// <summary>
	/// Adds the connection options to the configuration.
	/// </summary>
	/// <param name="configuration">
	/// The <see cref="IConnection" /> or
	/// <see cref="IConfigurationSection" /> indicating the configuration of the section.
	/// </param>
	public static void AddNetworkingConnectionOptions(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddOptions<ConnectionOptions>().Bind(configuration);
	}


	/// <summary>
	/// Adds the channel options to the application.
	/// </summary>
	/// <param name="configuration">
	/// The <see cref="IConnection" /> or
	/// <see cref="IConfigurationSection" /> indicating the configuration of the section.
	/// </param>
	public static void AddNetworkingChannelOptions(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddOptions<ChannelOptions>().Bind(configuration);
	}


	/// <summary>
	/// Adds all the networking services to the application
	/// </summary>
	public static void AddNetworkingServices(this IServiceCollection services)
	{
		// Factories
		services.AddTransient<ISocketFactory, SocketFactory>();
		services.AddTransient<ISocketServerFactory, SocketServerFactory>();

		services.AddTransient<IConnectionFactory, ConnectionFactory>();
		services.AddTransient<IChannelFactory, ChannelFactory>();
	}
}