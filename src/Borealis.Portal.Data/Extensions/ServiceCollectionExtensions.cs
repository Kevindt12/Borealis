using System;
using System.Linq;

using Borealis.Portal.Data.Contexts;

using Microsoft.Extensions.DependencyInjection;



namespace Borealis.Portal.Data.Extensions;


public static class ServiceCollectionExtensions
{
	public static void AddDataServices(this IServiceCollection services)
	{
		services.AddDbContext<ApplicationDbContext>();
	}
}