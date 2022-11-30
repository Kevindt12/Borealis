using System;
using System.Linq;

using Borealis.Portal.Domain.Connections;
using Borealis.Portal.Infrastructure.Connections;

using Microsoft.Extensions.DependencyInjection;



namespace Borealis.Portal.Infrastructure.Extensions;


public static class ServiceCollectionExtensions
{
    public static void AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddTransient<IDeviceConnectionFactory, DeviceConnectionFactory>();
    }
}