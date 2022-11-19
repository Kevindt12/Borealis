using Borealis.Portal.Domain.Devices;
using Borealis.Portal.Domain.Effects;

using Microsoft.Extensions.DependencyInjection;



namespace Borealis.Portal.Domain.Extensions;


public static class ServiceCollectionExtensions
{
	public static void AddDomainServices(this IServiceCollection services)
	{
		services.AddTransient<IDeviceFactory, DeviceFactory>();
		services.AddTransient<IEffectFactory, EffectFactory>();
	}
}